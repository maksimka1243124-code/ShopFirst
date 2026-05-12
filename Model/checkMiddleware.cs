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
using System.ComponentModel.DataAnnotations;
using RealApi.Data;
using RealApi.Models;
using RealApi.Iauditables;
using RealApiGeneric;
namespace RealApiCatch;
public class Catch
{
    private readonly RequestDelegate _next;
    public Catch(RequestDelegate next)
    {
        _next = next;
    }
    public async Task Invoke(HttpContext http)
    {
        try
        {
            await _next(http);
        }
        catch(Exception ex)
        {
            await MethodCheck(http,ex);
        }
    }
    private static Task MethodCheck(HttpContext http, Exception ex)
    {
        http.Request.ContentType = "application/json";
        http.Response.StatusCode = 500;
        var response = BaseResponse<object>.Fail(ex.Message);
        return http.Response.WriteAsJsonAsync(response);
    }
}
