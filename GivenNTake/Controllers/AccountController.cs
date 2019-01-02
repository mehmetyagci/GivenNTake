using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GivenNTake.Model;
using GivenNTake.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GivenNTake.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO registration)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            var user = await _userManager.FindByEmailAsync(registration.Email);
            if (user != null)
            {
                SerializableError serializableError =
                    new SerializableError { { nameof(registration.Email), "Email already exist in the system" } };
                return this.StatusCode((int)HttpStatusCode.Conflict, serializableError);
            }

            User newUser = new User
            {
                Email = registration.Email,
                UserName = registration.Email,
                Id = registration.Email,
            };
            var result = await _userManager.CreateAsync(newUser, registration.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            user = await _userManager.FindByEmailAsync(registration.Email);
            await _userManager.AddClaimAsync(user,
                new Claim("registration-date", DateTime.UtcNow.ToString("yy-MM-dd")));
            if (registration.Email.Contains("admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<SuccessfulLoginResult>> Login([FromBody] LoginUserDTO login)
        {
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            User user = await _userManager.FindByEmailAsync(login.Email);
            JwtSecurityToken token = await GenerateTokenAsync(user);
            // defined
            string serializedToken = new JwtSecurityTokenHandler().WriteToken(token); // serialize the token
            return Ok(new SuccessfulLoginResult() { Token = serializedToken });
        }

        private async Task<JwtSecurityToken> GenerateTokenAsync(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var expirationDays = _configuration.GetValue<int>("JWTConfiguration:TokenExpirationDays");
            var siginingKey = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWTConfiguration:SigningKey"));
            var token = new JwtSecurityToken
            (
                issuer: _configuration.GetValue<string>("JWTConfiguration:Issuer"),
                audience: _configuration.GetValue<string>("JWTConfiguration:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(expirationDays)),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(siginingKey),
                    SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        [Authorize]
        [HttpGet("Email")]
        public ActionResult<string> GetEmail()
        {
            return Ok(User.Identity.Name);
        }
    }
}