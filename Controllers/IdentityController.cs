﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Template.ViewModels;

namespace Template.Controllers
{
    /// <summary>
    /// Controller for handling user authentication (Register, Login, Logout, and JWT generation).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor to initialize IdentityController with dependency injection.
        /// </summary>
        public IdentityController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        /// <summary>
        /// Registers a new user with email and password.
        /// </summary>
        /// <param name="model">User registration details.</param>
        /// <returns>Returns success or failure response.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // MailAddress.User is used to take the username part of the email.
            var user = new IdentityUser { UserName = new MailAddress(model.Email).User, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var studentRole = await _userManager.AddToRoleAsync(user, "Student");
            if (!studentRole.Succeeded)
                return BadRequest(studentRole.Errors);

            return Ok(new { Message = "User registered successfully!" });
        }

        /// <summary>
        /// Logs in a user and generates a JWT token.
        /// </summary>
        /// <param name="model">User login credentials.</param>
        /// <returns>JWT Token on successful authentication.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Determine if the input is an email
            bool isEmail = new EmailAddressAttribute().IsValid(model.Email);

            // Retrieve the user using email or username
            var user = isEmail
                ? await _userManager.FindByEmailAsync(model.Email)
                : await _userManager.FindByNameAsync(model.Email);

            if (user == null)
                return Unauthorized(new { Message = "Invalid credentials" });

            // Sign in the user with Email or username
            //var username = new EmailAddressAttribute().IsValid(model.Email) ? new MailAddress(model.Email).User : model.Email;


            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized(new { Message = "Invalid credentials" });

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        /// <summary>
        /// Logs out the authenticated user.
        /// </summary>
        /// <returns>Success message.</returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Logged out successfully!" });
        }

        /// <summary>
        /// Generates a JWT token for the authenticated user.
        /// </summary>
        /// <param name="user">Authenticated IdentityUser.</param>
        /// <returns>JWT Token as a string.</returns>
        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                }),
                NotBefore = now,
                Expires = now.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
