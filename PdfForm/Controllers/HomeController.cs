using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfForm.Models;
using PdfForm.ResponseModels;

namespace PdfForm.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<List<PdfFormfields>> Uploader(List<IFormFile> files)
        {
            try
            {
                var filesPath = $"{this._hostingEnvironment.WebRootPath}/files";
                var filepath = string.Empty;
                foreach (var file in files)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName;

                    // Ensure the file name is correct
                    fileName = fileName.Contains("\\")
                        ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)
                        : fileName.Trim('"');

                    var fullFilePath = Path.Combine(filesPath, fileName);
                    filepath = fullFilePath;
                    if (file.Length <= 0)
                    {
                        continue;
                    }

                    using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                }

                return PdfHelper.ReadPdfFormfieldses(filepath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public IActionResult PdfGenerator()
        {
            var model = new InitialLoadModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PdfGenerator(InitialLoadModel model)
        {
            var list = PdfHelper.GetTemplatefields(model);

            var path = PdfHelper.GeneratePdfFromExisingValues(list);
            if (path == null)
                return Content("filename not present");

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [NonAction]
        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        [NonAction]
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }


        [HttpGet]
        public IActionResult OCrTest(InitialLoadModel model)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OCrTest(List<IFormFile> files)
        {
            byte[] image = null;

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        image = ms.ToArray();
                        // string s = Convert.ToBase64String(image);
                        // act on the Base64 data
                    }
                }
            }

            HttpClient client = new HttpClient();
            HttpContent bytesContent = new ByteArrayContent(image);
            
            var contentdata = new FormUrlEncodedContent(new[]
            {
                 new KeyValuePair<string, string>("Type", "SA_DL"),
            });
            
            var formdata = new MultipartFormDataContent();
            formdata.Add(bytesContent);
            formdata.Add(contentdata);

            var request = new HttpRequestMessage
            {
                Content = formdata,
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://52.172.133.91:5023/show"),
            };

            client.DefaultRequestHeaders.Add("Postman-Token", "0b2b8d5e-ac04-420e-96e7-ee60806a75a9");
            var response = await client.SendAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            return Ok(new OCRResult { Message = "Result Return Success", response = data });
        }
    }

    public class OCRResult
    {
        public string Message { get; set; }
        public object response { get; set; }
    }
}
