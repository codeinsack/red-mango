using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShoppingCartController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    protected ApiResponse _response;

    public ShoppingCartController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId,
        int updateQuantityBy)
    {
        var shoppingCart = _db.ShoppingCarts.FirstOrDefault(x => x.UserId == userId);
        var menuItem = _db.MenuItems.FirstOrDefault(x => x.Id == menuItemId);

        if (menuItem == null)
        {
            return BadRequest(_response);
        }

        if (shoppingCart == null && updateQuantityBy == 0)
        {
            var newCart = new ShoppingCart
            {
                UserId = userId
            };
            _db.ShoppingCarts.Add(newCart);
            _db.SaveChanges();

            var newCartItem = new CartItem
            {
                MenuItemId = menuItemId,
                Quantity = updateQuantityBy,
                ShoppingCartId = newCart.Id
            };
            _db.CartItems.Add(newCartItem);
            _db.SaveChanges();
        }

        return _response;
    }
}