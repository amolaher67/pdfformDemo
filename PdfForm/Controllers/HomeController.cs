using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    }
}
