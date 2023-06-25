using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

public class ShoppingCart
{
    public int Id { get; set; }
    public string UserId { get; set; }

    public ICollection<CartItem> CartItems { get; set; }

    [NotMapped]
    public double CartTotal { get; set; }
}