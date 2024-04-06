//using System.Data.Entity;
using System.Linq;
using AppMvc.Areas.Product.Models;
using AppMvc.Areas.Product.Service;
using AppMvc.Models;
using AppMvc.Models.Blog;
using AppMvc.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Areas.Product.Controllers
{

    [Area("Product")]
    public class ViewProductController : Controller
    {
        private readonly ILogger<ViewProductController> _logger;
        private readonly AppDbContext _context;
        private readonly CartService _cartservice;

        [TempData]
        public string StatusMessage { get; set; }

        public ViewProductController(ILogger<ViewProductController> logger, AppDbContext context, CartService cartservice)
        {
            _logger = logger;
            _context = context;
            _cartservice = cartservice;
        }

        [Route("/product/{categoryslug?}")]

        public IActionResult Index(string categoryslug, [FromQuery(Name ="p")]int currentPage, int pagingSize)
        {
            var categories = GetCategory();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;

            CategoryProduct category = null;
            if(!string.IsNullOrEmpty(categoryslug)){
                category =  _context.CategoryProducts.Where(c => c.Slug == categoryslug)
                            .Include(c => c.CategoryChildren)
                            .FirstOrDefault();
                if(category == null){
                    return NotFound("Không thấy danh mục sản phẩm nào");
                }
            }

            var products = _context.Products
                        .Include(p => p.Author)
                        .Include(p => p.ProductPhotos)
                        .Include(p => p.ProductCategoryProducts)
                        .ThenInclude(p => p.Category)
                        .AsQueryable();
            products = products.OrderByDescending(p => p.DateUpdated);

            if(category != null){
                var ids = new List<int>();
                category.ChildrenCateIDs(null,ids);
                ids.Add(category.Id);
                products = products.Where(p => p.ProductCategoryProducts.Where(p => ids.Contains(p.CategoryID)).Any());
            }


            int totalproduct =  products.Count();
            if(pagingSize < 10) pagingSize = 6 ;
            int countPages = (int)Math.Ceiling((double)totalproduct/ pagingSize);

            
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

            var productsInPage = products.Skip((currentPage - 1) * pagingSize)
                                .Take(pagingSize);

            ViewBag.category = category;

            

            return View(productsInPage.ToList());
        }

        [Route("/product/{productslug}.html")]
        public IActionResult Details(string productslug) { 
            
            var categories = GetCategory();
            ViewBag.categories = categories;

            var product = _context.Products.Where(p => p.Slug == productslug)
                                    .Include(p => p.Author)
                                    .Include(p=>p.ProductPhotos)
                                    .Include(p => p.ProductCategoryProducts)
                                    .ThenInclude(p => p.Category)
                                    .FirstOrDefault();
            if(product == null){
                return NotFound("không thấy sản phẩm");
            }
            
            CategoryProduct category = product.ProductCategoryProducts.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherProduct = _context.Products.Where(p => p.ProductCategoryProducts.Any(c => c.Category.Id == category.Id))
                                            .Where(p => p.ProductID != product.ProductID)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);

            ViewBag.otherProduct = otherProduct;
            return View(product);
        }

        private List<CategoryProduct> GetCategory(){
            var categories = _context.CategoryProducts
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategoryId == null)
                            .ToList();
            return categories;
        }
        /// Thêm sản phẩm vào cart
        [Route ("addcart/{productid:int}", Name = "addcart")]
        public IActionResult AddToCart ([FromRoute] int productid) {

            var product = _context.Products
                .Where (p => p.ProductID == productid)
                .FirstOrDefault ();
            if (product == null)
                return NotFound ("Không có sản phẩm");

            // Xử lý đưa vào Cart ...
            var cart = _cartservice.GetCartItems();
            var cartitem = cart.Find (p => p.Product.ProductID == productid);
            if (cartitem != null) {
                // Đã tồn tại, tăng thêm 1
                cartitem.Quantity++;
            } else {
                //  Thêm mới
                cart.Add (new CartItem () { Quantity = 1, Product = product });
            }

            // Lưu cart vào Session
            _cartservice.SaveCartSession (cart);
            // Chuyển đến trang hiện thị Cart
            return RedirectToAction (nameof (Cart));
        }

        // Hiện thị giỏ hàng
        [Route ("/cart", Name = "cart")]
        public IActionResult Cart () {
            return View (_cartservice.GetCartItems());
        }

        /// Cập nhật
        [Route ("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart ([FromForm] int productid, [FromForm] int quantity) {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartservice.GetCartItems ();
            var cartitem = cart.Find (p => p.Product.ProductID == productid);
            if (cartitem != null) {
                // Đã tồn tại, tăng thêm 1
                cartitem.Quantity = quantity;
            }
            _cartservice.SaveCartSession (cart);
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok();
        }

        /// xóa item trong cart
        [Route ("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart ([FromRoute] int productid) {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartservice.GetCartItems ();
            var cartitem = cart.Find (p => p.Product.ProductID == productid);
            if (cartitem != null) {
                 // Đã tồn tại, tăng thêm 1
                cart.Remove(cartitem);
            }

            _cartservice.SaveCartSession (cart);
            return RedirectToAction (nameof (Cart));
        }

        
        [Route ("/checkout")]
        public IActionResult Checkout ([FromRoute] int productid) {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartservice.GetCartItems ();
            // xử lý gửi hóa đơn đến email
            // xử lý lưu hóa đơn vào cơ sở dữ liệu

            _cartservice.ClearCart();
            StatusMessage = "đã gửi đơn hàng";
            return RedirectToAction(nameof(Index));
        }
    }
}
