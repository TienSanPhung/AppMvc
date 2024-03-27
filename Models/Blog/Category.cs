﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMvc.Models.Blog{

[Table("Category")]
public class Category
{
// Id ,parentcategoryId , title, content, slug, categorychildren, parentcategory, 


    [Key]
    public int Id { get; set; }

    // tiêu đề blog
    [Required(ErrorMessage ="Phải có tiêu đề")]
    [StringLength(255,MinimumLength =3,ErrorMessage ="{0} phải từ {1} cho đến {2} ký tự")]
    [Display(Name ="Tiêu Đề")]
    public string Title { get; set; }

    // nội dung thông tin blog
    [DataType(DataType.Text)]
    [Display(Name ="Nội Dung")]
    public string? Description   { get; set; }

    // chuỗi url
    [Required(ErrorMessage = "Phải tạo url")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
    [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
    [Display(Name = "Url hiện thị")]
    public string  Slug { get; set; }
    
    // danh mục con
    public ICollection<Category> CategoryChildren { get; set; }

    // dah mục cha
    [Display(Name ="Danh Mục Cha")]
    public int? ParentCategoryId { get; set; }
    
    // danh mục cha
    [ForeignKey("ParentCategoryId")]
    [Display(Name = "Danh mục cha")]
    public Category ParentCategory { get; set; }

    public void ChildrenCateIDs(ICollection<Category> childrencate , List<int> lists) {
            if(childrencate == null){
                childrencate = this.CategoryChildren;
            }
            foreach(Category childCate in childrencate){
                lists.Add(childCate.Id);
                ChildrenCateIDs(childCate.CategoryChildren,lists);
            }
     }
     public List<Category> ListParents() { 
        List<Category> li = new List<Category>();
        var parent = this.ParentCategory;
        while(parent!=null){
            li.Add(parent);
            parent = parent.ParentCategory;
        }
        li.Reverse();
        return li;
      }
    
}
}
