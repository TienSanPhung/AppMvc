﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppMvc.Models;
using AppMvc.Models.Product;
using Microsoft.AspNetCore.Authorization;
using AppMvc.Data;

namespace AppMvc.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/Product/category/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoryProductController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Blog/Category
        public async Task<IActionResult> Index()
        {
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                    .Where(c => c.ParentCategory == null)
                    .ToList();

            return View(categories);
        }

        // GET: Blog/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public void SelectItem(List<CategoryProduct> source, List<CategoryProduct> des, int level) { 
            string prefix = string.Concat(Enumerable.Repeat("--",level));
            foreach(var cCategory in source){
                //cCategory.Title = prefix + cCategory.Title;
                des.Add(new CategoryProduct(){
                    Id= cCategory.Id,
                    Title = prefix + "" + cCategory.Title
                });
                if(cCategory.CategoryChildren?.Count > 0){
                    SelectItem(cCategory.CategoryChildren.ToList(),des,level +1);
                }
            }
         }
        // GET: Blog/Category/Create
        public  async Task<IActionResult> CreateAsync()
        {
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = ( await qr.ToListAsync())
                    .Where(c => c.ParentCategory == null)
                    .ToList();
            categories.Insert(0,new CategoryProduct(){
                Id = -1,
                Title = "Không có danh mục cha"
            });
            var ilist = new List<CategoryProduct>();
            SelectItem(categories,ilist,0);

            var selectLists = new SelectList(ilist, "Id", "Title");
            ViewData["ParentCategoryId"] = selectLists ;
            return View();
        }

        // POST: Blog/Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] CategoryProduct category)
        {
            
            if (!ModelState.IsValid)
            {
                if(category.ParentCategoryId == -1) category.ParentCategoryId = null;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = ( await qr.ToListAsync())
                    .Where(c => c.ParentCategory == null)
                    .ToList();
            categories.Insert(0,new CategoryProduct(){
                Id = -1,
                Title = "Không có danh mục cha"
            });
            var ilist = new List<CategoryProduct>();
            SelectItem(categories,ilist,0);

            var selectLists = new SelectList(ilist, "Id", "Title");
            ViewData["ParentCategoryId"] = selectLists ;
            return View(category);
        }

        // GET: Blog/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = ( await qr.ToListAsync())
                    .Where(c => c.ParentCategory == null)
                    .ToList();
            categories.Insert(0,new CategoryProduct(){
                Id = -1,
                Title = "Không có danh mục cha"
            });
            var ilist = new List<CategoryProduct>();
            SelectItem(categories,ilist,0);

            var selectLists = new SelectList(ilist, "Id", "Title", category.ParentCategoryId);
            ViewData["ParentCategoryId"] = selectLists ;
            return View(category);
        }

        // POST: Blog/Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] CategoryProduct category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            bool CanUpdate = true;

            if(category.ParentCategoryId == category.Id){
                ModelState.AddModelError(string.Empty,"phải chọn danh mục cha khác");
                CanUpdate = false;
            }

            if(CanUpdate && category.ParentCategoryId != category.Id){
                var CateChildr = (from c in _context.CategoryProducts select c)
                        .Include(c => c.CategoryChildren)
                        .ToList().Where(c => c.ParentCategoryId == category.Id);

                // check với Func
                Func<List<CategoryProduct>, bool> checkCateIds = null;
                checkCateIds = (Cates) => {
                        foreach(var cate in Cates){
                            if(cate.Id == category.ParentCategoryId){
                                CanUpdate = false;
                                ModelState.AddModelError(string.Empty,"Danh mục cha không được là danh mục con của chính nó");
                                return true;
                            }
                            if(cate.CategoryChildren != null){
                                return checkCateIds(cate.CategoryChildren.ToList());
                            }
                        }
                        return false;
                };

                // check cate child
                checkCateIds(CateChildr.ToList());
            }

            if (!ModelState.IsValid && CanUpdate && category.ParentCategoryId != category.Id)
            {
                try
                {
                    if(category.ParentCategoryId == -1) category.ParentCategoryId = null;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);
            var categories = ( await qr.ToListAsync())
                    .Where(c => c.ParentCategory == null)
                    .ToList();
            categories.Insert(0,new CategoryProduct(){
                Id = -1,
                Title = "Không có danh mục cha"
            });
            var ilist = new List<CategoryProduct>();
            SelectItem(categories,ilist,0);

            var selectLists = new SelectList(ilist, "Id", "Title", category.ParentCategoryId);
            ViewData["ParentCategoryId"] = selectLists ;
            return View(category);
        }

        // GET: Blog/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Blog/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.CategoryProducts
                    .Include(c => c.CategoryChildren)
                    .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
               return NotFound();
            }
            foreach(var cCategory in category.CategoryChildren){
                cCategory.ParentCategoryId = category.ParentCategoryId;
            }
            _context.CategoryProducts.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.CategoryProducts.Any(e => e.Id == id);
        }
    }
}
