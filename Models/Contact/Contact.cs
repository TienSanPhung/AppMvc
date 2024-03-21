using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMvc.Models.Contacts
{
    public class Contact
    {
        [Key]
        public int ID { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        [Required(ErrorMessage ="Không được để trống")]
        [Display(Name ="Họ và Tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage ="Không được để trống")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage ="Định dạng email không đúng")]
        [Display(Name ="Địa chỉ Email")]
        public string Email { get; set; }
        public DateTime? DateSenT { get; set; }

        [Display(Name ="Nội dung")]
        public string?    Message { get; set; }
        [StringLength(50)]
        [Display(Name ="Số Điện Thoại")]
        [Phone(ErrorMessage ="Định dạng số điện thoại không đúng")]
        public string? PhoneNumber {get;set;}
    }
}
