using JwtApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using TestJWT.Data;

namespace JwtApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IConfiguration _config;
        private readonly UserDbContext _context;

        public AccountController(IConfiguration config, UserDbContext context)
        {
            _config = config;
            _context = context;
        }

        private byte[] generateSalt() => RandomNumberGenerator.GetBytes(128 / 8);

        private string HashPassword(string rawPass, byte[] salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: rawPass!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

            return hashed;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            var username = userLogin.Username.Trim();
            var password = userLogin.Password.Trim();

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) || username.Any(Char.IsWhiteSpace))
                return BadRequest("Login Failed, invalid parameters");

            if (username.Length <= 5 || password.Length <= 5)
                return BadRequest("Login Failed, username or password is too short");

            if (!UserExists(username))
            {
                return BadRequest("Login Failed, username does not exist");
            }

            var user = Authenticate(userLogin);

            if (user == null)
                return BadRequest("Login Failed, username or password is incorrect");

            var token = GenerateJWT(user);
            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserLogin userLogin)
        {
            var username = userLogin.Username.Trim();
            var password = userLogin.Password.Trim();

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) || username.Any(Char.IsWhiteSpace))
                return BadRequest("Register Failed, invalid parameters");


            if (username.Length <= 5 || password.Length <= 5)
                return BadRequest("Register Failed, username or password is too short");


            if (UserExists(username))
                return BadRequest("Register Failed, username already taken");

            var salt = generateSalt();
            var newUser = new UserModel
            {
                Username = username,
                Password = HashPassword(password,salt),
                Salt = salt
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private string GenerateJWT(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username)
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddMonths(12),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserModel Authenticate(UserLogin userLogin)
        {
            var hashedPass = HashPassword(userLogin.Password, _context.Users.First(o => o.Username == userLogin.Username).Salt);
            var currentUser = _context.Users.FirstOrDefault(o => o.Username.ToLower() == userLogin.Username.ToLower() && o.Password == hashedPass);
            
            if (currentUser != null)
            {
                return currentUser;
            }

            return null;
        }

        private bool UserExists(string username)
        {
            return _context.Users.Any(e => e.Username.ToLower() == username.ToLower());
        }
    }
}
