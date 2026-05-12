using Microsoft.AspNetCore.Mvc;
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
using System.Xml.Linq;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using RealApi.Data;
using RealApi.Models;
using RealApi.Iauditables;
using RealApiGeneric;
using RealApi;
using System.Security.Principal;
namespace twoPizdec;
public class OrderYes
{
    private readonly RequestDelegate _next;
    private readonly Check _check;
    private readonly AddDb _db;
    public OrderYes(RequestDelegate next,Check check)
    {
        _next = next;
        _check = check;
    }
    HashSet<string> apiAdress = new() {"order"};
    DateTime dateTime = DateTime.UtcNow;
    public async Task InvokeAsync(HttpContext http)
    {
        var path = http.Request.Path.Value.ToLower();
        if (http.Request.Method == "POST" && apiAdress.Contains(path))
        {
            var userid = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (_check.Ischeck(http,userid))
            {
                await _next(http);
            }
        }
    }
}