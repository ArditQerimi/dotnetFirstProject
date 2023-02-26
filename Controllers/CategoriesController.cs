using dotnetAPI.Data;
using dotnetAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace dotnetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext appDbContext;

        public CategoriesController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }


        [HttpGet("AllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await appDbContext.Categories.ToListAsync();

            return Ok(categories);
        }

    
    

        [HttpGet("GetCategory")]
        public async Task<IActionResult> CreateCategory(int categoryId)
        {
            var category = await appDbContext.Categories.Where(x => x.Id == categoryId).Select(x => new
            {
                id = x.Id,
                name = x.Name,

            }).FirstOrDefaultAsync(); ;

            return Ok(category);
        }

        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
            };
            await appDbContext.Categories.AddAsync(category);
            await appDbContext.SaveChangesAsync();

            return Ok(category);

            //return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        
        [HttpPut("UpdateCategory")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto categoryDto)
        {

            var category = await appDbContext.Categories
            .FirstOrDefaultAsync(p => p.Id == categoryDto.Id);
            category.Name = categoryDto.Name;

            await appDbContext.SaveChangesAsync();
            return Ok(category);
        }

        [HttpDelete("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryDto categoryDto)
        {
            var category = await appDbContext.Categories.FindAsync(categoryDto.Id);
            if (category == null)
            {
                return NotFound();
            }

            appDbContext.Categories.Remove(category);
            await appDbContext.SaveChangesAsync();

            var deletedCategory = new Category
            {
                Id = category.Id,
                Name = category.Name,
            };


            return Ok(deletedCategory);
        }
    }
}
