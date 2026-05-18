using System.ComponentModel.DataAnnotations;

namespace GLMS.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Client name is required")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Region")]
        public string Region { get; set; } = string.Empty;

        [Display(Name = "Tax ID / VAT Number")]
        public string? TaxId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Modified")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation property - A client can have many contracts
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}