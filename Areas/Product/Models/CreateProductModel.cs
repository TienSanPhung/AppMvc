using System.ComponentModel.DataAnnotations;
using AppMvc.Models.Product;

namespace AppMvc.Areas.Product.Models
{
    public class CreateProductModel : ProductModel
    {
        [Display(Name = "Chuyên mục")]
        public int[] CategoryIDs { get; set; }
    }
}
