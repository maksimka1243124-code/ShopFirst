using Microsoft.AspNetCore.Mvc;
using RealApi.Models;
using RealApi.Data;
using RealApi.Models;
using RealApi.Iauditables;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.Data;
using System.IO.Compression;
using System.Security.Claims;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Data.Common;
using FluentValidation.AspNetCore;
using SQLitePCL;
using System.Transactions;
using Microsoft.AspNetCore.Http.HttpResults;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Runtime.CompilerServices;
[ApiController]
[Route("api/[ProductController]")]
public class ProductController(AddDb db) : ControllerBase
{
    private readonly AddDb _db;
    private readonly IValidator<User> _validator;
       public (int,int) Getvalidepage(int page, int pagesize)
    {
        if (page <= 0) page = 1;
        if (pagesize <= 0) pagesize = 10;
        return (page,pagesize);
    }
    [HttpGet("/product")]
    public async Task<IActionResult> Product(Product product,int page, int pagesize)
    {
        var (cleanpage, cleanPageSize) = Getvalidepage(page,pagesize);
        var use = await db.products.Skip((cleanpage-1)*cleanPageSize).Take(cleanPageSize).ToListAsync();
        return Ok(use);
    }
    [HttpGet("/product/{productid}")]
    public async Task<IActionResult> ProductSearch(int productid)
    {
        var prodid = await db.products.AsNoTracking().Where(u => u.Id == productid).Select(u => new UserDto
        {
            Name = u.Name,
            Id = u.Id,
              }).ToListAsync();
                if (prodid is null) return NotFound("Продукта не найдено!");
                    return Ok(prodid);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("/product/removed/[ProductController]")]
    public async Task<IActionResult> ProductRemoved(int page, int pagesize)
    {
        var (cleanpage,cleanpagesize) = Getvalidepage(page,pagesize);
        var deleted = await db.products.
        IgnoreQueryFilters().
        AsNoTracking().
        Where(u=> u.isDeleted == true).
        Select(u => new UserDto
        {
            Name = u.Name,
            Id = u.Id,
        }).
        Skip((cleanpage - 1) * cleanpagesize).Take(cleanpagesize).
        ToListAsync();
        if (!deleted.Any()) return NotFound();
        if (deleted.Count == 0) return Ok(new {message = "корзина пуста!"});
        return Ok(deleted);
    }
}