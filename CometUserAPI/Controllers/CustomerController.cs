using ClosedXML.Excel;
using CometUserAPI.Model;
using CometUserAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Data;

namespace CometUserAPI.Controllers
{
    [Authorize]
    //[DisableCors]
    [EnableRateLimiting("fixedWindow")]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CustomerController(ICustomerService customerService, IWebHostEnvironment webHostEnvironment)
        {
            _customerService = customerService;
            _webHostEnvironment = webHostEnvironment;
        }
        //[EnableCors("corspolicy1")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await this._customerService.GetAll();

            if(data == null) 
            { 
                return NotFound();
            }
            return Ok(data);

        }

        [HttpGet("GetByCode")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var data = await this._customerService.GetByCode(code);

            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);

        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CustomerModel _data)
        {
            var data = await this._customerService.Create(_data);
            return Ok(data);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(CustomerModel _data, string code)
        {
            var data = await this._customerService.Update(_data, code);
            return Ok(data);
        }

        [HttpPut("Remove")]
        public async Task<IActionResult> Remove(string code)
        {
            var data = await this._customerService.Remove(code);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("ExportExcel")]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                string filePath = GetFilePath();
                string excelPath = filePath + "\\customer.xlsx";
                DataTable dt = new DataTable();
                dt.Columns.Add("Code", typeof(string));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Email", typeof(string));
                dt.Columns.Add("Phone", typeof(string));
                dt.Columns.Add("CreditLimit", typeof(string));

                var data = await this._customerService.GetAll();

                if (data != null && data.Count > 0)
                {
                    data.ForEach(item => dt.Rows.Add(item.Code, item.Name, item.Email, item.Phone, item.CreditLimit));
                }
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.AddWorksheet(dt, "Customer Info");
                    using(MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        if (System.IO.File.Exists(excelPath))
                        {
                            System.IO.File.Delete(excelPath);
                        }
                        wb.SaveAs(excelPath);

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customer.xlsx");
                    }
                }

            } 
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        private string GetFilePath()
        {
            return this._webHostEnvironment.WebRootPath + "\\export\\";
        }
    }
}
