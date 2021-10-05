
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
        private readonly ArticleContext _articleContext;

        public ArticlesController(ArticleContext articleContext)
        {
            _articleContext = articleContext;
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
            var articles = await _articleContext.Articles
            .Where(article =>
            article.author != null && article.author.username != null && article.author.username == author ||
            article.tagList != null && article.tagList.Count() > 0 && article.tagList.Any(tagInList => tag == tagInList.name ||
            true
            ))
            .Select(article => new Article()
            {
                slug = article.slug,
                title = article.title,
                description = article.description,
                body = article.body,
                createdAt = article.createdAt,
                updatedAt = article.updatedAt,
                favorited = article.favorited,
                favoritesCount = article.favoritesCount,
                tagList = article.tagList.Select(tag => tag.name)
            }).ToListAsync();

            var response =
               new MultipleArticlesResponse(articles, articles.Count());

            return (response);
        }

        [HttpGet]
        [Route("articles/{slug}")]
        public async Task<ActionResult<SingleArticleResponse>> GetArticle(string slug)
        {
            var articleInRepo = await _articleContext.Articles.FirstOrDefaultAsync(articleInRepo => articleInRepo.slug == slug);
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
                    tagList = articleInRepo.tagList.Select(tag => tag.name)
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

        [HttpGet]
        [Route("/articles/feed")]
        public async Task<ActionResult<MultipleArticlesResponse>> GetArticleFeed(
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0
            )
        {
            var articles = await _articleContext.Articles
                       .Select(article => new Article()
                       {
                           slug = article.slug,
                           title = article.title,
                           description = article.description,
                           body = article.body,
                           createdAt = article.createdAt,
                           updatedAt = article.updatedAt,
                           favorited = article.favorited,
                           favoritesCount = article.favoritesCount,
                           tagList = article.tagList.Select(tag => tag.name)
                       }).Take(limit).Skip(offset).ToListAsync();

            var response =
               new MultipleArticlesResponse(articles, articles.Count());

            return (response);
        }

        [HttpPost]
        [Route("articles")]
        public async Task<ActionResult<SingleArticleResponse>> CreateArticle([FromBody] NewArticleRequest article)
        {
            var articleToSave = new Conduit.Models.Article()
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
                tagList = article.article.tagList != null && article.article.tagList.Count() > 0 ? article.article.tagList.Select(tag => new Tag() { name = tag }).ToList() : new List<Tag>(),
                comments = new List<Conduit.Models.Comment>()
            };

            _articleContext.Articles.Add(articleToSave);
            await _articleContext.SaveChangesAsync();

            var articleResponse = new Article()
            {
                slug = articleToSave.slug,
                title = articleToSave.title,
                description = articleToSave.description,
                body = articleToSave.body,
                createdAt = articleToSave.createdAt,
                updatedAt = articleToSave.updatedAt,
                favorited = articleToSave.favorited,
                favoritesCount = articleToSave.favoritesCount,
                tagList = articleToSave.tagList.Select(tag => tag.name)
            };

            var response = new SingleArticleResponse()
            {
                article = articleResponse
            };

            return CreatedAtAction(nameof(GetArticle), new { slug = response.article.slug }, response);
        }

        [HttpPut]
        [Route("articles/{slug}")]
        public async Task<ActionResult<SingleArticleResponse>> UpdateArticle(string slug, [FromBody] UpdateArticleRequest article)
        {
            var articleInRepo = await _articleContext.Articles.FirstOrDefaultAsync(articleInRepo => articleInRepo.slug == slug);

            if (articleInRepo != null)
            {
                articleInRepo.title = article.article.title != null ? article.article.title : articleInRepo.title;
                articleInRepo.description = article.article.description != null ? article.article.description : articleInRepo.description;
                articleInRepo.body = article.article.body != null ? article.article.body : articleInRepo.body;

                await _articleContext.SaveChangesAsync();
                var articleResponse = new Article()
                {
                    slug = articleInRepo.slug,
                    title = articleInRepo.title,
                    description = articleInRepo.description,
                    body = articleInRepo.body,
                    createdAt = articleInRepo.createdAt,
                    updatedAt = articleInRepo.updatedAt,
                    favorited = articleInRepo.favorited,
                    favoritesCount = articleInRepo.favoritesCount,
                    tagList = articleInRepo.tagList.Select(tag => tag.name)
                };
                var response = new SingleArticleResponse()
                {
                    article = articleResponse
                };
                return Ok(response);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpDelete]
        [Route("articles/{slug}")]
        public async Task<ActionResult<SingleArticleResponse>> DeleteArticle(string slug, [FromBody] UpdateArticleRequest article)
        {
            var articleInRepo = await _articleContext.Articles.FirstOrDefaultAsync(articleInRepo => articleInRepo.slug == slug);

            if (articleInRepo != null)
            {
                _articleContext.Articles.Remove(articleInRepo);
                await _articleContext.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Route("/articles/{slug}/comments")]
        public async Task<ActionResult<MultipleCommentsResponse>> GetArticleComments(string slug)
        {
            var articleInRepo = await _articleContext.Articles.FirstOrDefaultAsync(articleInRepo => articleInRepo.slug == slug);
            if (articleInRepo != null)
            {
                if (articleInRepo.comments != null && articleInRepo.comments.Count() > 0)
                {
                    var comments = articleInRepo.comments.Select(commentInRepo => new Comment()
                    {

                        id = commentInRepo.id,
                        createdAt = commentInRepo.createdAt,
                        updatedAt = commentInRepo.updatedAt,
                        body = commentInRepo.body,
                        author = new Profile() { username = commentInRepo.author.username, bio = commentInRepo.author.bio, image = commentInRepo.author.image, following = commentInRepo.author.following }
                    });

                    var response = new MultipleCommentsResponse() { comments = comments };
                    return (response);
                }
                else
                {
                    var response = new MultipleCommentsResponse() { comments = new List<Conduit.Articles.Comment>() };
                    return (response);
                }
            }
            else
            {
                var response = NotFound();
                return (response);
            }
        }

        [HttpPost]
        [Route("articles/{slug}/comments")]
        public async Task<ActionResult<SingleCommentResponse>> CreateArticleComment(string slug, [FromBody] Comment comment)
        {
            var articleInRepo = await _articleContext.Articles.FirstOrDefaultAsync(articleInRepo => articleInRepo.slug == slug);
            if (articleInRepo != null)
            {
                if (articleInRepo.comments != null)
                {
                    var commentToAdd = new Conduit.Models.Comment()
                    {
                        id = articleInRepo.comments.Count() + 1,
                        createdAt = DateTime.UtcNow,
                        updatedAt = DateTime.UtcNow,
                        body = comment.body,
                        author = comment.author != null ? new Conduit.Models.Profile() { username = comment.author.username, bio = comment.author.bio, image = comment.author.image, following = comment.author.following } : null
                    };
                    var comments = articleInRepo.comments.ToList();
                    comments.Add(commentToAdd);
                    await _articleContext.SaveChangesAsync();
                    var response = new SingleCommentResponse()
                    {
                        comment = new Comment()
                        {
                            id = commentToAdd.id,
                            createdAt = commentToAdd.createdAt,
                            updatedAt = commentToAdd.updatedAt,
                            body = commentToAdd.body,
                            author = new Profile()
                            {
                                username = commentToAdd.author.username,
                                bio = commentToAdd.author.bio,
                                image = commentToAdd.author.image,
                                following = commentToAdd.author.following
                            }
                        }
                    };
                    return (response);
                }
                else
                {
                    var response = NotFound();
                    return (response);
                }
            }
            else
            {
                var response = NotFound();
                return (response);
            }
        }
    }
    public class SingleCommentResponse
    {
        public Comment comment { get; set; }
    }

    public class MultipleCommentsResponse
    {
        public IEnumerable<Comment> comments { get; set; }
    }

    public class Comment
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string body { get; set; }
        public Profile author { get; set; }
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