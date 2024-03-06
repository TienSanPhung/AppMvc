using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AppMvc.Areas.MvcBd.Controllers
{
    [Area("MvcBd")]
    [Route("/MvcBd/[action]")]
    public class CustomerUpdateController : Controller
    {
        public class CustomerInfor
        {
            [Required(ErrorMessage ="Thiếu tên")]
            [StringLength(20,MinimumLength =3,ErrorMessage ="Tên ít nhất {2} ký tự và nhiều nhất {1}")]
            [Display(Name ="Họ và Tên")]
            public string CtName { get; set; }

            [Required(ErrorMessage ="Thiếu Email")]
            [EmailAddress]
            [Display(Name ="Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Thiếu ngày sinh")]
            [Range(1900,2024,ErrorMessage ="người sống trước năm {1} nghẻo hết rồi, và năm nay mới là năm {2} thôi")]
            [Display(Name = "Năm sinh")]
            public int? YearOfBirth { get; set; }
        }

        [BindProperty]
        public CustomerInfor Customer { get; set; }

        [TempData]
        public string Message { get; set; }

        public IActionResult Index()
        {
            // xử lý nếu có dữu liệu binding đến
            if(ModelState.IsValid && Request.Method == HttpMethod.Post.Method)
            {
                Message = "Dữ liệu gửi đến hợp lệ";
                Console.WriteLine(Request.Method);
            }
            else
            {
                Message = "Dữ liệu gửi đến không hợp lệ";
            }
            ViewData["Message"] = Message;
            return View(Customer);
        }
    }
}
