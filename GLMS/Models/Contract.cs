using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace GLMS.Models
{
    public enum ContractStatus
    {
        [Display(Name = "📄 Draft")]
        Draft,
        [Display(Name = "✅ Active")]
        Active,
        [Display(Name = "❌ Expired")]
        Expired,
        [Display(Name = "⏸️ On Hold")]
        OnHold
    }

    public enum ServiceLevel
    {
        [Display(Name = "⭐ Standard")]
        Standard,
        [Display(Name = "⭐⭐ Premium")]
        Premium,
        [Display(Name = "⭐⭐⭐ Enterprise")]
        Enterprise
    }

    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Contract Reference")]
        [StringLength(50)]
        public string ContractReference { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Client")]
        [Display(Name = "Client")]
        public int ClientId { get; set; }
        public virtual Client? Client { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "Contract Status")]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        [Display(Name = "Service Level")]
        public ServiceLevel ServiceLevel { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Contract Value (USD)")]
        [Range(0, 999999999.99)]
        public decimal ContractValue { get; set; }

        [Display(Name = "Signed Agreement Path")]
        public string? SignedAgreementPath { get; set; }

        [NotMapped]
        [Display(Name = "Signed Agreement File")]
        public IFormFile? SignedAgreement { get; set; }

        [Display(Name = "Terms and Conditions")]
        [Column(TypeName = "ntext")]
        public string? TermsAndConditions { get; set; }

        [Display(Name = "Special Clauses")]
        [Column(TypeName = "ntext")]
        public string? SpecialClauses { get; set; }

        [Display(Name = "Auto-Renew")]
        public bool AutoRenew { get; set; } = false;

        [Display(Name = "Notice Period (Days)")]
        [Range(0, 365)]
        public int NoticePeriodDays { get; set; } = 30;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        public bool IsActive()
        {
            return Status == ContractStatus.Active &&
                   StartDate <= DateTime.UtcNow &&
                   EndDate >= DateTime.UtcNow;
        }

        public bool CanCreateServiceRequest()
        {
            return Status == ContractStatus.Active && IsActive();
        }

        public int GetRemainingDays()
        {
            if (EndDate < DateTime.UtcNow) return 0;
            return (int)(EndDate - DateTime.UtcNow).TotalDays;
        }

        public string GetStatusColor()
        {
            return Status switch
            {
                ContractStatus.Active => "success",
                ContractStatus.Expired => "danger",
                ContractStatus.OnHold => "warning",
                _ => "secondary"
            };
        }
    }
}