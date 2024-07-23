using AutoMapper;
using CometUserAPI.Entities;
using CometUserAPI.Model;

namespace CometUserAPI.Helper

{
    public class AutoMapperHandler:Profile
    {
        public AutoMapperHandler() {
            CreateMap<TblCustomer, CustomerModel>().ForMember(item => item.StatusName, opt => opt.MapFrom(item => (item.IsActive != null && item.IsActive.Value) ? "Active" : "Inactive")).ReverseMap();  
        }
    }
}
