using AppMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbcontext;

        public DbManageController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
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
    }
}
