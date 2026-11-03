using System.ComponentModel.DataAnnotations;

namespace MenderaQociDFMS.Models
{
    public class SubKebele
    {
        [Key]
        public int SubKebeleId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Sub-Kebele Name")]
        public string SubKebeleName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Resident> Residents { get; set; } = new List<Resident>();
    }
}
