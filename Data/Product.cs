using System.Data.Common;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using RealApi.Models;
using RealApi.Iauditables;
using Microsoft.VisualBasic;
using FluentValidation;
using System.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using twoPizdec;
namespace RealApi.Data
{
    
}

    public class AddDb : DbContext 
    {
        public DbSet<User> users {get;set;}
        public DbSet<Product> products {get;set;}
        public DbSet<Order> orders {get;set;}
        public DbSet<OrderItem> orderItems {get;set;}
        public DbSet<CartItem> cartitems {get;set;}
        public DbSet<Review> reviews {get;set;}
        public DbSet<Category> categories {get;set;}
        public DbSet<OrderYes> twoPizdecs {get;set;}
        public DbSet<Check> Checks {get;set;}
        public async Task<User?> GetUserEverywhere(int id)
    {
        return await users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
    }
    public async Task<Product> GetProductEverywhere(int productid)
    {
        return await products.FindAsync(productid);
    }
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("data source=add.db");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
    {
        entity.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);
        entity.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
    });

    modelBuilder.Entity<Category>(entity =>
    {
        entity.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);
    });
    modelBuilder.Entity<User>(entity =>
    {
        entity.HasIndex(c=>c.Password).IsUnique();
        entity.HasIndex(c => c.Login).IsUnique();
    });
    modelBuilder.Entity<Category>().HasMany(u => u.products).WithOne(u => u.Category).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<Product>().HasQueryFilter(u => !u.isDeleted);
    modelBuilder.Entity<User>().HasQueryFilter(u => !u.isDeleted);
    modelBuilder.Entity<UserDto>().HasQueryFilter(u => !u.IsDeleted);
    modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();
    modelBuilder.Entity<User>().HasQueryFilter(u => !u.Isblocked);
    modelBuilder.Entity<UserDto>().HasQueryFilter(u => !u.IsBlocked);
    modelBuilder.Entity<Product>().HasIndex(u=>u.Name);
    modelBuilder.Entity<User>().HasIndex(u=>u.Id);
    }
        public class JwtOptions
    {
        public const string name = "jwt";
        [Required]
        [MinLength(32,ErrorMessage = "password to short"),MaxLength(500, ErrorMessage = "password to very big!")]
        public string key {get;set;} = string.Empty;
    }
        public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(u=>u.Password).NotEmpty().MinimumLength(10).WithMessage("the password cannot be empty");
            RuleFor(u => u.Login).NotEmpty().MinimumLength(5).WithMessage("login too short");
        }
    }
[ApiController]
[Route("api/[controller]")] //ОБЯЗАТЕЛЬНО НУЖНО ПОНЯТЬ МНЕ!!! / 100 % 
public class AuthController : ControllerBase
    {
        private readonly IValidator<User> validator;
        public AuthController(IValidator<User> _validator)
        {
            validator = _validator;
        }
        [HttpPost("Register")]
        public IActionResult Register (User user)
        {
            var validations = validator.Validate(user);
            if (!validations.IsValid)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
    }
