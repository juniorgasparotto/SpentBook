using Microsoft.AspNetCore.Mvc;
using SpentBook.Web.Models;

namespace SpentBook.Web.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Account/SignIn
        [HttpGet]
        public IActionResult Index([FromServices ]ApplicationContext context)
        {
            var post = new Post();
            post.Blog = new Blog();
            post.Blog.Url2 = "URL";
            post.Title = "title";
            context.Posts.Add(post);
            context.SaveChanges();
            return RedirectToAction("Index", "Manage", new { area = "IdentityService" });
            }
    }
}
