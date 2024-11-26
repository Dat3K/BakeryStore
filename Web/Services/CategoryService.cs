using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Services.Interfaces;
using Web.Services.Exceptions;

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
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException($"Category with ID {id} was not found");
            return category;
        }

        public async Task AddCategoryAsync(Category category)
        {
            if (category == null)
                throw new ValidationException("Category cannot be null");
                
            await _categoryRepository.AddAsync(category);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            if (category == null)
                throw new ValidationException("Category cannot be null");

            var existingCategory = await _categoryRepository.GetByIdAsync(category.Id);
            if (existingCategory == null)
                throw new NotFoundException($"Category with ID {category.Id} was not found");

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id) ?? throw new NotFoundException($"Category with ID {id} was not found");
            await _categoryRepository.DeleteAsync(id);
        }
    }
}
