using System;
using System.ComponentModel.DataAnnotations;

namespace Code.Hub.Shared.Dtos.Inputs
{
    public class CreateOrEditUserInput
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Comment { get; set; }
        public bool RequestPasswordChange { get; set; }
        public bool LoginEnabled { get; set; }
        public DateTime LastLoggedInTime { get; set; }
    }
}
