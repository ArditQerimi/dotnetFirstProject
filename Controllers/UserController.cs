//using Microsoft.AspNetCore.Mvc;
//using dotnetAPI.Dtos;
//using System.Drawing;
//using dotnetAPI.Data;
//using Microsoft.EntityFrameworkCore;

//namespace dotnetAPI.Controllers
//{
//    [ApiController]
//    [Route("api/users")]
//    public class UserController : ControllerBase
//    {
//        private readonly AppDbContext appDbContext;

//        public UserController(AppDbContext appDbContext)
//        {
//            this.appDbContext = appDbContext;
//        }
//        // GET api/users
//        [HttpGet]
//        public async Task<IActionResult> GetAllUsersAsync()
//        {
//            //var users = new List<UserDto>();
//            var users = await appDbContext.Users
//                .Select(x => new
//                {
//                    id = x.Id,
//                    firstName = x.FirstName,
//                    lastName = x.LastName,
//                    products = x.Products == null ? null : x.Products.Select(c => new
//                    {
//                        id = c.Id,
//                        name = c.Name
//                    })
//                }).ToListAsync();   
            
//            return Ok(users);
//        }

//        // GET api/users/{id}
//        [HttpGet("{id}")]
//        public IActionResult GetUserById(int id)
//        {
//            // implementation to get a user by id from database or other source
//            var user = new UserDto();
//            if (user == null)
//            {
//                // return a 404 Not Found response if user is not found
//                return NotFound();
//            }
//            // return a 200 OK response with the user
//            return Ok(user);
//        }

//        // POST api/users
//        [HttpPost]
//        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
//        {
//            var user = new User{
//                FirstName = createUserDto.FirstName,
//                LastName = createUserDto.LastName,  
//                Email = createUserDto.Email,
//                Password= createUserDto.Password,
//                CreatedAt= DateTime.Now,
//                UpdatedAt= DateTime.Now,    
//            };

//            await appDbContext.Users.AddAsync(user);
//            await appDbContext.SaveChangesAsync();
//            return Ok(user);
//            //return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
//        }

//        // PUT api/users/{id}
//        [HttpPut("{id}")]
//        public IActionResult UpdateUser(int id, UpdateUserDto updateUserDto)
//        {
//            // implementation to update an existing user in the database or other source
//            var user = new UserDto();
//            if (user == null)
//            {
//                // return a 404 Not Found response if user is not found
//                return NotFound();
//            }
//            // update the user with the new data from the DTO
//            user.FirstName = updateUserDto.FirstName;
//            user.LastName = updateUserDto.LastName;
//            user.Email = updateUserDto.Email;
//            // return a 204 No Content response
//            return NoContent();
//        }

//        // DELETE api/users/{id}
//        [HttpDelete("{id}")]
//        public IActionResult DeleteUser(int id)
//        {
//            // implementation to delete an existing user from the database or other source
//            var user = new UserDto();
//            if (user == null)
//            {
//                // return a 404 Not Found response if user is not found
//                return NotFound();
//            }
//            // return a 204 No Content response
//            return NoContent();
//        }
//    }
//}
