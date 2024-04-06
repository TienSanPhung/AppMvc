using AppMvc.Models.Product;

namespace AppMvc.Areas.Product.Models{

public class CartItem
{
    public int Quantity {set; get;}
    public ProductModel Product {set; get;}

}
}
