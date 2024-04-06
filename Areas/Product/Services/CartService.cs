

using AppMvc.Areas.Product.Models;
using Newtonsoft.Json;

namespace AppMvc.Areas.Product.Service{

    public class CartService
    {
        public const string CARTKEY = "cart";
        private readonly IHttpContextAccessor _context;

        private readonly HttpContext httpContext;

        public CartService(IHttpContextAccessor context)
        {
            _context = context;
            httpContext = context.HttpContext;

        }

        

        // Lấy cart từ Session (danh sách CartItem)
        public List<CartItem> GetCartItems () {

            var session = httpContext.Session;
            string jsoncart = session.GetString (CARTKEY);
            if (jsoncart != null) {
                return JsonConvert.DeserializeObject<List<CartItem>> (jsoncart);
            }
            return new List<CartItem> ();
        }

        // Xóa cart khỏi session
        public void ClearCart () {
            var session = httpContext.Session;
            session.Remove (CARTKEY);
        }

        // Lưu Cart (Danh sách CartItem) vào session
        public void SaveCartSession (List<CartItem> ls) {
            var session = httpContext.Session;
            string jsoncart = JsonConvert.SerializeObject (ls);
            session.SetString (CARTKEY, jsoncart);
        }
          

    }
}