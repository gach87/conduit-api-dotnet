using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conduit.Models
{
    public class Article
    {
        [Key]
        public string slug { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string body { get; set; }
        public IEnumerable<Tag> tagList { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool favorited { get; set; }
        public int favoritesCount { get; set; }
        public Profile author { get; set; }
        public IEnumerable<Comment> comments { get; set;}

    }
    public class Profile
    {
        [Key]
        public string username { get; set; }
        public string bio { get; set; }
        public string image { get; set; }
        public bool following { get; set; }
    }

    public class Tag
    {
        [Key]
        public string name { get; set; }
    }

    public class Comment
    {
        [Key]
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string body { get; set; }
        public Profile author { get; set; }
    }

}