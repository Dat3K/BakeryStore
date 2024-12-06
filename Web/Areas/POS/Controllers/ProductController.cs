using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services.Interfaces;
using Web.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductController(
            IProductService productService, 
            ICategoryService categoryService,
            ICloudinaryService cloudinaryService,
            IConfiguration configuration)
        {
            _productService = productService;
            _categoryService = categoryService;
            _cloudinaryService = cloudinaryService;
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
        [HttpGet]
        [Route("POS/[controller]/CheckDuplicateSKU")]
        public async Task<IActionResult> CheckDuplicateSKU(string sku)
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                bool isDuplicate = products.Any(p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
                return Json(isDuplicate);
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("POS/[controller]/AddProduct")]
        public async Task<IActionResult> AddProduct([FromForm] Product product, IFormFile image)
        {
            try
            {
                // Generate SKU if not provided
                if (string.IsNullOrEmpty(product.Sku))
                {
                    product.Sku = GenerateProductSku(product.Name);
                }

                // Check for duplicate SKU
                var products = await _productService.GetAllProductsAsync();
                if (products.Any(p => p.Sku.Equals(product.Sku, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = "Product SKU already exists" });
                }

                if (image != null && image.Length > 0)
                {
                    // Upload image to Cloudinary
                    var imageUrl = await _cloudinaryService.UploadImageAsync(image);
                    product.Thumbnail = imageUrl;
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

        private string GenerateProductSku(string productName)
        {
            if (string.IsNullOrEmpty(productName))
                return string.Concat("SKU", DateTime.Now.Ticks.ToString().AsSpan(0, 8));

            // Remove special characters and spaces, keep letters and numbers
            var words = productName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var sku = "";

            // Take up to first 3 words
            for (int i = 0; i < Math.Min(words.Length, 3); i++)
            {
                var word = words[i];
                // Take first 3 letters of each word if available
                if (word.Length > 0)
                {
                    sku += new string(word.Where(c => char.IsLetterOrDigit(c))
                                        .Take(3)
                                        .ToArray());
                }
            }

            // If SKU is too short, add timestamp
            if (sku.Length < 3)
            {
                sku = string.Concat("SKU", DateTime.Now.Ticks.ToString().AsSpan(0, 8));
            }
            else
            {
                // Add random numbers for uniqueness
                sku += DateTime.Now.ToString("MMdd");
            }

            return sku.ToUpper();
        }

        [Authorize]
        [Route("POS/[controller]/GetAllProducts")]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                // Order by CreatedAt descending (newest first)
                var orderedProducts = products.OrderByDescending(p => p.CreatedAt).ToList();
                var response = new
                {
                    draw = HttpContext.Request.Query["draw"].FirstOrDefault(),
                    recordsTotal = orderedProducts.Count,
                    recordsFiltered = orderedProducts.Count,
                    data = orderedProducts
                };
                return Json(response, _jsonOptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving products", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        [Route("POS/[controller]/GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Json(categories, _jsonOptions);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("POS/[controller]/AddCategory")]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            try
            {
                if (string.IsNullOrEmpty(category.Name))
                {
                    return BadRequest(new { success = false, message = "Category name is required" });
                }

                await _categoryService.AddCategoryAsync(category);
                return Json(new { success = true, message = "Category added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut]
        [Route("POS/[controller]/UpdateCategory/{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] Category category)
        {
            try
            {
                if (string.IsNullOrEmpty(category.Name))
                {
                    return BadRequest(new { success = false, message = "Category name is required" });
                }

                var existingCategory = await _categoryService.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound(new { success = false, message = "Category not found" });
                }

                existingCategory.Name = category.Name;

                await _categoryService.UpdateCategoryAsync(existingCategory);
                return Json(new { success = true, message = "Category updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("POS/[controller]/DeleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                // Check if category is in use
                var categories = await _categoryService.GetAllCategoriesAsync();
                if (categories.Any(p => p.Id == id))
                {
                    return Json(new { success = false, message = "Cannot delete category because it is being used by products" });
                }

                await _categoryService.DeleteCategoryAsync(id);
                return Json(new { success = true, message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        [Route("POS/[controller]/GetProductsByCategory/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategoryIdAsync(Guid categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                if (products == null || products.Count == 0)
                {
                    return NotFound($"No products found for category ID: {categoryId}");
                }
                return Json(products, _jsonOptions);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving products: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("POS/[controller]/UpdateProduct")]
        public async Task<IActionResult> UpdateProduct([FromForm] Product updatedProduct, IFormFile image)
        {
            try
            {
                var existingProduct = await _productService.GetProductByIdAsync(updatedProduct.Id);
                if (existingProduct == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                bool hasChanges = false;

                // Check and update each field only if it has changed
                if (!string.IsNullOrEmpty(updatedProduct.Name) && updatedProduct.Name != existingProduct.Name)
                {
                    existingProduct.Name = updatedProduct.Name;
                    hasChanges = true;
                }

                if (updatedProduct.Price != existingProduct.Price)
                {
                    existingProduct.Price = updatedProduct.Price;
                    hasChanges = true;
                }

                if (updatedProduct.CostPrice != existingProduct.CostPrice)
                {
                    existingProduct.CostPrice = updatedProduct.CostPrice;
                    hasChanges = true;
                }

                if (updatedProduct.StockQuantity != existingProduct.StockQuantity)
                {
                    existingProduct.StockQuantity = updatedProduct.StockQuantity;
                    hasChanges = true;
                }

                if (updatedProduct.CategoryId != existingProduct.CategoryId)
                {
                    existingProduct.CategoryId = updatedProduct.CategoryId;
                    hasChanges = true;
                }

                // Handle image update
                if (image != null && image.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingProduct.Thumbnail))
                    {
                        try
                        {
                            string publicId = ExtractCloudinaryPublicId(existingProduct.Thumbnail);
                            await _cloudinaryService.DeleteImageAsync(publicId);
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue with update
                            Console.WriteLine($"Failed to delete old image: {ex.Message}");
                        }
                    }

                    // Upload new image
                    var imageUrl = await _cloudinaryService.UploadImageAsync(image);
                    existingProduct.Thumbnail = imageUrl;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    existingProduct.UpdatedAt = DateTime.UtcNow;
                    
                    // Update only the changed properties
                    await _productService.UpdateProductPropertiesAsync(existingProduct.Id, new Dictionary<string, object>
                    {
                        { nameof(Product.Name), existingProduct.Name },
                        { nameof(Product.Price), existingProduct.Price },
                        { nameof(Product.CostPrice), existingProduct.CostPrice },
                        { nameof(Product.StockQuantity), existingProduct.StockQuantity },
                        { nameof(Product.CategoryId), existingProduct.CategoryId },
                        { nameof(Product.Thumbnail), existingProduct.Thumbnail },
                        { nameof(Product.UpdatedAt), existingProduct.UpdatedAt }
                    });

                    return Json(new { success = true, message = "Product updated successfully" });
                }

                return Json(new { success = true, message = "No changes detected" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                // Check if the product exists before attempting to delete
                var product = await _productService.GetProductByIdAsync(id);

                // If the product has a thumbnail, attempt to delete from Cloudinary
                if (!string.IsNullOrEmpty(product.Thumbnail))
                {
                    try
                    {
                        // Extract the public ID from the Cloudinary URL (implementation depends on your Cloudinary URL structure)
                        string publicId = ExtractCloudinaryPublicId(product.Thumbnail);
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                    catch (Exception cloudinaryEx)
                    {
                        // Log the Cloudinary deletion error, but don't stop the product deletion
                        // You might want to add proper logging here
                        Console.WriteLine($"Failed to delete image from Cloudinary: {cloudinaryEx.Message}");
                    }
                }

                // Delete the product from the database
                await _productService.DeleteProductAsync(id);

                return Json(new { success = true, message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string? ExtractCloudinaryPublicId(string cloudinaryUrl)
        {
            // This is a basic implementation. Adjust based on your actual Cloudinary URL structure
            // Example: https://res.cloudinary.com/your-cloud/image/upload/v1234/folder/imagename.jpg
            // Extract the public ID (imagename in this case)
            if (string.IsNullOrEmpty(cloudinaryUrl))
                return null;

            var parts = cloudinaryUrl.Split('/');
            var fileName = parts.LastOrDefault();
            
            if (fileName == null)
                return null;

            // Remove file extension
            var publicId = Path.GetFileNameWithoutExtension(fileName);
            return publicId;
        }
    }
}