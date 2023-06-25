using Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MenuItem>()
            .HasData(
                new MenuItem
                {
                    Id = 1,
                    Name = "Spring Roll",
                    Description = "Some Description",
                    Image = "",
                    Price = 7.99,
                    Category = "Appetizer",
                    SpecialTag = ""
                },
                new MenuItem
                {
                    Id = 2,
                    Name = "Fall Roll",
                    Description = "Some Description",
                    Image = "",
                    Price = 4.99,
                    Category = "Appetizer",
                    SpecialTag = ""
                });
    }
}