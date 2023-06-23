using System.Net;
using Api.Data;
using Api.Models;
using Api.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/MenuItem")]
[ApiController]
public class MenuItemController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private ApiResponse _response;

    public MenuItemController(ApplicationDbContext db)
    {
        _db = db;
        _response = new ApiResponse();
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuItems()
    {
        _response.Result = _db.MenuItems;
        _response.StatusCode = HttpStatusCode.OK;
        return Ok(_response);
    }

    [HttpGet("{id:int}", Name = "GetMenuItem")]
    public async Task<IActionResult> GetMenuItem(int id)
    {
        if (id == 0)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            return BadRequest(_response);
        }

        var menuItem = _db.MenuItems.FirstOrDefault(x => x.Id == id);

        if (menuItem == null)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.IsSuccess = false;
            return NotFound(_response);
        }

        _response.Result = menuItem;
        _response.StatusCode = HttpStatusCode.OK;
        return Ok(_response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromBody]MenuItemCreateDto menuItemCreateDto)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var menuItemToCreate = new MenuItem()
                {
                    Name = menuItemCreateDto.Name,
                    Category = menuItemCreateDto.Category,
                    Description = menuItemCreateDto.Description,
                    Price = menuItemCreateDto.Price,
                    Image = menuItemCreateDto.Image,
                    SpecialTag = menuItemCreateDto.SpecialTag,
                };

                _db.MenuItems.Add(menuItemToCreate);

                _db.SaveChanges();
                _response.Result = menuItemToCreate;
                _response.StatusCode = HttpStatusCode.OK;
                return CreatedAtRoute("GetMenuItem", new
                {
                    id = menuItemToCreate.Id
                }, _response);
            }
            else
            {
                _response.IsSuccess = false;
            }
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>()
            {
                ex.ToString()
            };
        }

        return _response;
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateMenuItem(
        int id,
        [FromBody]MenuItemUpdateDto menuItemUpdateDto
    )
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (menuItemUpdateDto == null || id == 0)
                {
                    return BadRequest();
                }

                var menuItemFromDb = await _db.MenuItems.FindAsync(id);

                if (menuItemFromDb == null)
                {
                    return BadRequest();
                }

                menuItemFromDb.Name = menuItemUpdateDto.Name;
                menuItemFromDb.Price = menuItemUpdateDto.Price;
                menuItemFromDb.Category = menuItemUpdateDto.Category;
                menuItemFromDb.SpecialTag = menuItemUpdateDto.SpecialTag;
                menuItemFromDb.Description = menuItemUpdateDto.Description;

                _db.MenuItems.Update(menuItemFromDb);

                _db.SaveChanges();
                _response.Result = menuItemFromDb;
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
            }
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>()
            {
                ex.ToString()
            };
        }

        return _response;
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
    {
        try
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var menuItemFromDb = await _db.MenuItems.FindAsync(id);

            if (menuItemFromDb == null)
            {
                return BadRequest();
            }

            _db.MenuItems.Remove(menuItemFromDb);

            _db.SaveChanges();
            _response.Result = menuItemFromDb;
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>()
            {
                ex.ToString()
            };
        }

        return _response;
    }
}