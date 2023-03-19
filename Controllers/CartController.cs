using dotnetAPI.Data;
using dotnetAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;

namespace dotnetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext appDbContext;

        public CartController( AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }



        [Authorize]
        [HttpGet]
        [Route("getAll")]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {

                var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(accessToken);

                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

                var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

                var cartItems = await appDbContext.CartItems
                    .Where(x => x.UserId == user.Id)
                    .Select(x => new
                    {
                        id = x.Id,

                        product = x.Product == null ? null : new
                        
                             {
                                    id = x.Id,
                            name = x.Product.Name,
                            price = x.Product.Price,
                            category = x.Product.Category == null ? null : new
                            {
                                id = x.Product.Category.Id,
                                name = x.Product.Category.Name
                            },
                            categoryId = x.Product.CategoryId,
                            size = x.Product.Size == null ? null : new
                            {
                                id = x.Product.Size.Id,
                                name = x.Product.Size.Name,
                                productId = x.Id
                            },
                            colors = x.Product.Colors == null ? null : x.Product.Colors.Select(c => new
                            {
                                id = c.Id,
                                name = c.Name
                            })
        
                            },  
                       
                        quantity = x.Quantity,
                        total=x.Quantity * x.Product.Price,
                    }).ToListAsync();

                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("{id}/incrementQuantity")]
        public async Task<IActionResult> IncrementOneItemsQuantity([FromRoute] int id)
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(accessToken);

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

            var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            var cartItem = await appDbContext.CartItems.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);
          if (cartItem != null)
            {
                cartItem.Quantity++;
            }

            var foundedProduct = await appDbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            var total = foundedProduct?.Price * cartItem.Quantity;

            await appDbContext.SaveChangesAsync();

            return Ok();
        }


        [Authorize]
        [HttpPatch]
        [Route("{id}/decrementQuantity")]
        public async Task<IActionResult> DecrementOneItemsQuantity([FromRoute] int id)
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(accessToken);

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

            var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            var cartItem = await appDbContext.CartItems.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);
            if (cartItem != null)
            {
                cartItem.Quantity--;
                if (cartItem.Quantity < 1)
                {
                    appDbContext.CartItems.Remove(cartItem);
                }

                await appDbContext.SaveChangesAsync();
            }

            return Ok();
        }


        [Authorize]
        [HttpDelete]
        [Route("removeItem")]
        public async Task<IActionResult> RemoveItem([FromBody] RemoveCartItemDto removeCartItem)
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(accessToken);

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

            var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            var cartItem = await appDbContext.CartItems.FirstOrDefaultAsync(p => p.Id == removeCartItem.itemId && p.UserId == user.Id);
          
                if (cartItem != null)
                {
                    appDbContext.CartItems.Remove(cartItem);
                await appDbContext.SaveChangesAsync();
            }


            return Ok();
        }


        [Authorize]
        [HttpDelete]
        [Route("clearCart")]
        public async Task<IActionResult> ClearCart()
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(accessToken);

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

            var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            var cartItem = await appDbContext.CartItems.Where(p => p.UserId == user.Id).ToListAsync();


            foreach (var itemId in cartItem)
            {
                appDbContext.CartItems.Remove(itemId);
                await appDbContext.SaveChangesAsync();
            }


            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("cart")]
        public async Task<IActionResult> CreateCartItem([FromBody] CreateCartItemDto createCartItem)
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(accessToken);

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

            var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            var foundedProduct = await appDbContext.Products.FirstOrDefaultAsync(p => p.Id == createCartItem.productId);

            if (foundedProduct == null || foundedProduct.UserId == user.Id)
            {
                return BadRequest("Cannot create a cart item for your own product.");
            }


            int quantity = 1;
            var total = foundedProduct?.Price * quantity;
            var cart = new CartItem {
                Product = foundedProduct,
                Total = total,
                Quantity = quantity,
                UserId = (int)user?.Id
            };
            await appDbContext.CartItems.AddAsync(cart);
            await appDbContext.SaveChangesAsync();
            return Ok(cart.Id);

        }


    }

}
