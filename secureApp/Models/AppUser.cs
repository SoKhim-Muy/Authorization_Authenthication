using System.ComponentModel.DataAnnotations;

namespace MyCodeFirstProject.Models
{
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "សូមបញ្ចូលឈ្មោះពេញ")]
        [StringLength(100)]
        [Display(Name = "ឈ្មោះពេញ")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "សូមបញ្ចូល Email")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = "";

        [Display(Name = "ស្ថានភាព")]
        public bool IsActive { get; set; } = true;
    }
}