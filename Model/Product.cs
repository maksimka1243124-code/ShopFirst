using System.Net;
using System.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Net.Http.Headers;
using RealApi.Data;
using FluentValidation;
using RealApi.Iauditables;
using System.Diagnostics.Eventing.Reader;
using RealApiGeneric;
using System.Security.Claims;
using Microsoft.VisualBasic;
using System.Net.Sockets;
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
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Routing;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;

namespace RealApi.Models
{
    public class Ip : IidDelete
    {
        public HashSet<string> ipadress = new() {"127.0.0.0.1"};
        public bool IsDelete(string ip)
        {
            var dev = ipadress.Contains(ip);
            return dev;
        }
    }
    public interface IidDelete
    {
        bool IsDelete(string ip);
    }
    public class Check : Checked
    {
        public bool Ischeck(HttpContext http,string userid)
        {
           string realId = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
           if (string.IsNullOrEmpty(realId)) throw new Exception("nothing");
           return realId == userid;
        }
    }
    public interface Checked
    {
        bool Ischeck(HttpContext http,string userid);
    }
    public class User
    {
        public int Id {get;set;}
        public string Login {get;set;}
        public string Role {get;set;}
        public string Name {get;set;}
        public List<Review> reviews {get;set;}
        public bool isDeleted {get;set;}
        public bool Isblocked {get;set;}
        public string Password {get;set;}
        public DateTime Datejoin {get;set;}
    }
    public class UserDto
    {
        public int Id {get;set;}
        public string Login {get;set;}
        public string Role {get;set;}
        public string Name {get;set;}
        public bool IsDeleted {get;set;}
        public bool IsBlocked {get;set;}
        public DateTime Datejoin {get;set;}
        public string HashPassword {get;set;}
        public string Token { get; set; }
    }
        public class Product : Iauditable
    {
        public int Id { get; set; }
        public int CategoryId {get;set;}
        public DateTime CreateDate { get; set; }
        public bool isDeleted {get;set;}
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Category Category {get;set;}
        public bool IsHidden {get;set;}
        public int StockCount {get;set;}
    }
    public class Category
    {
        public int id {get;set;}
        public string Name {get;set;}
        public List<Product> products {get;set;} = new List<Product>();
        public Product producted {get;set;}
    }
    public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}
    public class ProductDTO
    {
        public int Id {get;set;}
        public string Name {get;set;}
        public decimal Price {get;set;}
        public int CategoryId {get;set;}
        public Category Category {get;set;}
        public bool IsAvailable {get;set;}
        public int StockCount {get;set;}

    }
    public class CartItem
    {
        public int Id {get;set;}
        public int ProductId {get;set;}
        public Product Product {get;set;}
        public int Quantity {get;set;}
        public int UserId {get;set;}
        public User User {get;set;}
    }
    public class Order
    {
        public int Id {get;set;}
        public int UserId {get;set;}
        public decimal Totalprice {get;set;}
        public User User {get;set;}
        public DateTime OrderTime {get;set;}
        public string Status {get;set;}
    }
    public class OrderReponseDto
    {
        public int OrderId {get;set;}
        public decimal TotalPrice {get;set;}
        public DateTime dateTime {get;set;}
        public string Status {get;set;}
        public List<string> summary {get;set;}
    }
    public class OrderItem
    {
        public int Id {get;set;}
        public Order Order {get;set;}
        public int OrderId {get;set;}
        public int ProductId {get;set;}
        public decimal Price {get;set;}
        public int Quantity {get;set;}
    }
    public class Review
    {
        public int Id {get;set;}
        public int ProductId {get;set;}
        public string Comment {get;set;}
        public int Rating {get;set;}
        public DateTime Date {get;set;}
        public int UserId {get;set;}
        public User user {get;set;}
        public Product product {get;set;}
    }
}