using System.Linq;
using System.Threading.Tasks;
using Conduit.Models;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Controllers
{
    [Controller]
    public class UsersController : Controller
    {
        private readonly ConduitContext _context;

        public UsersController(ConduitContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("user")]
        public ActionResult<UserResponse> GetCurrentUser()
        {
            this.Request.Headers.TryGetValue("Authorization", out var headerValue);
            if (headerValue != "")
            {
                var user = _context.Users.FirstOrDefault(user => headerValue.ToString().Contains(user.token));
                if (user != null)
                {
                    return Ok(new UserResponse()
                    {
                        user = new User()
                        {
                            token = user.token,
                            email = user.email,
                            username = user.username,
                            bio = user.bio,
                            image = user.image
                        }
                    });
                }
                else
                {
                    return Unauthorized();
                }

            }
            return NotFound();
        }

        [HttpPut]
        [Route("user")]
        public ActionResult<UserResponse> UpdateCurrentUser([FromBody] UpdateUserRequest request)
        {
            this.Request.Headers.TryGetValue("Authorization", out var headerValue);
            if (headerValue != "")
            {
                var user = _context.Users.FirstOrDefault(user => headerValue.ToString().Contains(user.token));
                if (user != null)
                {
                    return Ok(new UserResponse()
                    {
                        user = new User()
                        {
                            token = user.token,
                            email = user.email,
                            username = user.username,
                            bio = user.bio,
                            image = user.image
                        }
                    });
                }
                else
                {
                    return Unauthorized();
                }

            }
            return NotFound();
        }


        [HttpPost]
        [Route("users/login")]
        public async Task<ActionResult<UserResponse>> Login([FromBody] LoginUserRequst request)
        {
            var user = await _context.Users.FindAsync(request.user.email);
            if (user != null)
            {
                return Ok(new UserResponse()
                {
                    user = new User()
                    {
                        token = user.token,
                        email = user.email,
                        username = user.username,
                        bio = user.bio,
                        image = user.image
                    }
                });
            }
            else
            {
                return UnprocessableEntity();
            }

        }


        [HttpPost]
        [Route("users")]
        public async Task<ActionResult<UserResponse>> CreateUser([FromBody] NewUserRequest request)
        {
            var user = await _context.Users.FindAsync(request.user.email);
            if (user == null)
            {
                var newUser = new Conduit.Models.User()
                {
                    email = request.user.email,
                    password = request.user.password,
                    username = request.user.username,
                    token = System.Guid.NewGuid().ToString(),
                    bio = "",
                    image = ""
                };
                _context.Add(newUser);
                _context.SaveChanges();

                return CreatedAtAction(nameof(CreateUser), new UserResponse()
                {
                    user = new User()
                    {
                        token = newUser.token,
                        email = newUser.email,
                        username = newUser.username,
                        bio = newUser.bio,
                        image = newUser.image
                    }
                });
            }
            else
            {
                return UnprocessableEntity();
            }

        }
    }

    public class UpdateUserRequest
    {
        public UpdateUser user { get; set; }
    }

    public class UpdateUser
    {

        public string email { get; set; }
        public string token { get; set; }
        public string username { get; set; }
        public string bio { get; set; }
        public string image { get; set; }
    }

    public class LoginUserRequst
    {
        public LoginUser user { get; set; }
    }

    public class LoginUser
    {
        public string email { get; set; }
        public string password { get; set; }
    }

    public class UserResponse
    {
        public User user { get; set; }
    }

    public class NewUserRequest
    {
        public NewUser user { get; set; }
    }

    public class NewUser
    {
        public string email { get; set; }
        public string password { get; set; }
        public string username { get; set; }


    }

    public class User
    {
        public string email { get; set; }
        public string token { get; set; }
        public string username { get; set; }
        public string bio { get; set; }
        public string image { get; set; }
    }
}