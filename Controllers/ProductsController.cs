using dotnetAPI.Data;
using dotnetAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnetAPI.Dtos;

namespace EFCoreRelationships.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext appDbContext;

        public ProductsController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int categoryId)
        {

                var products = await appDbContext.Products
            .Where(x => x.CategoryId == categoryId)
            .Include(x => x.Size)
            .Include(x => x.Colors)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                price = x.Price,
                category = x.Category == null ? null : new
                {
                    id = x.Category.Id,
                    name = x.Category.Name
                },
                categoryId = x.CategoryId,
                size = x.Size == null ? null : new
                {
                    id = x.Size.Id,
                    name = x.Size.Name,
                    productId = x.Id
                },
                colors = x.Colors == null ? null : x.Colors.Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                })
            }).ToListAsync();
                return Ok(products);
        }


        [HttpGet("GetOneProduct")]
        public async Task<IActionResult> GetOneProduct(int productId)
        {

            var products = await appDbContext.Products
        .Where(x => x.Id == productId)
        .Include(x => x.Size)
        .Include(x => x.Colors)
        .Select(x => new
        {
            id = x.Id,
            name = x.Name,
            price = x.Price,
            category = x.Category == null ? null : new
            {
                id = x.Category.Id,
                name = x.Category.Name
            },
            categoryId = x.CategoryId,
            size = x.Size == null ? null : new
            {
                id = x.Size.Id,
                name = x.Size.Name,
                productId = x.Id
            },
            colors = x.Colors == null ? null : x.Colors.Select(c => new
            {
                id = c.Id,
                name = c.Name
            })
        }).FirstOrDefaultAsync();
            return Ok(products);
        }


        [HttpPost("CreateOneProduct")]
        public async Task<IActionResult> CreateOneProduct([FromBody] CreateProductDto productDto)
        {
            var category = await appDbContext.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Invalid CategoryId");
            }

            var size = await appDbContext.Sizes.FindAsync(productDto.SizeId);
            if (size == null)
            {
                return BadRequest("Invalid SizeId");
            }

            var colors = new List<Color>();
            foreach (var colorId in productDto.ColorIds)
            {
                var color = await appDbContext.Colors.FindAsync(colorId);
                if (color == null)
                {
                    return BadRequest($"Invalid ColorId: {colorId}");
                }
                colors.Add(color);
            }

            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Category = category,
                Size = size,
                Colors = colors
            };

            await appDbContext.Products.AddAsync(product);
            await appDbContext.SaveChangesAsync();

            return CreatedAtAction("GetOneProduct", new { id = product.Id }, product);
        }



        [HttpDelete("DeleteOneProduct")]
        public async Task<IActionResult> DeleteOneProduct([FromBody] DeleteProductDto productDto)
        {
            var product = await appDbContext.Products.FindAsync(productDto.Id);
            if (product == null)
            {
                return NotFound();
            }

            appDbContext.Products.Remove(product);
            await appDbContext.SaveChangesAsync();

            var deletedProduct = new Product 
            {
                Id = product.Id,
                Name = product.Name,
            };


            return Ok(deletedProduct);
        }


        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto productDto)
        {

            var product = await appDbContext.Products
            .Include(p => p.Colors)
            .FirstOrDefaultAsync(p => p.Id == productDto.Id);

            if (product == null)
            {
                return NotFound();
            }

            var category = await appDbContext.Categories.FindAsync(productDto.CategoryId);
            var size = await appDbContext.Sizes.FindAsync(productDto.SizeId);
            var colors = await appDbContext.Colors.Where(c => productDto.ColorIds.Contains(c.Id)).ToListAsync();

            if (product.Size != null && product.Size.Id != size.Id)
            {
                

                return BadRequest("A different size is already assigned to this product.");
            }

            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.Category = category;
            product.Size = size;
            product.Colors.Clear();
            product.Colors.AddRange(colors);

            await appDbContext.SaveChangesAsync();
            Console.WriteLine(product);
            return Ok(product);

          
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {

            var products = await appDbContext.Products
                .Include(x => x.Size)
                .Include(x => x.Colors)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    price = x.Price,
                    category = x.Category == null ? null : new
                    {
                        id = x.Category.Id,
                        name = x.Category.Name
                    },
                    categoryId = x.CategoryId,
                    size = x.Size == null ? null : new
                    {
                        id = x.Size.Id,
                        name = x.Size.Name,
                        productId = x.Id
                    },
                    colors = x.Colors == null ? null : x.Colors.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name
                    })
                }).ToListAsync();

            return Ok(products);
        }


        [HttpGet("getCategories")]
        public async Task<IActionResult> GetCategories()
        {

            var categories = await appDbContext.Categories
                .Include(x => x.Products)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    products = x.Products.Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        price = p.Price
                    })
                })
                .ToListAsync();
            return Ok(categories);

        }
    }
    }