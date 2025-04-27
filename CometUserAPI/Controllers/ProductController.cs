using CometUserAPI.Entities;
using CometUserAPI.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace CometUserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment env;
        private readonly CometUserDBContext _dbContext;
        public ProductController(IWebHostEnvironment environment, CometUserDBContext dbContext) {
            this.env = environment;
            this._dbContext = dbContext;
        }

        [HttpPut("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile formFile, string productCode)
        {
            APIResponse response = new APIResponse();
            try
            {
                string filePath = this.GetFilePath(productCode);
                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }

                string imagepath = filePath + "\\" + productCode + ".png";
                if (!System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                }
                using (FileStream stream = System.IO.File.Create(imagepath))
                {
                    await formFile.CopyToAsync(stream);
                    response.ResponseCode = 200;
                    response.Result = "pass";
                }
            }
            catch (Exception ex)
            {
                response.Message=ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("MultiUploadImage")]
        public async Task<IActionResult> MultiUploadImage(IFormFileCollection fileCollection, string productCode)
        {
            APIResponse response = new APIResponse();
            int passCount = 0;
            int errorCount = 0;
            try
            {
                string filePath = this.GetFilePath(productCode);
                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }
                foreach (var file in fileCollection)
                {
                    string imagePath = filePath + "\\" + file.FileName;
                    if (!System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    using (FileStream stream = System.IO.File.Create(imagePath))
                    {
                        await file.CopyToAsync(stream);
                        passCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                response.Message = ex.Message;
            }
            response.ResponseCode = 200;
            response.Result = passCount + "Files uploaded &" + errorCount + "files failed";

            return Ok(response);
        }

        [HttpPut("DBUploadMultipleImages")]
        public async Task<IActionResult> DBUploadMultipleImages(IFormFileCollection fileCollection, string productCode)
        {
            APIResponse response = new APIResponse();
            int passCount = 0;
            int errorCount = 0;
            try
            {
                foreach (var file in fileCollection)
                {
                    using(MemoryStream stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        this._dbContext.TblProductimages.Add(new TblProductimage()
                        {
                            Productcode = productCode,
                            Productimage = stream.ToArray()
                        });
                        await this._dbContext.SaveChangesAsync();
                        passCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                response.Message = ex.Message;
            }
            response.ResponseCode = 200;
            response.Result = passCount + "Files uploaded &" + errorCount + "files failed";

            return Ok(response);
        }

        [HttpGet("GetImage")]
        public async Task<IActionResult> GetImage(string productCode)
        {
            string ImageUrl = string.Empty;
            string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilePath(productCode);
                string imagePath = Filepath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    ImageUrl = hostUrl + "/upload/product/" + productCode + "/" + productCode + ".png";
                } 
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {

            }
            return Ok(ImageUrl);
        }

        [HttpGet("download")]
        public async Task<IActionResult> download(string productCode)
        {
            try
            {
                string Filepath = GetFilePath(productCode);
                string imagePath = Filepath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    MemoryStream stream = new MemoryStream();
                    using (FileStream fileStream = new FileStream(imagePath, FileMode.Open))
                    {
                        await fileStream.CopyToAsync(stream);
                    }
                    stream.Position = 0;
                    return File(stream, "image/png", productCode + ".png");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet("remove")]
        public async Task<IActionResult> remove(string productCode)
        {
            try
            {
                string Filepath = GetFilePath(productCode);
                string imagePath = Filepath + "\\" + productCode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                    return Ok("pass");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet("removeMultiple")]
        public async Task<IActionResult> removeMultiple(string productCode)
        {
            List<string> ImageUrl = new List<string>();
            string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilePath(productCode);

                if (System.IO.Directory.Exists(Filepath))
                {
                    DirectoryInfo DirectoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = DirectoryInfo.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        fileInfo.Delete();
                    }
                    return Ok("pass");

                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
            return Ok(ImageUrl);
        }

        [HttpGet("GetMultipleImages")]
        public async Task<IActionResult> GetMultipleImages(string productCode)
        {
            List<string> ImageUrl = new List<string>();
            string hostUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string Filepath = GetFilePath(productCode);

                if (System.IO.Directory.Exists(Filepath))
                {
                    DirectoryInfo DirectoryInfo = new DirectoryInfo(Filepath);
                    FileInfo[] fileInfos = DirectoryInfo.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        string filename = fileInfo.Name;
                        string imagePath = Filepath + "\\" + filename;
                        if (System.IO.File.Exists(imagePath))
                        {
                            string _imageUrl = hostUrl + "/upload/product/" + productCode + "/" + filename;
                            ImageUrl.Add(_imageUrl);
                        }
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {

            }
            return Ok(ImageUrl);
        }

        [HttpGet("GetDBMultipleImages")]
        public async Task<IActionResult> GetDBMultipleImages(string productCode)
        {
            List<string> ImageUrl = new List<string>();
            try
            {
                var _productImage = this._dbContext.TblProductimages.Where(item => item.Productcode == productCode).ToList();
                if(_productImage != null && _productImage.Count > 0) 
                {
                    _productImage.ForEach(item =>
                    {
                        ImageUrl.Add(Convert.ToBase64String(item.Productimage));
                    });

                    string Filepath = GetFilePath(productCode);
                    string imagePath = Filepath + "\\" + productCode + ".png";
                    if (System.IO.File.Exists(imagePath))
                    {
                        MemoryStream stream = new MemoryStream();
                        using (FileStream fileStream = new FileStream(imagePath, FileMode.Open))
                        {
                            await fileStream.CopyToAsync(stream);
                        }
                        stream.Position = 0;
                        return File(stream, "image/png", productCode + ".png");
                    }
                    else
                    {
                        return NotFound();
                    }
                } else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {

            }
            return Ok(ImageUrl);
        }

        [HttpGet("dbDoownload")]
        public async Task<IActionResult> dbDoownload(string productCode)
        {
            try
            {
                var _productImage = await this._dbContext.TblProductimages.FirstOrDefaultAsync(item => item.Productcode == productCode);
                if (_productImage != null)
                {
                    return File(_productImage.Productimage, "image/png", productCode + ".png");
                }

                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {

            }
            return Ok("downlaod");
        }

        private string GetFilePath(string productCode)
        {
            return this.env.WebRootPath + "\\Upload\\\\product\\" + productCode;
        }
    }
}
