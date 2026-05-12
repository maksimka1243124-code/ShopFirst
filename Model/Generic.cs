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
using RealApiCatch;
namespace RealApiGeneric;
public class BaseResponse<T>
{
    public List<string> Errors {get;set;} = new();
    public bool Success {get;set;}
    public T Data {get;set;}
    public string Message {get;set;}
    public static BaseResponse<T> Ok(T data, string message = "success") => new() {Data = data, Message =message, Success = true};
    public static BaseResponse<T> Fail(string message = "fail", List<string> errors = null) => new() {Data = default, Errors = errors, Success = false, Message = message};
}