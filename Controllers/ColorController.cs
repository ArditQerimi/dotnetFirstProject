using dotnetAPI.Data;
using dotnetAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace dotnetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly AppDbContext appDbContext;

        public ColorController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }


        [HttpGet("AllColors")]
        public async Task<IActionResult> GetAllColors()
        {
            var categories = await appDbContext.Colors.ToListAsync();

            return Ok(categories);
        }




        [HttpGet("GetColor")]
        public async Task<IActionResult> GetColor(int colorId)
        {
            var color = await appDbContext.Colors.Where(x => x.Id == colorId).Select(x => new
            {
                id = x.Id,
                name = x.Name,

            }).FirstOrDefaultAsync(); ;

            return Ok(color);
        }

        [HttpPost("CreateColor")]
        public async Task<IActionResult> CreateColor([FromBody] CreateColorDto colorDto)
        {
            var color = new Color
            {
                Name = colorDto.Name,
            };
            await appDbContext.Colors.AddAsync(color);
            await appDbContext.SaveChangesAsync();

            return Ok(colorDto);

            //return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }


        [HttpPut("UpdateColor")]
        public async Task<IActionResult> UpdateColor([FromBody] UpdateColorDto colorDto)
        {

            var color = await appDbContext.Colors
            .FirstOrDefaultAsync(p => p.Id == colorDto.Id);
            color.Name = colorDto.Name;

            await appDbContext.SaveChangesAsync();
            return Ok(color);
        }

        [HttpDelete("DeleteColor")]
        public async Task<IActionResult> DeleteColor([FromBody] DeleteColorDto colorDto)
        {
            var color = await appDbContext.Colors.FindAsync(colorDto.Id);
            if (color == null)
            {
                return NotFound();
            }

            appDbContext.Colors.Remove(color);
            await appDbContext.SaveChangesAsync();

            var deletedColor = new Color
            {
                Id = color.Id,
                Name = color.Name,
            };


            return Ok(deletedColor); }

        }
    }

