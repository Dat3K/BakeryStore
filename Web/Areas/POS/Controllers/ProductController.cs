using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services.Interfaces;
using Web.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ProductController(IProductService productService, ICategoryService categoryService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _productService = productService;
            _categoryService = categoryService;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        [Authorize]
        [HttpPost]
        [Route("POS/[controller]/AddProduct")]
        public async Task<IActionResult> AddProduct([FromForm] Product product, IFormFile image)
        {
            try
            {
                if (image != null && image.Length > 0)
                {
                    var imgurClientId = _configuration["Imgur:ClientId"];
                    
                    using (var ms = new MemoryStream())
                    {
                        await image.CopyToAsync(ms);
                        ms.Position = 0;
                        
                        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.imgur.com/3/image");
                        request.Headers.Add("Authorization", $"Client-ID {imgurClientId}");

                        var content = new MultipartFormDataContent();
                        content.Add(new StreamContent(ms), "image");
                        request.Content = content;

                        var response = await _httpClient.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            var imgurResponse = JsonSerializer.Deserialize<ImgurResponse>(responseContent);
                            if (imgurResponse?.Data?.Link != null)
                            {
                                product.Thumbnail = imgurResponse.Data.Link;
                            }
                            else
                            {
                                return Json(new { success = false, message = "Failed to get image URL from Imgur" });
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to upload image to Imgur" });
                        }
                    }
                }

                // Add product to database
                await _productService.AddProductAsync(product);

                return Json(new { success = true, message = "Product added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [Authorize]
        [Route("POS/[controller]/GetAllProducts")]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                var response = new
                {
                    draw = HttpContext.Request.Query["draw"].FirstOrDefault(),
                    recordsTotal = products.Count(),
                    recordsFiltered = products.Count(),
                    data = products
                };
                return Json(response, _jsonOptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving products", error = ex.Message });
            }
        }
    }

    public class ImgurResponse
    {
        public ImgurData? Data { get; set; }
        public bool Success { get; set; }
    }

    public class ImgurData
    {
        public string? Link { get; set; }
    }
}