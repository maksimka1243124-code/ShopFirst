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
[Route("api/[controller]")]
public class AuthController(AddDb db, IConfiguration config) : ControllerBase
{
    private readonly AddDb db;
    private readonly IValidator<User> _validator;
    [HttpPost("Register")]
    public async Task<IActionResult> Register(User user)
    {
        var us = await db.users.AnyAsync(u => u.Login == user.Login);
        if (us) return BadRequest("Юзер уже есть");
        var password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        var used = new User
        {
            Login = user.Login,
            Password = password,
            Datejoin = DateTime.UtcNow,
        };
        db.users.Add(used);
        await db.SaveChangesAsync();
        return Ok("пользователь зарегестрирован!");
        
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(User user,string newpassword,string oldpassword,IConfiguration config)
    {
        var userb = db.users.FirstOrDefault(u => u.Login == user.Login);
        if (userb is not null) return BadRequest("Пользователя не существует");
        bool pass = BCrypt.Net.BCrypt.Verify(newpassword, user.Password);
        if (!pass) return Unauthorized();
        var token = CreateToken(user);
        return Ok(new UserDto
        {
            Login = user.Login,
            Datejoin = DateTime.UtcNow
        });
    }
    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("Jwt:Key").Value!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    [HttpPut("Personalcabinet/restore-password")]
    public async Task<IActionResult> RestorePass(User user,String newPassword,string oldpassword)
    {
        var us = await db.users.FirstOrDefaultAsync(u => u.Login == user.Login);
        if (us is null) return NotFound("Не найдено!");
        bool pass = BCrypt.Net.BCrypt.Verify(oldpassword,user.Password);
        if (!pass) return BadRequest("не правильный пароль!");
        var passnewhash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        us.Password = passnewhash;
        await db.SaveChangesAsync();
        return Ok("successfully!");
    }
}
