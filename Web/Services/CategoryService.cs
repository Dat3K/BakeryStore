using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.OrderBy(c => c.Name);
        }

        public async Task<Category> GetCategoryByIdAsync(Guid id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _categoryRepository.AddAsync(category);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
        }
    }
}
