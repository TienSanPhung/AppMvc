using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppMvc.Models;
using Microsoft.AspNetCore.Authorization;
using AppMvc.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AppMvc.Utilities;
using System.Data;
using AppMvc.Areas.Product.Models;
using AppMvc.Models.Product;

namespace AppMvc.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/productmanage/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class ProductManageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public ProductManageController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string  StatusMessage { get; set; }
        // GET: Blog/Post
        public async Task<IActionResult> Index([FromQuery(Name ="p")] int currentPage, int pagingSize)
        {
            var products = _context.Products.Include(p => p.Author)
                                .OrderByDescending(p => p.DateUpdated);
            
            int totalproduct = await products.CountAsync();
            if(pagingSize < 20) pagingSize =20;
            int countPages = (int)Math.Ceiling((double)totalproduct / pagingSize);

            
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
            ViewBag.totalproduct = totalproduct;

            ViewBag.productIndex = (currentPage - 1) * pagingSize;

            var productsInPage = await products.Skip((currentPage - 1) * pagingSize)
                                .Take(pagingSize)
                                .Include(p => p.ProductCategoryProducts)
                                .ThenInclude(pc => pc.Category)
                                .ToListAsync(); 
            return View(productsInPage);
        }

        // GET: Blog/Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Blog/Post/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.CategoryProducts.ToListAsync();

            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Blog/Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIDs, Price")] CreateProductModel product)
        {
            
            
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title" );
            if(product.Slug == null){
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }
            if(await _context.Products.AnyAsync(p => p.Slug == product.Slug)){
                ModelState.AddModelError("Slug","chuỗi Url bị trùng, nhập chuỗi Url khác");
                return View(product);
            }
            
            if (ModelState.IsValid)
            {
            
                var user = await _userManager.GetUserAsync(this.User);

                product.DateCreated = product.DateUpdated = DateTime.Now;
                product.AuthorId = user.Id;
                _context.Add(product);

                if(product.CategoryIDs != null){
                    foreach(var p in product.CategoryIDs){
                        _context.Add(new ProductCategoryProduct(){
                            CategoryID = p,
                            Product = product
                        });
                    }
                }

                
                await _context.SaveChangesAsync();
                StatusMessage = "Vừa tạo bài viết mới";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Blog/Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var post = await _context.Posts.FindAsync(id);
            var product = await _context.Products.Include(pc => pc.ProductCategoryProducts).FirstOrDefaultAsync(pc => pc.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }
            var ProductEdit = new CreateProductModel(){
                ProductID = product.ProductID,
                Title = product.Title,
                Content = product.Content,
                Description = product.Description,
                Slug = product.Slug,
                Published = product.Published,
                CategoryIDs = product.ProductCategoryProducts.Select(pc => pc.CategoryID).ToArray(),
                Price = product.Price
            };

            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            
            return View(ProductEdit);
        }

        // POST: Blog/Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Title,Description,Slug,Content,Published, CategoryIDs, Price")] CreateProductModel product)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
             if(product.Slug == null){
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }
            if(await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.ProductID != id)){
                ModelState.AddModelError("Slug","chuỗi Url bị trùng, nhập chuỗi Url khác");
                return View(product);
            }
            
            

            if (ModelState.IsValid)
            {
                try
                {
                     var productUpdate = await _context.Products.Include(pc => pc.ProductCategoryProducts).FirstOrDefaultAsync(pc => pc.ProductID == id);
                    if (productUpdate == null)
                    {
                        return NotFound();
                    }

                    productUpdate.Title = product.Title;
                    productUpdate.Content = product.Content;
                    productUpdate.Description = product.Description;
                    productUpdate.Slug = product.Slug;
                    productUpdate.DateUpdated = DateTime.Now;
                    productUpdate.Published = product.Published;
                    productUpdate.Price = product.Price;

                    // cập nhật PostCategory
                    if (product.CategoryIDs == null) product.CategoryIDs = new int[] {};

                    var oldCateIDs = productUpdate.ProductCategoryProducts.Select(pc => pc.CategoryID).ToArray();
                    var newCateIDs = product.CategoryIDs;  

                    var removeCatePproduct = from productCate in productUpdate.ProductCategoryProducts
                                            where(!newCateIDs.Contains(productCate.CategoryID))
                                            select productCate;
                    _context.ProductCateProducts.RemoveRange(removeCatePproduct);
                    var addCateIDs = from CateId in newCateIDs
                                        where(!oldCateIDs.Contains(CateId))
                                        select CateId;

                    foreach(var cateId in addCateIDs){
                        _context.ProductCateProducts.Add(new ProductCategoryProduct(){
                            ProductID= id,
                            CategoryID = cateId
                        });
                    }       



                    _context.Update(productUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
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
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", product.AuthorId);
            return View(product);
        }

        // GET: Blog/Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Blog/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
                
            _context.Products.Remove(product);
            StatusMessage = "Bạn vừa xóa bài viết" + product.Title;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}
