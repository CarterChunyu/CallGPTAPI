using GptApi.Helpers;
using GptApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Chat;
using System.Text;

namespace GptApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GptController : ControllerBase
    {
        private static Dictionary<string, GptResult> _dic = new Dictionary<string, GptResult>();
        private readonly IConfiguration _config;

        public GptController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost(Name = "BeginGpt")]
        public async Task<BeginReponse> BeginGpt(BeginRequest request)
        {
            if (string.IsNullOrEmpty(request.Question))
                return new BeginReponse(400, "問題不得為空");

            var key = Guid.NewGuid().ToString();
            var response = new BeginReponse(200, "success", key);

            DesCrytoHelper.TryDesDecrypt(_config["GptConstant:apikey"], request.DesKey, request.DesIV, out string gptkey);

            var client = new ChatClient
                (_config["GptConstant:modelname"], gptkey);
                        
            _dic[key] = new GptResult { IsComplete = false, Body = "" };
            bool flag = false;            
            var stream = client.CompleteChatStreamingAsync(request.Question);
            await Task.Run(async() =>
            {
                await foreach(var update in stream)
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
                Thread.Sleep(50);                    
            return response;
        }

        [HttpPost(Name = "GetMessage")]
        public GetMessageReponse GetMessage(GetMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.key) || !_dic.ContainsKey(request.key))
                return new GetMessageReponse { StatusCode = 400, Message = "key值有問題" };

            var gptResult = _dic[request.key];
            var response = new GetMessageReponse {StatusCode=200, Message = "success", IsComplete = gptResult.IsComplete, Body = gptResult.Body };
            if (gptResult.IsComplete)
                _dic.Remove(request.key);
            return response;
        }
    }
}
