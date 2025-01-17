using GptApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GptApi.Helpers
{
    public class JwtHelper
    {
        public static string GenerateJsonWebToken(User userInfo, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenParameter.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, userInfo.UserName));
            claimsIdentity.AddClaim(new Claim("PWD", userInfo.UserPwd));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            var token = new JwtSecurityToken(TokenParameter.Issuer,
              TokenParameter.Audience,
              claimsIdentity.Claims,
              expires: DateTime.Now.AddMinutes(TokenParameter.AccessExpiration),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    public class TokenParameter
    {
        public const string Issuer = "padaa";
        public const string Audience = "yu";
        public const string Secret = "11223344556677888877665544332211";      
        public const int AccessExpiration = 60;
    }
    
}
