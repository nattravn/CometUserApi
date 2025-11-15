using AutoMapper;
using CometUserAPI.Entities;
using CometUserAPI.Helper;
using CometUserAPI.Model;
using CometUserAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CometUserAPI.Container
{
    public class CustomerService : ICustomerService
    {
        private readonly CometUserDBContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;
        public CustomerService(CometUserDBContext context, IMapper mapper, ILogger<CustomerService> logger)
        {
            this._context = context;
            this._mapper = mapper;
            this._logger = logger;
        }

        public async Task<APIResponse> Create(CustomerModel data)
        {
            APIResponse response = new APIResponse();
            try
            {
                this._logger.LogInformation("Create begins");
                TblCustomer _customer = this._mapper.Map<CustomerModel, TblCustomer>(data);
                await this._context.TblCustomers.AddAsync(_customer);
                await this._context.SaveChangesAsync();
                response.ResponseCode = 201;
                response.Result= "pass";
            }
            catch (Exception ex) 
            {
                response.ResponseCode= 400;
                response.Result= ex.Message;
                this._logger.LogError(ex.Message, ex);
            }
            return response;
        }

        public async Task<List<CustomerModel>> GetAll()
        {
            List<CustomerModel> _response = new List<CustomerModel>();
            var _data = await this._context.TblCustomers.ToListAsync();
            if(_data != null)
            {
                _response = this._mapper.Map<List<TblCustomer>, List<CustomerModel>>(_data);
            }
            return _response;
        }

        public async Task<CustomerModel> GetByCode(string code)
        {
            CustomerModel _response = new CustomerModel();
            var _data = await this._context.TblCustomers.FindAsync(code);
            if (_data != null)
            {
                _response = this._mapper.Map<TblCustomer, CustomerModel>(_data);
            }
            return _response;
        }

        public async Task<APIResponse> Remove(string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                var _customer = await this._context.TblCustomers.FindAsync(code);
                if(_customer!= null)
                {
                    this._context.TblCustomers.Remove(_customer);
                    await this._context.SaveChangesAsync();
                    response.ResponseCode = 201;
                    response.Result = "pass";
                } else
                {
                    response.ResponseCode = 201;
                    response.Message = "Data not found";
                }
                
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.Result = ex.Message;
            }
            return response;
        }

        public async Task<APIResponse> Update(CustomerModel data, string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                var _customer = await this._context.TblCustomers.FindAsync(code);
                if (_customer != null)
                {
                    _customer.Name = data.Name; 
                    _customer.Email = data.Email;
                    _customer.Phone = data.Phone;
                    _customer.IsActive = data.IsActive;
                    //_customer.CreditLimit = data.CreditLimit;
                    await this._context.SaveChangesAsync();
                    response.ResponseCode = 201;
                    response.Result = "pass";
                }
                else
                {
                    response.ResponseCode = 201;
                    response.Message = "Data not found";
                }

            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.Result = ex.Message;
            }
            return response;
        }
    }
}
