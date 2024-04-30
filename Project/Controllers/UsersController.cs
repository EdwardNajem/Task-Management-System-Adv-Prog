using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project.Data;
using Project.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private IConfiguration _config;

        public UsersController(AppDbContext appDbContext, IConfiguration config)
        {
            _appDbContext = appDbContext;   
            _config = config;
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _appDbContext.Users.FindAsync(id);
            if (user == null){return NotFound();}
            _appDbContext.Users.Remove(user);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost]
        public async Task<ActionResult<User>> AddUser([FromBody] User user)
        {      _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _appDbContext.Users
                .Include(u => u.ProjectUsers)
                    .ThenInclude(pu => pu.Projectt)
                .Include(u => u.Tasks) // Include the Tasks for each user
                .ToListAsync();
        }



        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _appDbContext.Users.FindAsync(id);
            if (user == null)
            {return NotFound();}
            return user;
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo(int id, [FromBody] User userInfo)
        {
            var user = await _appDbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Name = userInfo.Name;
            user.Email = userInfo.Email;
            user.Bio = userInfo.Bio;

            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Login([FromBody] UserDto loginUser)
        {
            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Name == loginUser.Name && u.Password == loginUser.Password);

            if (user == null)
            {
                return NotFound(new { message = "Invalid username or password." });
            }

            var token = Generate(user);

            var response = new
            {
                user = user,
                token = token
            };
            return Ok(response);

        }

        private string Generate(User user)
        {
            Console.WriteLine(_config["Jwt:Key"]);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.IsAdmin?"admin":"user")
            };

            Console.WriteLine(claims);
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddHours(1),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }






    }
}