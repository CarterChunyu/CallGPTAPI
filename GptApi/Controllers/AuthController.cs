using GptApi.Data;
using GptApi.Helpers;
using GptApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GptApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly GptContext _context;

        public AuthController(GptContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<TokenCommonReponse?> GetToken(User userinfo)
        {
            User? user = null;
            TokenCommonReponse? response = null;
            int cnt = 0;
            while (cnt < 2 && user == null)
            {
                try
                {
                    user = await GetUser(userinfo);
                    cnt = 2;
                    response = new TokenCommonReponse() { StatusCode = 400, Message = "帳號密碼錯誤" };
                }
                catch (Exception ex)
                {
                    cnt++;
                    var msg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    response = new TokenCommonReponse() { StatusCode = 403, Message = msg };
                }
            }
            if (user == null)
                return response;
            var role = user?.Cnt <= 2000 ? "GptTester" : "Customer";
            var token = JwtHelper.GenerateJsonWebToken(userinfo, role);
            return new TokenCommonReponse { StatusCode = 200, Message ="success", Token = token };
        }


        private async Task<User?> GetUser(User userinfo)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.UserName == userinfo.UserName && x.UserPwd == userinfo.UserPwd);
        }
    }
}
