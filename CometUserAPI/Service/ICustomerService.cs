using CometUserAPI.Entities;
using CometUserAPI.Helper;
using CometUserAPI.Model;

namespace CometUserAPI.Service
{
    public interface ICustomerService
    {
        Task<List<CustomerModel>> GetAll();
        Task<CustomerModel> GetByCode(string code);
        Task<APIResponse> Remove(string code);
        Task<APIResponse> Create(CustomerModel data);
        Task<APIResponse> Update(CustomerModel data, string code);
    }
}
