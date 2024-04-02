using AppMvc.Data;
using AppMvc.Models;
using AppMvc.Models.Blog;
using AppMvc.Models.Product;

//using AppMvc.Models.Product;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbcontext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbManageController(AppDbContext dbcontext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbcontext = dbcontext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDb() { 

            return View();
        }

        [TempData]
        public string StatusMessage { get; set; }


        // Xóa Db
        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync() { 

            var success =  await _dbcontext.Database.EnsureDeletedAsync();

            StatusMessage = success ? "đã xóa DB thành công" : "không xóa được DB";

            return RedirectToAction(nameof(Index));
        }
        // cập nhật và tạo Db
        [HttpPost]
        public async Task<IActionResult> Migration() { 

             await _dbcontext.Database.MigrateAsync();

            StatusMessage = "Cập nhật Db thành công" ;

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SeedDataAsync() {
            var roleNames = typeof(RoleName).GetFields().ToList();
            foreach(var r in roleNames){
                var rolename = (string)r.GetRawConstantValue();
                var rfound = await _roleManager.FindByNameAsync(rolename);
                if(rfound==null){
                    await _roleManager.CreateAsync(new IdentityRole(rolename));
                }
            }
            // tạo một account admin với pass=123456 email= admin@gmail.com

            var useradmin = await _userManager.FindByEmailAsync("admin@gmail.com");
            if(useradmin == null){
                useradmin =  new AppUser(){
                    UserName = "admin",
                    HomeAdress = "Việt Nam",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(useradmin,"123456");
                await _userManager.AddToRoleAsync(useradmin,RoleName.Administrator);
            }

            
            SeedPostCategory();
            //SeedProductCategory();
            

            StatusMessage = "vừa seed data";
            return RedirectToAction("Index");
        }

        private void SeedProductCategory(){

                _dbcontext.CategoryProducts.RemoveRange(_dbcontext.CategoryProducts.Where(p=>p.Description.Contains("[FakeData]")));
                _dbcontext.Products.RemoveRange(_dbcontext.Products.Where(p=>p.Content.Contains("[FakeData]")));

                _dbcontext.SaveChanges();


                //  Seed Category
                var fakeCategory = new Faker<CategoryProduct>();
                int cm = 1;
                fakeCategory.RuleFor(c => c.Title, fk => $"SP{cm++} " + fk.Lorem.Sentence(1,2).Trim('.'));
                fakeCategory.RuleFor(c => c.Description,fk => fk.Lorem.Sentence(5) + "[FakeData]");
                fakeCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

                var cate1 = fakeCategory.Generate();
                    var cate11 = fakeCategory.Generate();
                    var cate12 = fakeCategory.Generate();
                var cate2 = fakeCategory.Generate();
                    var cate21 = fakeCategory.Generate();
                        var cate211 = fakeCategory.Generate();

                cate11.ParentCategory = cate1;
                cate12.ParentCategory = cate1;
                cate21.ParentCategory = cate2;
                cate211.ParentCategory = cate21;

                var categories = new CategoryProduct[] {cate1,cate2,cate11,cate12,cate21,cate211};
                _dbcontext.CategoryProducts.AddRange(categories);
                // End Seed Category
                

                // Seed Post

                var rCateIndex = new Random();
                int bv = 1;

                var user = _userManager.GetUserAsync(this.User).Result;

                
                var fakerProduct = new Faker<ProductModel>();

                fakerProduct.RuleFor(p => p.AuthorId, f => user.Id);
                fakerProduct.RuleFor(p => p.Content, f => f.Commerce.ProductDescription() + "[FakeData]");
                fakerProduct.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2024,1,1), new DateTime(2024,3,28)));
                fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
                fakerProduct.RuleFor(p => p.Published,f => true);
                fakerProduct.RuleFor(p => p.Slug, f => f.Lorem.Slug());
                fakerProduct.RuleFor(p => p.Title,f => $"Sản Phẩm {bv++}: " + f.Commerce.ProductName());
                fakerProduct.RuleFor(p => p.Price, f => int.Parse(f.Commerce.Price(500,1500,0)));



                List<ProductModel> products = new List<ProductModel>();
                List<ProductCategoryProduct> product_Categories = new List<ProductCategoryProduct>();


                for(int i = 0; i< 50;i++){
                    var product = fakerProduct.Generate();
                    product.DateUpdated = product.DateCreated;
                    products.Add(product);
                    product_Categories.Add(new ProductCategoryProduct(){
                        Product = product,
                        Category = categories[rCateIndex.Next(5)]
                    });
                }
                _dbcontext.AddRange(products);
                _dbcontext.AddRange(product_Categories);
                // End Seed Post


                _dbcontext.SaveChangesAsync();

        }
      
        private void SeedPostCategory(){

                _dbcontext.Categories.RemoveRange(_dbcontext.Categories.Where(c=>c.Description.Contains("[FakeData]")));
                _dbcontext.Posts.RemoveRange(_dbcontext.Posts.Where(c=>c.Content.Contains("[FakeData]")));

                _dbcontext.SaveChanges();


                //  Seed Category
                var fakeCategory = new Faker<Category>();
                int cm = 1;
                fakeCategory.RuleFor(c => c.Title, fk => $"CM{cm++} " + fk.Lorem.Sentence(1,2).Trim('.'));
                fakeCategory.RuleFor(c => c.Description,fk => fk.Lorem.Sentence(5) + "[FakeData]");
                fakeCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

                var cate1 = fakeCategory.Generate();
                    var cate11 = fakeCategory.Generate();
                    var cate12 = fakeCategory.Generate();
                var cate2 = fakeCategory.Generate();
                    var cate21 = fakeCategory.Generate();
                        var cate211 = fakeCategory.Generate();

                cate11.ParentCategory = cate1;
                cate12.ParentCategory = cate1;
                cate21.ParentCategory = cate2;
                cate211.ParentCategory = cate21;

                var categories = new Category[] {cate1,cate2,cate11,cate12,cate21,cate211};
                _dbcontext.Categories.AddRange(categories);
                // End Seed Category
                

                // Seed Post

                var rCateIndex = new Random();
                int bv = 1;

                var user = _userManager.GetUserAsync(this.User).Result;
                var fakerPost = new Faker<Post>();

                fakerPost.RuleFor(p => p.AuthorId, f => user.Id);
                fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[FakeData]");
                fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2024,1,1), new DateTime(2024,3,28)));
                fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
                fakerPost.RuleFor(p => p.Published,f => true);
                fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
                fakerPost.RuleFor(p => p.Title,f => $"Bài {bv++}: " + f.Lorem.Sentence(3,4).Trim('.'));



                List<Post> posts = new List<Post>();
                List<PostCategory> post_Categories = new List<PostCategory>();


                for(int i = 0; i< 50;i++){
                    var post = fakerPost.Generate();
                    post.DateUpdated = post.DateCreated;
                    posts.Add(post);
                    post_Categories.Add(new PostCategory(){
                        Post = post,
                        Category = categories[rCateIndex.Next(5)]
                    });
                }
                _dbcontext.AddRange(posts);
                _dbcontext.AddRange(post_Categories);
                // End Seed Post


                _dbcontext.SaveChangesAsync();

        }
    }   
        
}
