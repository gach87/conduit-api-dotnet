using System;
using System.Linq;
using System.Threading.Tasks;
using Conduit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Profiles
{
    public class ProfileController : Controller
    {
        public readonly ConduitContext _context;
        public ProfileController(ConduitContext context)
        {
            this._context = context;
        }

        [HttpGet]
        [Route("profiles/{username}")]
        public async Task<ActionResult<ProfileResponse>> GetProfileByUsername(string username)
        {
            var isAFollower = false;
            var userInRepo = await this._context.Users.FirstOrDefaultAsync(user => user.username == username);
            if (userInRepo != null)
            {
                this.Request.Headers.TryGetValue("Authorization", out var headerValue);
                if (headerValue != "")
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(user => headerValue.ToString().Contains(user.token));
                    if (currentUser != null)

                    {
                        isAFollower = userInRepo.followers.Any(Follower => Follower.username == currentUser.username);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return Ok(new ProfileResponse()
                {
                    profile = new Profile()
                    {
                        username = userInRepo.username,
                        bio = userInRepo.bio,
                        image = userInRepo.image,
                        following = isAFollower
                    }
                });
            }
            else
            {
                return UnprocessableEntity();
            }
        }

        [HttpPost]
        [Route("profiles/{username}/follow")]
        public async Task<ActionResult<ProfileResponse>> FollowUserByUsername(string username)
        {
            this.Request.Headers.TryGetValue("Authorization", out var headerValue);
            if (headerValue != "")
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(user => headerValue.ToString().Contains(user.token));
                if (currentUser != null)
                {
                    var userInRepo = await this._context.Users.FirstOrDefaultAsync(user => user.username == username);
                    if (userInRepo != null)
                    {
                        try
                        {
                            userInRepo.followers.Add(new Follower() { username = currentUser.username });
                            await _context.SaveChangesAsync();
                            return Ok(new ProfileResponse()
                            {
                                profile = new Profile()
                                {
                                    username = userInRepo.username,
                                    bio = userInRepo.bio,
                                    image = userInRepo.image,
                                    following = userInRepo.followers.Any(Follower => Follower.username == currentUser.username)
                                }
                            });
                        }
                        catch (Exception e)
                        {

                            return Ok(new ProfileResponse()
                            {
                                profile = new Profile()
                                {
                                    username = userInRepo.username,
                                    bio = userInRepo.bio,
                                    image = userInRepo.image,
                                    following = userInRepo.followers.Any(Follower => Follower.username == currentUser.username)
                                }
                            });
                        }


                    }
                    else
                    {
                        return UnprocessableEntity();
                    }

                }
                else
                {
                    return Unauthorized();
                }
            }
            return Ok();
        }

        [HttpDelete]
        [Route("profiles/{username}/follow")]
        public async Task<ActionResult<ProfileResponse>> UnfollowUser(string username)
        {
            var isAFollower = false;
            var userInRepo = await this._context.Users.FirstOrDefaultAsync(user => user.username == username);
            if (userInRepo != null)
            {
                this.Request.Headers.TryGetValue("Authorization", out var headerValue);
                if (headerValue != "")
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(user => headerValue.ToString().Contains(user.token));
                    if (currentUser != null)
                    {
                        if (userInRepo.followers.Any(Follower => Follower.username == currentUser.username))
                        {
                            userInRepo.followers = userInRepo.followers.Where(Follower => Follower.username != currentUser.username).ToList();
                            await _context.SaveChangesAsync();
                            return Ok(new ProfileResponse()
                            {
                                profile = new Profile()
                                {
                                    username = userInRepo.username,
                                    bio = userInRepo.bio,
                                    image = userInRepo.image,
                                    following = userInRepo.followers.Any(Follower => Follower.username == currentUser.username)
                                }
                            });
                        };
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return Ok(new ProfileResponse()
                {
                    profile = new Profile()
                    {
                        username = userInRepo.username,
                        bio = userInRepo.bio,
                        image = userInRepo.image,
                        following = isAFollower
                    }
                });
            }
            else
            {
                return UnprocessableEntity();
            }

        }


        public class ProfileResponse
        {
            public Profile profile { get; set; }
        }

        public class Profile
        {
            public string username { get; set; }
            public string bio { get; set; }
            public string image { get; set; }
            public bool following { get; set; }

        }
    }
}