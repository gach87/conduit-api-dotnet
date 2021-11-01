using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Conduit.Models
{
    public class User
    {
        [Key]
        public string email { get; set; }
        public string token { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        public string bio { get; set; }
        public string image { get; set; }

        public IList<Follower> followers { get; set; }
    }

    public class Follower
    {
        [Key]
        public string username { get; set; }
    }
}