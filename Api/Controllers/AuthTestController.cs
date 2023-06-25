using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/AuthTest")]
[ApiController]
public class AuthTestController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<string>> GetSomething()
    {
        return "You are authenticated";
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<string>> GetSomething(int someIntValue)
    {
        return "You are authorized with Role of Admin";
    }
}