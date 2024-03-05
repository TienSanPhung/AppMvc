using AppMvc.Data;
using AppMvc.Models;
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

            var useradmin = await _userManager.FindByEmailAsync("admin");
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

            StatusMessage = "vừa seed data";
            return RedirectToAction("Index");
        }
    }
}
