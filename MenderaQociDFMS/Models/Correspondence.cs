using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenderaQociDFMS.Models
{
    public class Correspondence
    {
        [Key]
        public int CorrespondenceId { get; set; }

        [Required(ErrorMessage = "Resident selection is required.")]
        public int ResidentId { get; set; }

        [ForeignKey("ResidentId")]
        public Resident? Resident { get; set; }

        [Required(ErrorMessage = "Document Type is required.")]
        [StringLength(100)]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = string.Empty; // Support Letter, Residence Certificate, ID Verification Letter

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Purpose is required.")]
        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty;

        public string? IssuedByUserId { get; set; }

        [ForeignKey("IssuedByUserId")]
        public ApplicationUser? IssuedByUser { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Reference Number")]
        public string ReferenceNumber { get; set; } = string.Empty;
    }
}
