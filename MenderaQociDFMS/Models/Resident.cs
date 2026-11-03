using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenderaQociDFMS.Models
{
    public class Resident
    {
        [Key]
        public int ResidentId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Sub-Kebele selection is required.")]
        [Display(Name = "Sub-Kebele")]
        public int SubKebeleId { get; set; }

        [ForeignKey("SubKebeleId")]
        public SubKebele? SubKebele { get; set; }

        [Display(Name = "Household Head Name")]
        [StringLength(100)]
        public string? HouseholdHead { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive, Transferred, Deceased

        [Display(Name = "Profile Photo")]
        [StringLength(255)]
        public string? PhotoPath { get; set; }

        // Navigation properties
        public ICollection<KebeleIdRecord> KebeleIdRecords { get; set; } = new List<KebeleIdRecord>();
        public ICollection<Correspondence> Correspondences { get; set; } = new List<Correspondence>();
    }
}
