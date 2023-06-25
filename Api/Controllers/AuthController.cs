using System.Net;
using Api.Data;
using Api.Models;
using Api.Models.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
}