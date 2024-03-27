using AppMvc.Models.Blog;
using Microsoft.AspNetCore.Mvc;

namespace AppMvc.Components{

    [ViewComponent]
    public class CategorySideBar : ViewComponent
    {
        public class CategorySidebarData{

            public List<Category> Categories { get; set; }
            public int Level { get; set; }
            public string CateSideBarSlug { get; set; }

        }
        public IViewComponentResult Invoke(CategorySidebarData data){

            return View(data);
        }


    }
}
