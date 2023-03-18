using dotnetAPI.Data;
using dotnetAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnetAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using dotnetAPI.Interfaces;

namespace EFCoreRelationships.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private readonly UserManager<User> _userManager;

        public ProductsController(UserManager<User> userManager, AppDbContext appDbContext)
        {
            _userManager = userManager;
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


        // Assuming you have a DbContext with a Driver DbSet and a Person DbSet

        [HttpPost("Filter")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> Find([FromBody] IProductFilter filter)
        {
            try
            {

                


                var query = appDbContext.Products
                    .Include(x => x.Size)
                    .Include(x => x.Category)
                    .Include(x => x.Colors)
                     .AsQueryable();



                if (!string.IsNullOrEmpty(filter.Name))
                {
                    query = query.Where(d => d.Name.Contains(filter.Name));
                } 
                if (!string.IsNullOrEmpty(filter.Size))
                {
                    query = query.Where(d => d.Size.Name.Contains(filter.Size));
                }
                if (!string.IsNullOrEmpty(filter.Category))
                {
                    query = query.Where(d => d.Category.Name.Contains(filter.Category));
                }

                if (!string.IsNullOrEmpty(filter.Color))
                {
                    query = query.Where(d => d.Colors.Select(c => c.Name).Contains(filter.Color));
                }

                if (filter.Price != null && filter.Price.Length == 2)
                {
                    double minPrice = filter.Price[0];
                    double maxPrice = filter.Price[1];
                    query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }

                var result = query.Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    price = x.Price,
                    userId = x.UserId,
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
                }).ToList();

                //var result = await query.ToListAsync();


                //            var filteredProducts = products
                //.Where(p => p.name.Contains(filter.Name))
                //.Where(p => filter.Price == null || (p.price >= filter.Price[0] && p.price <= filter.Price[1]))
                //.Where(p => p.colors.Any(c => c.name == filter.Color))
                //.ToList();


                return Ok(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
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

        [Authorize]
        [HttpPost("CreateOneProduct")]
        public async Task<IActionResult> CreateOneProduct([FromBody] CreateProductDto productDto)
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(accessToken);

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

            var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
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
                Colors = colors,
                UserId = (int)user.Id
                
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
        [Authorize]
        [HttpGet]
        [Route("getAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {

                var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(accessToken);

                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

                var user = await appDbContext.Users.FirstOrDefaultAsync(u=>u.Email == email);
                //var userId = user.Id;

                var products2 = await appDbContext.Products.Where(x => x.UserId == user.Id).ToListAsync();


                var products = await appDbContext.Products
                    .Where(x => x.UserId == user.Id)
                    .Include(x => x.Size)
                    .Include(x => x.Colors)
                    .Select(x => new
                    {
                        id = x.Id,
                        name = x.Name,
                        price = x.Price,
                        userId=x.UserId,
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {



                var products = await appDbContext.Products
                    
                    .Include(x => x.Size)
                    .Include(x => x.Category)
                    .Include(x => x.Colors)
                    .Select(x => new
                    {
                        id = x.Id,
                        name = x.Name,
                        price = x.Price,
                        userId = x.UserId,
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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