
using System.Data;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualBasic;
using RealApi.Data;
using System.Security.Claims;
using RealApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using System.IO.Compression;
using FluentValidation.AspNetCore;
using FluentValidation;
using SQLitePCL;
using System.Transactions;
using Microsoft.AspNetCore.Http.HttpResults;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static AddDb;
using System.ComponentModel.DataAnnotations;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AddDb>();
builder.Services.AddFluentValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator<User>>();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AddDb>();
    if (!db.users.Any(u => u.Login == "admin"))
    {
    db.users.Add(new User { Login = "admin", Password = "passAdmin", Role = "Admin" }); //not real ok
        db.SaveChanges();
    }
}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AddDb>();
    db.Database.EnsureCreated();
    if (!db.products.Any())
    {
        db.products.AddRange
        (
            new Product { Id = 1,Name = "box", Price = 100,CategoryId = 3},
            new Product { Id = 2,Name = "mouse", Price = 100, CategoryId = 1},
            new Product { Id = 3,Name = "milk", Price = 100, CategoryId = 2}
            
        );
        db.SaveChanges();
            }
    if (!db.cartitems.Any())
    {
        var cartItems = new List<CartItem>
{
    new CartItem { UserId = 1, ProductId = 1, Quantity = 2},
    new CartItem { UserId = 1, ProductId = 2, Quantity = 1 },
    new CartItem { UserId = 1, ProductId = 3, Quantity = 3 },
};
    db.cartitems.AddRange(cartItems);
    db.SaveChanges();
    }
    if (!db.categories.Any())
    {
        db.categories.AddRange 
        (
            new Category {id = 1,Name = "electronics"},
            new Category {id = 2,Name = "food"},
            new Category {id = 3,Name = "others"}
        );
        db.SaveChanges();
    }
    }
app.MapPost("/register",async (string password,string login,AddDb db,AuthController auth) =>
{
    string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
    var usered = new User
    {
        Password  = passwordHash,
        Login = login,
        Role = "admin",
        Isblocked = false,
    };
    db.users.Add(usered);
    await db.SaveChangesAsync();
    return Results.Ok(new UserDto
    {
        Id = usered.Id,
        Role = usered.Role,
        IsBlocked = usered.Isblocked,
    });
});
app.MapPut("/Personalcabinet/restore-password",async (string login,string newpassword,string oldpassword, AddDb db) =>
{
    var users = await db.users.FirstOrDefaultAsync(u => u.Login == login);
    if (users is null) return Results.NotFound("user not found");
    bool inconnect = BCrypt.Net.BCrypt.Verify(oldpassword, users.Password);
    if (!inconnect) return Results.BadRequest("not");
    users.Password = BCrypt.Net.BCrypt.HashPassword(newpassword);
    await db.SaveChangesAsync();
    return Results.Ok("password success");
});

app.MapPost("/login",async(string login,string password,AddDb db,IConfiguration config) =>
{
    var loginNow = await db.users.FirstOrDefaultAsync(u => u.Login == login);
    if(loginNow is null) return Results.NotFound();
    bool isValid = BCrypt.Net.BCrypt.Verify(password, loginNow.Password);
    if (!isValid) return Results.Unauthorized();
    var secretkey = config["Jwt:Key"];
    var token = GenerateJwtToken(loginNow.Id.ToString(), loginNow.Role,secretkey);
        return Results.Ok(new UserDto
    {
        Login = loginNow.Login,
        Datejoin = loginNow.Datejoin,
        Token = token,
    });
});
builder.Services.AddOptions<JwtOptions>().Bind(builder.Configuration.GetSection(JwtOptions.name)).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretkeys = builder.Configuration["Jwt:Key"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretkeys)
            )
        };
    });
