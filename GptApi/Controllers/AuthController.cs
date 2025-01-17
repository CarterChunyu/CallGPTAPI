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
        public async Task<ActionResult> GetToken(User userinfo)
        {
            var User = await _context.Users.FirstOrDefaultAsync
                (x => x.UserName == userinfo.UserName && x.UserPwd == userinfo.UserPwd);
            if (User == null)
                return BadRequest("帳號或密碼錯誤");
            var role = User.Cnt <= 2000 ? "GptTester" : "Customer";
            var token = JwtHelper.GenerateJsonWebToken(userinfo, role);
            return Ok(token);
        }
    }
}
