using AppMvc.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace AppMvc.Components{

    [ViewComponent]
    public class ProductSideBar : ViewComponent
    {
        public class ProductSideBarData
        {

            public List<CategoryProduct> Categories { get; set; }
            public int Level { get; set; }
            public string CateSideBarSlug { get; set; }

        }
        public IViewComponentResult Invoke(ProductSideBarData data){

            return View(data);
        }


    }
}
