using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Conduit.Models;

namespace Conduit.Tags
{
    public class TagsController
    {
        private readonly ArticleContext _context;

        public TagsController(ArticleContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("tags")]
        public ActionResult<TagsResponse> ListTags()

        {
            IList<string> tags = new List<string>();
            _context.Articles.ToList().ForEach(article => article.tagList.ToList().ForEach(tag => tags.Add(tag.name)));
            var response = new TagsResponse() { tags = tags.Distinct().ToList() };

            return (response);
        }

        public class TagsResponse
        {
            public IEnumerable<string> tags { get; set; }
        }
    }
}