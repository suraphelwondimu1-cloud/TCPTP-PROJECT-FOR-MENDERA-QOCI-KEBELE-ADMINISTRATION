using System.ComponentModel.DataAnnotations;

namespace MenderaQociDFMS.ViewModels
{
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role assignment is required.")]
        public string Role { get; set; } = "Staff";
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string Role { get; set; } = "Staff";

        public bool IsActive { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password (Optional)")]
        public string? NewPassword { get; set; }
    }
}
