


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMvc.Models.Product{

    [Table("ProductPhoto")]
    public class ProductPhoto
    {
        [Key]
        public int ID { get; set; }

        // file name 123.png, asbc.jpeg ...
        // => /contents/Products/123.png ....
        public string FileName { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Products { get; set; }

    }

}
