using dotnetAPI.Data;
using dotnetAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace dotnetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        private readonly AppDbContext appDbContext;

        public SizeController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        [HttpGet("AllSizes")]
        public async Task<IActionResult> GetAllSizes()
        {
            var categories = await appDbContext.Sizes.ToListAsync();

            return Ok(categories);
        }




        [HttpGet("GetSize")]
        public async Task<IActionResult> GetSize(int sizeId)
        {
            var size = await appDbContext.Sizes.Where(x => x.Id == sizeId).Select(x => new
            {
                id = x.Id,
                name = x.Name,

            }).FirstOrDefaultAsync(); 

            return Ok(size);
        }

        [HttpPost("CreateSize")]
        public async Task<IActionResult> CreateSize([FromBody] CreateSizeDto createSize)
        {
            var size = new Size
            {
                SizeType = createSize.SizeType,
                Name = createSize.Name,
            };
            await appDbContext.Sizes.AddAsync(size);
            await appDbContext.SaveChangesAsync();

            return Ok(size);

            //return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }


        [HttpPut("UpdateSize")]
        public async Task<IActionResult> UpdateSize([FromBody] UpdateSizeDto updateDto)
        {

            var size = await appDbContext.Sizes.FirstOrDefaultAsync(p => p.Id == updateDto.Id);
            
            size.Name = updateDto.Name;
            size.SizeType = updateDto.SizeType;

            await appDbContext.SaveChangesAsync();
            return Ok(size);
        }

        [HttpDelete("DeleteSize")]
        public async Task<IActionResult> DeleteSize([FromBody] DeleteSizeDto sizeDto)
        {
            var size = await appDbContext.Sizes.FindAsync(sizeDto.Id);
            if (size == null)
            {
                return NotFound();
            }

            appDbContext.Sizes.Remove(size);
            await appDbContext.SaveChangesAsync();

            var deletedSize = new Size
            {
                Id = size.Id,
                Name = size.Name,
                SizeType = size.SizeType
            };


            return Ok(deletedSize);
        }
    }
}
