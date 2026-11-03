using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenderaQociDFMS.Models
{
    public class KebeleIdRecord
    {
        [Key]
        public int IdRecordId { get; set; }

        [Required]
        public int ResidentId { get; set; }

        [ForeignKey("ResidentId")]
        public Resident? Resident { get; set; }

        [Required(ErrorMessage = "Kebele ID Number is required.")]
        [StringLength(50)]
        [Display(Name = "ID Number")]
        public string IdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Issue Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Expiry Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddYears(2);

        [Required]
        [StringLength(20)]
        [Display(Name = "ID Status")]
        public string IdStatus { get; set; } = "Active"; // Active, Expired, Cancelled

        public string? IssuedByUserId { get; set; }

        [ForeignKey("IssuedByUserId")]
        public ApplicationUser? IssuedByUser { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }
    }
}
