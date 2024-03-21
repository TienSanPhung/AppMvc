using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppMvc.Models;
using AppMvc.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using AppMvc.Data;
using AppMvc.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AppMvc.Utilities;
using System.Data;

namespace AppMvc.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/post/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string  StatusMessage { get; set; }
        // GET: Blog/Post
        public async Task<IActionResult> Index([FromQuery(Name ="p")] int currentPage, int pagingSize)
        {
            var posts = _context.Posts.Include(p => p.Author)
                                .OrderByDescending(p => p.DateCreated);
            
            int totalpost = await posts.CountAsync();
            if(pagingSize < 20) pagingSize =20;
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

            var postsInPage = await posts.Skip((currentPage - 1) * pagingSize)
                                .Take(pagingSize)
                                .Include(p => p.PostCategories)
                                .ThenInclude(pc => pc.Category)
                                .ToListAsync(); 
            return View(postsInPage);
        }

        // GET: Blog/Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Blog/Post/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.Categories.ToListAsync();

            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Blog/Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post)
        {
            
            
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title" );
            if(post.Slug == null){
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }
            if(await _context.Posts.AnyAsync(p => p.Slug == post.Slug)){
                ModelState.AddModelError("Slug","chuỗi Url bị trùng, nhập chuỗi Url khác");
                return View(post);
            }
            
            if (ModelState.IsValid)
            {
            
                var user = await _userManager.GetUserAsync(this.User);

                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;
                _context.Add(post);

                if(post.CategoryIDs != null){
                    foreach(var p in post.CategoryIDs){
                        _context.Add(new PostCategory(){
                            CategoryID = p,
                            Post = post
                        });
                    }
                }

                
                await _context.SaveChangesAsync();
                StatusMessage = "Vừa tạo bài viết mới";
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Blog/Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var post = await _context.Posts.FindAsync(id);
            var post = await _context.Posts.Include(pc => pc.PostCategories).FirstOrDefaultAsync(pc => pc.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            var PosEdit = new CreatePostModel(){
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                CategoryIDs = post.PostCategories.Select(pc => pc.CategoryID).ToArray()
            };

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            
            return View(PosEdit);
        }

        // POST: Blog/Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published, CategoryIDs")] CreatePostModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
             if(post.Slug == null){
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }
            if(await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id)){
                ModelState.AddModelError("Slug","chuỗi Url bị trùng, nhập chuỗi Url khác");
                return View(post);
            }
            
            

            if (ModelState.IsValid)
            {
                try
                {
                     var postUpdate = await _context.Posts.Include(pc => pc.PostCategories).FirstOrDefaultAsync(pc => pc.PostId == id);
                    if (postUpdate == null)
                    {
                        return NotFound();
                    }

                    postUpdate.Title = post.Title;
                    postUpdate.Content = post.Content;
                    postUpdate.Description = post.Description;
                    postUpdate.Slug = post.Slug;
                    postUpdate.DateUpdated = DateTime.Now;
                    postUpdate.Published = post.Published;

                    // cập nhật PostCategory
                    if(post.CategoryIDs == null) post.CategoryIDs = new int[] {};

                    var oldCateIDs = postUpdate.PostCategories.Select(pc => pc.CategoryID).ToArray();
                    var newCateIDs = post.CategoryIDs;  

                    var removeCatePosts = from postCate in postUpdate.PostCategories
                                            where(!newCateIDs.Contains(postCate.CategoryID))
                                            select postCate;
                    _context.PostCategories.RemoveRange(removeCatePosts);
                    var addCateIDs = from CateId in newCateIDs
                                        where(!oldCateIDs.Contains(CateId))
                                        select CateId;

                    foreach(var cateId in addCateIDs){
                        _context.PostCategories.Add(new PostCategory(){
                            PostID= id,
                            CategoryID = cateId
                        });
                    }       



                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                StatusMessage = " vừa cập nhật bài viết";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }

        // GET: Blog/Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Blog/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
                
            _context.Posts.Remove(post);
            StatusMessage = "Bạn vừa xóa bài viết" + post.Title;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
