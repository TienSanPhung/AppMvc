//using System.Data.Entity;
using System.Linq;
using AppMvc.Models;
using AppMvc.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Areas.Blog.Controllers
{

    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;

        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Route("/post/{categoryslug?}")]

        public IActionResult Index(string categoryslug, [FromQuery(Name ="p")]int currentPage, int pagingSize)
        {
            var categories = GetCategory();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;

            Category category = null;
            if(!string.IsNullOrEmpty(categoryslug)){
                category =  _context.Categories.Where(c => c.Slug == categoryslug)
                            .Include(c => c.CategoryChildren)
                            .FirstOrDefault();
                if(category == null){
                    return NotFound("Không thấy category nào");
                }
            }

            var posts = _context.Posts
                        .Include(p => p.Author)
                        .Include(p => p.PostCategories)
                        .ThenInclude(p => p.Category)
                        .AsQueryable();
            posts.OrderByDescending(p => p.DateUpdated);

            if(category != null){
                var ids = new List<int>();
                category.ChildrenCateIDs(null,ids);
                ids.Add(category.Id);
                posts = posts.Where(p => p.PostCategories.Where(p => ids.Contains(p.CategoryID)).Any());
            }


            int totalpost =  posts.Count();
            if(pagingSize < 20) pagingSize = 5 ;
            int countPages = (int)Math.Ceiling((double)totalpost/ pagingSize);

            
            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;
            var pagingModel = new PagingModel(){
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) =>  Url.Action("Index",new {
                    p = pageNumber,
                    pagingSize = pagingSize
                })
            };
            ViewBag.PagingModel = pagingModel;
            ViewBag.totalpost = totalpost;
            ViewBag.postIndex = (currentPage - 1) * pagingSize;

            var postsInPage = posts.Skip((currentPage - 1) * pagingSize)
                                .Take(pagingSize);

            ViewBag.category = category;

            

            return View(postsInPage.ToList());
        }

        [Route("/post/{postslug}.html")]
        public IActionResult Details(string postslug) { 
            
            var categories = GetCategory();
            ViewBag.categories = categories;

            var post = _context.Posts.Where(p => p.Slug == postslug)
                                    .Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(p => p.Category)
                                    .FirstOrDefault();
            if(post == null){
                return NotFound("không thấy bài viết");
            }
            
            Category category = post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var ortherPost = _context.Posts.Where(p => p.PostCategories.Any(c => c.Category.Id == category.Id))
                                            .Where(p => p.PostId != post.PostId)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);

            ViewBag.ortherPost = ortherPost;
            return View(post);
        }

        private List<Category> GetCategory(){
            var categories = _context.Categories
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategoryId == null)
                            .ToList();
            return categories;
        }
    }
}
