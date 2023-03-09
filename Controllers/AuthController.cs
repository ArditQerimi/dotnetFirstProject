using dotnetAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using dotnetAPI.Dtos;
using Microsoft.AspNetCore.Identity;
using Azure;

namespace dotnetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
            {

                var authClaims = new List<Claim>
                {
                    new Claim("Email", user.Email),
                    new Claim("UserName", user.UserName),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };



                var token = GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            var userExists = await _userManager.FindByNameAsync(register.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            User user = new()
            {
                Email = register.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = register.FirstName,
                LastName = register.LastName,
                UserName = register.UserName
            };
            var result = await _userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }





        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }


        //[HttpPost]
        //[Route("login")]
        //public async Task<IActionResult> Login([FromBody] LoginDto login)
        //{
        //    var user = await _userManager.FindByNameAsync(login.Email);
        //    if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
        //    {
        //        var userRoles = await _userManager.GetRolesAsync(user);

        //        var authClaims = new List<Claim>
        //        {
        //            new Claim(ClaimTypes.Email, user.Email),
        //            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //        };

        //        //foreach (var userRole in userRoles)
        //        //{
        //        //    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        //        //}

        //        var token = GetToken(authClaims);

        //        return Ok(new
        //        {
        //            token = new JwtSecurityTokenHandler().WriteToken(token),
        //            expiration = token.ValidTo
        //        });
        //    }
        //    return Unauthorized();
        //}


        //[HttpPost]
        //[Route("register")]
        //public async Task<IActionResult> Register([FromBody] RegisterDto register)
        //{
        //    var userExists = await _userManager.FindByNameAsync(register.Email);
        //    if (userExists != null)
        //        return StatusCode(StatusCodes.Status500InternalServerError);

        //    User user = new()
        //    {
        //        Email = register.Email,
        //        //SecurityStamp = Guid.NewGuid().ToString(),
        //        Password=register.Password,
        //        FirstName =  register.FirstName,
        //        LastName =  register.LastName,
        //    };
        //    var result = await _userManager.CreateAsync(user, register.Password);
        //    if (!result.Succeeded)
        //        return StatusCode(StatusCodes.Status500InternalServerError);

        //    return Ok();
        //}

        //private JwtSecurityToken GetToken(List<Claim> authClaims)
        //{
        //    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["JWT:ValidIssuer"],
        //        audience: _configuration["JWT:ValidAudience"],
        //        expires: DateTime.Now.AddHours(3),
        //        claims: authClaims,
        //        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        //        );

        //    return token;
        //}

        // private AppDbContext appDbContext;

        // public AuthController(UserManager<IdentityUser> userManager, AppDbContext context)
        // {
        //     this.appDbContext = context;

        // }
        // [HttpPost("Register")]
        // public async Task<IActionResult> Register([FromBody] RegisterDto register)
        // {
        //     // Generate a random salt
        //     byte[] salt = new byte[16];
        //     using (var rng = new RNGCryptoServiceProvider())
        //     {
        //         rng.GetBytes(salt);
        //     }

        //     // Hash the password with the salt
        //     string hashedPassword = HashPassword(register.Password, salt);

        //     // Create a new User instance
        //     var user = new User
        //     {
        //         Password = hashedPassword,
        //         Email = register.Email,
        //         FirstName = register.FirstName,
        //         LastName = register.LastName,

        //     };

        //     // Save the user to the database
        //     await appDbContext.Users.AddAsync(user);
        //     await appDbContext.SaveChangesAsync();

        //     // Generate a JWT token for the user
        //     string token = GenerateJwtToken(user);

        //     // Return the token in the response
        //     return Ok(new { Token = token });
        // }


        // private string HashPassword(string password, byte[] salt)
        // {
        //     using var sha256 = SHA256.Create();
        //     var saltedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
        //     var hashedPassword = sha256.ComputeHash(saltedPassword);
        //     return Convert.ToBase64String(hashedPassword);
        // }

        // private string GenerateJwtToken(User user)
        // {
        //     var key = new byte[16]; 
        //     using (var generator = RandomNumberGenerator.Create())
        //     {
        //         generator.GetBytes(key);
        //     }
        //     var tokenDescriptor = new SecurityTokenDescriptor
        //     {
        //         Subject = new ClaimsIdentity(new[]
        //{
        //     new Claim("Email", user?.Email),
        //     new Claim("FirstName", user?.FirstName),
        //     new Claim("LastName", user?.LastName),

        // }),
        //         Expires = DateTime.UtcNow.AddDays(7),
        //         SigningCredentials = new SigningCredentials(
        //    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //     };

        //     var tokenHandler = new JwtSecurityTokenHandler();
        //     var token = tokenHandler.CreateToken(tokenDescriptor);
        //     string jwtToken = tokenHandler.WriteToken(token);

        //     return jwtToken;
        // }





    }
}