string GenerateJwtToken(string userId,string role,string secretkey)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
    var code = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        expires: DateTime.Now.AddHours(1),
     signingCredentials: code,
      claims: new[] {
        new Claim(ClaimTypes.Role,role),
        new Claim(ClaimTypes.NameIdentifier, userId)
});
    return new JwtSecurityTokenHandler().WriteToken(token);
}
app.MapGet("/cart{id}",async (int id,AddDb db) =>
{
    var cartget = await db.cartitems.FindAsync(id); 
    if (cartget is null) return Results.NotFound("not found!");
    return Results.Ok(new {message = "Product found!"});
});
app.MapGet("/cart",async (AddDb db) =>
{
    return await db.cartitems.ToListAsync();
});
app.MapDelete("/cart{id}", [Authorize] async (int id, AddDb db,HttpContext https) =>
{
    var userid = int.Parse(https.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    var delete = await db.cartitems.FindAsync(id);
    if (delete is null) return Results.NotFound();
    if (delete.UserId != userid) return Results.Forbid();
    db.cartitems.Remove(delete);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapPost("/cart/add",async (CartItem cartitem,AddDb db) =>
{
    var search = await db.products.FindAsync(cartitem.ProductId);
    if (search is null) return Results.BadRequest("not found!");
    var ad = new CartItem
    {
        ProductId = cartitem.ProductId,
        UserId = cartitem.UserId,
        Quantity = cartitem.Quantity
    };
    db.cartitems.Add(ad);
    await db.SaveChangesAsync();
    return Results.Created($"/cart/{ad.Id}",ad);
});
app.MapGet("/products/search", [Authorize(Roles = "Admin")] async (string? name,int page, int pagesize, AddDb db,HttpContext http) =>
{
    if (page <= 0) page = 1;
    if (pagesize <= 0) pagesize = 10;
    var search = db.products.AsQueryable();
    if(http.User.IsInRole("Admin"))
    {
        search = search.IgnoreQueryFilters();
    }
        if(!string.IsNullOrWhiteSpace(name))
    {
        search = search.Where(u => u.Name.Contains(name));
    }
    var pages = await search.Skip((page-1) * pagesize).Take(pagesize).Select(u => new ProductDTO
    {
        Id = u.Id,
        Name = u.Name,
        Price = u.Price
    }).ToListAsync();
    return Results.Ok(new {Data = pages,Pagesize = pagesize, Page = page,});
});
app.MapGet("/products/{categoryid}",async (int categoryid,AddDb db) =>
{
var search = await db.products.AsNoTracking().Where(u=>!u.IsHidden).Select(u => new ProductDTO
{
    Id = u.Id,
    Price = u.Price,
    IsAvailable = u.StockCount > 0
}).ToListAsync();
    return Results.Ok(search);
    });
app.MapPut("/products/{id}", async (Product producted,int id, AddDb db) => 
{
    var products = await db.products.FindAsync(id);
    if (products is null) return Results.NotFound();
    products.Name = producted.Name;
    products.Price = producted.Price;
    await db.SaveChangesAsync();
    return Results.Ok(products);
});
app.MapPut("/cart/{id}",async (int id,CartItem cartItem, AddDb db) =>
{
    var carts = await db.cartitems.FindAsync(id);
    if (carts is null) return Results.NotFound();
    if (cartItem.Quantity <= 0) {return Results.BadRequest("not null and < 0");}
    carts.Quantity = cartItem.Quantity;
    await db.SaveChangesAsync();
    return Results.Ok(carts);
});
app.MapPost("/products/{id}",[Authorize(Roles = "Admin")] async (int id,AddDb db) =>
{
    var us = await db.products.IgnoreQueryFilters().FirstOrDefaultAsync(u =>u.Id == id);
    if (us is null) return Results.NotFound();
    if(!us.isDeleted)
    {
        return Results.BadRequest("This product is already available");
    }
    us.isDeleted = false;
    await db.SaveChangesAsync();
    return Results.Ok(us);
});
app.MapPost("/order",async (int userid,AddDb db) =>
{

    using var transaction = await db.Database.BeginTransactionAsync();
    try {
    var search = await db.cartitems.Include(u => u.Product).Where(u => u.UserId == userid).ToListAsync();
    if (!search.Any()) return Results.NotFound("cart empty");
    
    var totalprice = search.Sum(u => u.Product.Price * u.Quantity);
    var order = new Order
    {
        Totalprice = totalprice,
        UserId = userid,
        OrderTime = DateTime.Now,
        Status = "Created"
    };
    db.orders.Add(order);
    await db.SaveChangesAsync();
    foreach (var item in search)
    {
        var ordernew = new OrderItem
        {
            OrderId = order.Id,
            Price = item.Product.Price,
            Quantity = item.Quantity,
            ProductId = item.ProductId
        };
        db.orderItems.Add(ordernew);
    }
    db.cartitems.RemoveRange(search);
    await db.SaveChangesAsync();
    var use = new OrderReponseDto
    {
        OrderId = order.Id,
        TotalPrice = order.Totalprice,
        Status = order.Status,
        dateTime = order.OrderTime,
        summary = search.Select(u => $"{u.Product.Id} x{u.Product.Name}x{u.Product.Price}").ToList()
    };
    return Results.Ok(use);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return Results.BadRequest("error by order! " + ex.Message);
    }
});
app.MapGet("/orders", async (int page,int pagesize,int userId,AddDb db) =>
{
    if (page <= 0) page = 1;
    if (pagesize <= 0) pagesize = 10;
    var orders = db.orders.Where(u => u.UserId == userId);
    var haha = await orders.CountAsync();
    var items = await orders.Skip((page-1) * pagesize).Take(pagesize).ToListAsync();
    return Results.Ok(new {haha,items,page,pagesize});
});
app.MapGet ("/products/category", async (decimal? maxprice,int page,int pagesize,AddDb db)=>
{
    if (page <= 0) page =1;
    if(pagesize <= 0) pagesize = 10;
    var usi = db.products.AsQueryable();
    if (maxprice.HasValue)
    {
        usi = usi.Where(u => u.Price <= maxprice);
    }
    var pagess = await usi.Skip((page-1)* pagesize).Take(pagesize).ToListAsync();
    return Results.Ok(new {Page = page, Data = pagess, Pagessize = pagesize});
});
app.MapGet ("/products", async (decimal? minprice,int page,int pagesize,AddDb db)=>
{
    if (page <= 0) page = 1;
    if (pagesize <= 10) pagesize = 10;
    var usi = db.products.AsQueryable();
    if (minprice.HasValue)
    {
        usi = usi.Where(u => u.Price >= minprice);
    }
    var us = await usi.Skip((page-1) * pagesize).Take(pagesize).ToListAsync();
    return Results.Ok(new {Data = us,Page = page, Pagesize = pagesize});
});
app.MapPost("/product/{productid}/review", async (int productid, Review review, AddDb db) =>
{
    var prodidsearch = await db.GetProductEverywhere(productid);
    if (prodidsearch is null) return Results.NotFound();
    review.ProductId = productid;
    review.Date = DateTime.Now;
    db.reviews.Add(review);
    await db.SaveChangesAsync();
    return Results.Created($"/product/{productid}/rewiews/{review.Id}",review);
});
app.MapPut("/product/{productid}/categories/{categoryid}",async (int productid, int categoryid,AddDb db) =>
{
    var prodidsearch = await db.GetProductEverywhere(productid);
    var categoryidsearch = await db.categories.FindAsync(categoryid);
    if (prodidsearch is null || categoryidsearch is null) return Results.NotFound();
    prodidsearch.Category = categoryidsearch;
    await db.SaveChangesAsync();
    return Results.Ok($"Product {prodidsearch.Name} now belongs to {categoryidsearch.Name}");
});
app.MapGet("/product/{productid}/review",async (int productid,int page,int pagesize,AddDb db) => 
{
    if (page <= 0) page = 1;
    if(pagesize <= 0) pagesize = 10;
    var use = await db.reviews.OrderBy(u=>u.Id).AsNoTracking().Where(u => u.ProductId == productid).Skip((page-1)* pagesize).Take(pagesize).ToListAsync();
    return Results.Ok(use);
});
app.MapPut("/user/block/{id}", [Authorize(Roles = "Admin")] async (int id, AddDb db) =>
{
    var us = await db.GetUserEverywhere(id);
    if(us is null) return Results.NotFound();
    if (us.Isblocked) return Results.BadRequest("banned!");
    us.Isblocked = true;
    await db.SaveChangesAsync();
    return Results.Ok("user banned!");
});
app.MapPut("/user/unblock/{id}",[Authorize(Roles = "Admin")] async (int id, AddDb db) =>
{
    var us = await db.GetUserEverywhere(id);
    if (us is null) return Results.NotFound();
    if(us.Isblocked is false) return Results.BadRequest("user blocked");
    us.Isblocked = false;
    await db.SaveChangesAsync();
    return Results.Ok(new {message = "the user has been successfully blocked!"});
});
app.MapGet("/cart/{id}/admin",[Authorize(Roles = "Admin")] () => "Hello admin!");
app.MapGet("/user", [Authorize(Roles = "User")] () => "User hello!");
app.MapGet("/all",[Authorize] () => "Hello, you are authorized");
app.MapDelete("admin/cart/{id}",[Authorize(Roles = "Admin")] async (int id,AddDb db) =>
{
    var use = await db.cartitems.FindAsync(id);
    if (use is null) return Results.NotFound();
    db.cartitems.Remove(use);
    await db.SaveChangesAsync();
    return Results.NoContent();
});