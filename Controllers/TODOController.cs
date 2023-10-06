using Ju.Data;
using Ju.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ju.Controllers
{
    [ApiController]
    [Route("[Action]")]
    public class TODOController : ControllerBase
    {

        private readonly ILogger<TODOController> _logger;
        private readonly TODODbContext _context;
        private readonly IConfiguration _configuration;

        public TODOController(ILogger<TODOController> logger, TODODbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }


        /*ESTUDAR CRUD segue um exemplo de CRUD*/

        // create user (CREATE)
        [HttpPost]
        public IActionResult createUser(UserModel user)
        {
            if (ModelState.IsValid)
            {
                _context.users.Add(user);
                _context.SaveChanges();
                return Ok(new { message = "User created successfully", user });
            }
            else
                return BadRequest();
        }

        //get all users (READ)
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            List<UserModel> users = _context.users.ToList();
            if (users.Count > 0)
            {
                return Ok(users);
            }
            else
            {
                return null;
            }
        }

        //update user (UPDATE)
        [HttpPost]
        public IActionResult UpdateUser(UserModel updateUser)
        {
            if (ModelState.IsValid)
            {
                UserModel? userDB = _context.users.FirstOrDefault(x => x.Id == updateUser.Id);
                if (userDB != null)
                {
                    userDB.Username = updateUser.Username;
                    userDB.Password = updateUser.Password;
                    _context.users.Update(userDB);
                    _context.SaveChanges();

                    return Ok(new { message = "User update successfully", userDB });
                }
                else
                {
                    return BadRequest("user not found");
                }

            }
            else
                return BadRequest("model not valid");
        }

        // delete user (DELETE)
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                UserModel? userDB = _context.users.FirstOrDefault(x => x.Id == id);
                if (userDB != null)
                {
                    _context.users.Remove(userDB);
                    _context.SaveChanges();
                }
                return Ok(new { message = "User Delete successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /*login gerando token*/
        [HttpPost]
        public IActionResult Login(UserModel user)
        {
            UserModel? userDB = _context.users.FirstOrDefault(x => x.Username == user.Username && x.Password == user.Password);
            if (userDB != null)
            {
                //create claims details based on the user information
                var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("Id", user.Id.ToString()),
                        new Claim("Username", user.Username)
                    };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:Time"])),
                    signingCredentials: signIn);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            else
            {
                return BadRequest("Invalid user");
            }
        }


        /*create todo*/
        [HttpPost]
        public IActionResult CreateTODOs(TodoModel todo, int userId)
        {
            //verif if user exist first 
            UserModel? userDB = _context.users.FirstOrDefault(x => x.Id == userId);
            if (userDB != null)
            {
                todo.UserId = userDB.Id;

                if (ModelState.IsValid)
                {
                    _context.todos.Add(todo);
                    _context.SaveChanges();
                    return Ok(new { message = "TODO created successfully", todo });
                }
                else
                    return BadRequest();
            }
            else
            {
                return BadRequest("user not found");
            }
        }

        [HttpGet]
        public IActionResult GetAllTODOs()
        {
            List<TodoModel> todos = _context.todos.ToList();
            if (todos.Count > 0)
            {
                return Ok(todos);
            }
            else
            {
                return null;
            }
        }


    }
}