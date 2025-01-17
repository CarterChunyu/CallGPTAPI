using Azure.Core;
using GptApi.Data;
using GptApi.Helpers;
using GptApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Chat;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace GptApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GptController : ControllerBase
    {
        private static Dictionary<string, GptResult> _dic = new Dictionary<string, GptResult>();
        private readonly IConfiguration _config;
        private readonly GptContext _context;

        public GptController(IConfiguration config, GptContext context)
        {
            _config = config;
            _context = context;
        }

        [Authorize(Roles = "GptTester")]
        [HttpPost(Name = "BeginGpt")]
        public async Task<BeginReponse> BeginGpt(BeginRequest request)
        {
            if (string.IsNullOrEmpty(request.Question))
                return new BeginReponse(400, "問題不得為空");

            var key = Guid.NewGuid().ToString();
            var response = new BeginReponse(200, "success", key);

            var claims = HttpContext.User.Claims;
            var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var pwd = claims.FirstOrDefault(x => x.Type == "PWD")?.Value;
            var user = _context.Users.FirstOrDefault(x => x.UserName == name && x.UserPwd == pwd);
            if (user?.Cnt >= 2000)
                return new BeginReponse(400, "你的提問已經達到2000次");
            user.Cnt = user.Cnt + 1;
            _context.Users.Update(user);
            _context.SaveChanges();

            DesCrytoHelper.TryDesDecrypt(_config["GptConstant:apikey"], _config["Des:k"], _config["Des:iv"], out string gptkey);

            var client = new ChatClient
                (_config["GptConstant:modelname"], gptkey);

            _dic[key] = new GptResult { IsComplete = false, Body = "" };
            bool flag = false;
            var stream = client.CompleteChatStreamingAsync(request.Question);
            Task.Run(async () =>
            {
                await foreach (var update in stream)
                {
                    flag = true;
                    foreach (var content in update.ContentUpdate)
                    {
                        _dic[key].Body += content.Text;
                    }
                }
                _dic[key].IsComplete = true;
            }).ContinueWith((task) =>
            {
                if (!task.IsFaulted)
                    return;
                _dic.Remove(key);
                response = new BeginReponse(400, task.Exception?.InnerException?.Message ?? "GPT系統故障");
                flag = true;
            });

            while (!flag)
                Thread.Sleep(5);
            return response;
        }

        [HttpPost(Name = "GetMessage")]
        public GetMessageReponse GetMessage(GetMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.key) || !_dic.ContainsKey(request.key))
                return new GetMessageReponse { StatusCode = 400, Message = "key值有問題" };

            var gptResult = _dic[request.key];
            var response = new GetMessageReponse { StatusCode = 200, Message = "success", IsComplete = gptResult.IsComplete, Body = gptResult.Body };
            return response;
        }

        [HttpGet(Name = "RemoveDictionary")]
        public RemoveDictionaryResponse RemoveDictionary(string key)
        {
            if (string.IsNullOrEmpty(key) || !_dic.ContainsKey(key))
                return new RemoveDictionaryResponse { StatusCode = 400, Message = "key值有問題" };
            _dic.Remove(key);
            return new RemoveDictionaryResponse { StatusCode = 200, Message = "移除成功" };
        }
    }
}
