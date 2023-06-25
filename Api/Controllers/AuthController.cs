using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Api.Data;
using Api.Models;
using Api.Models.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private ApiResponse _response;
    private string secretKey;

    public AuthController(
        ApplicationDbContext db,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _response = new ApiResponse();
        secretKey = configuration.GetValue<string>("ApiSettings:Secret");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody]RegisterRequestDto model)
    {
        var userFromDb = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == model.UserName);

        if (userFromDb != null)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Username already exists");
            return BadRequest(_response);
        }

        var newUser = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.UserName,
            NormalizedEmail = model.UserName.ToUpper(),
            Name = model.Name
        };

        try
        {
            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole("admin"));
                    await _roleManager.CreateAsync(new IdentityRole("user"));
                }

                if (model.Role.ToLower() == "admin")
                {
                    await _userManager.AddToRoleAsync(newUser, "admin");
                }
                else
                {
                    await _userManager.AddToRoleAsync(newUser, "user");
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        _response.StatusCode = HttpStatusCode.BadRequest;
        _response.IsSuccess = false;
        _response.ErrorMessages.Add("Error while registering");
        return BadRequest(_response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]LoginRequestDto model)
    {
        var userFromDb = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == model.UserName);

        var isValid = await _userManager.CheckPasswordAsync(userFromDb, model.Password);

        if (isValid == false)
        {
            _response.Result = new LoginResponseDto();
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Username or password is incorrect");
            return BadRequest(_response);
        }

        var roles = await _userManager.GetRolesAsync(userFromDb);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("fullName", userFromDb.Name),
                new Claim("id", userFromDb.Id.ToString()),
                new Claim(ClaimTypes.Email, userFromDb.UserName.ToString()),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var loginResponse = new LoginResponseDto
        {
            Email = userFromDb.Email,
            Token = tokenHandler.WriteToken(token)
        };

        if (loginResponse.Email == null || string.IsNullOrEmpty(loginResponse.Token))
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Username or password is incorrect");
            return BadRequest(_response);
        }

        _response.StatusCode = HttpStatusCode.OK;
        _response.IsSuccess = true;
        _response.Result = loginResponse;
        return Ok(_response);
    }
}