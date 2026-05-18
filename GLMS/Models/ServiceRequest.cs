using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Models
{
    public enum RequestStatus
    {
        [Display(Name = "⏳ Pending")]
        Pending,
        [Display(Name = "✓ Approved")]
        Approved,
        [Display(Name = "🔄 In Progress")]
        InProgress,
        [Display(Name = "✅ Completed")]
        Completed,
        [Display(Name = "❌ Cancelled")]
        Cancelled,
        [Display(Name = "⚠️ On Hold")]
        OnHold
    }

    public enum PriorityLevel
    {
        [Display(Name = "🔴 High")]
        High,
        [Display(Name = "🟡 Medium")]
        Medium,
        [Display(Name = "🟢 Low")]
        Low
    }

    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Contract")]
        public int ContractId { get; set; }
        public virtual Contract? Contract { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 5)]
        [Display(Name = "Service Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount (USD)")]
        [Range(0.01, 999999999.99)]
        public decimal AmountUSD { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount (ZAR)")]
        public decimal AmountZAR { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [Display(Name = "Exchange Rate Used")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRateUsed { get; set; }

        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Required By Date")]
        [DataType(DataType.Date)]
        public DateTime? RequiredByDate { get; set; }

        [Display(Name = "Special Instructions")]
        [Column(TypeName = "ntext")]
        public string? SpecialInstructions { get; set; }

        [Display(Name = "Internal Notes")]
        [Column(TypeName = "ntext")]
        public string? InternalNotes { get; set; }

        [Display(Name = "Priority")]
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

        [Display(Name = "Tracking Number")]
        public string? TrackingNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public bool IsCompleted => Status == RequestStatus.Completed;

        public TimeSpan? GetTimeToComplete()
        {
            if (CompletedAt.HasValue && CreatedAt != null)
            {
                return CompletedAt.Value - CreatedAt;
            }
            return null;
        }
    }
}