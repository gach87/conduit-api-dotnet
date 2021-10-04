
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Conduit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace Conduit.Articles
{


    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly ArticleContext _context;

        public ArticlesController(ArticleContext context)
        {
            _context = context;

        }

        [HttpGet]
        [Route("articles")]
        public async Task<ActionResult<MultipleArticlesResponse>> ListArticles(
            [FromQuery] string tag,
            [FromQuery] string author,
            [FromQuery] string favorited,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0
        )

        {
            var articles = await _context.Articles.Select(article => new Article()
            {
                slug = article.slug,
                title = article.title,
                description = article.description,
                body = article.body,
                createdAt = article.createdAt,
                updatedAt = article.updatedAt,
                favorited = article.favorited,
                favoritesCount = article.favoritesCount,
                tagList = article.tagList
            }).ToListAsync();

            var response =
               new MultipleArticlesResponse(articles, articles.Count());

            return (response);
        }

        [HttpGet]
        [Route("articles/{slug}")]
        public async Task<ActionResult<SingleArticleResponse>> GetArticle(string slug)
        {
            var articleInRepo = await _context.Articles.FirstOrDefaultAsync(articleInRepo => articleInRepo.slug == slug);
            if (articleInRepo != null)
            {

                var article = new Article()
                {
                    slug = articleInRepo.slug,
                    title = articleInRepo.title,
                    description = articleInRepo.description,
                    body = articleInRepo.body,
                    createdAt = articleInRepo.createdAt,
                    updatedAt = articleInRepo.updatedAt,
                    favorited = articleInRepo.favorited,
                    favoritesCount = articleInRepo.favoritesCount,
                    tagList = articleInRepo.tagList
                };
                var response = new SingleArticleResponse()
                {
                    article = article
                }
                                    ;
                return (response);
            }
            else
            {
                var response = NotFound();
                return (response);
            }
        }

        [HttpPost]
        [Route("articles")]
        public async Task<ActionResult<SingleArticleResponse>> CreateArticle([FromBody] NewArticleRequest article)
        {
            var response = new Conduit.Models.Article()
            {
                slug = article.article.title.Replace(" ", "-").ToLower(),
                title = article.article.title,
                description = article.article.description,
                body = article.article.body,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                favorited = false,
                favoritesCount = 0,
                author = new Conduit.Models.Profile() { username = "test", bio = "test", image = "test", following = false },
                tagList = article.article.tagList != null && article.article.tagList.Count() > 0 ? article.article.tagList.Select(tag => tag).ToList() : new List<string>()
            };

            _context.Articles.Add(response);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArticle), new { slug = response.slug }, response);
        }

        [HttpPut]
        [Route("articles/{slug}")]
        public async Task<ActionResult<SingleArticleResponse>> UpdateArticle(
            string slug,
            [FromBody] UpdateArticleRequest article
            )
        {
            var articleInRepo = await _context.Articles.FirstOrDefaultAsync(articleInRepo => articleInRepo.slug == slug);

            if (articleInRepo != null)
            {
                articleInRepo.title = article.article.title != null ? article.article.title : articleInRepo.title;
                articleInRepo.description = article.article.description != null ? article.article.description : articleInRepo.description;
                articleInRepo.body = article.article.body != null ? article.article.body : articleInRepo.body;

                await _context.SaveChangesAsync();
                return Ok(articleInRepo);
            }
            else
            {
                return NotFound();
            }

        }
        [HttpDelete]
        [Route("articles/{slug}")]
        public async Task<ActionResult<SingleArticleResponse>> DeleteArticle(
                 string slug,
                 [FromBody] UpdateArticleRequest article
                 )
        {
            var articleInRepo = await _context.Articles.FirstOrDefaultAsync(articleInRepo => articleInRepo.slug == slug);

            if (articleInRepo != null)
            {
                _context.Articles.Remove(articleInRepo);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }


    }


    public class UpdateArticleRequest
    {
        public UpdateArticle article { get; set; }
    }

    public class UpdateArticle
    {
        public string title { get; set; }
        public string description { get; set; }
        public string body { get; set; }

    }

    public class NewArticleRequest
    {
        public NewRequest article { get; set; }
    }

    public class NewRequest
    {
        public string title { get; set; }
        public string description { get; set; }
        public string body { get; set; }
        public IEnumerable<string> tagList { get; set; }

    }

    public class SingleArticleResponse
    {
        public Article article { get; set; }

    }
    public class MultipleArticlesResponse
    {
        public IEnumerable<Article> articles { get; set; }
        public int articlesCount { get; set; }

        public MultipleArticlesResponse(
            IEnumerable<Article> articles, int articlesCount)
        {
            this.articles = articles;
            this.articlesCount = articlesCount;
        }

    }


    public class Article
    {
        public string slug { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string body { get; set; }
        public IEnumerable<string> tagList { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool favorited { get; set; }
        public int favoritesCount { get; set; }
        public Profile author { get; set; }


    }
    public class Profile
    {
        public string username { get; set; }
        public string bio { get; set; }
        public string image { get; set; }
        public bool following { get; set; }
    }
}