using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SalaryReview.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SalaryReview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration;

        public LoginController(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        private Users AuthenticationUser(Users users)
        {
            Users _user = null;
            if(users.userName == "admin" &&  users.password == "admin")
            {
                _user = new Users { userName = "Parthib Banik" }; 
            }
            return _user;
        }

        private string GenerateToken(Users users)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], null, 
                expires: DateTime.Now.AddMinutes(1),signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(Users user)
        {
            IActionResult actionResult = Unauthorized();
            var _user = AuthenticationUser(user);
            if (_user != null) 
            {
                var token = GenerateToken(_user);
                actionResult = Ok(new {token = token});
            }
            return actionResult;

        }
    }
}
