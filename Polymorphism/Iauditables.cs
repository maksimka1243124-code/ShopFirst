using RealApi.Models;
using RealApi.Data;
namespace RealApi.Iauditables
{
    public interface Iauditable
    {
        DateTime CreateDate {get;set;}
        bool isDeleted {get;set;}
    }  
}