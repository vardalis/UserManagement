using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Models
{
    public class UserModel
    {
        public byte[] RowVersion { get; set; }

        public string Id { get; set; }

        //[Display(Name = "FirstName")]
        [StringLength(50)]
        public string FirstName { get; set; }

        //[Display(Name = "LastName")]
        [StringLength(50)]
        public string LastName { get; set; }

        //[EmailAddress(ErrorMessage = "EmailNotValid")]
        //[Display(Name = "Email")]
        [Required()]
        [EmailAddress()]
        public string Email { get; set; }

        //[Required(ErrorMessage = "PasswordRequired")]
        //[StringLength(100, ErrorMessage = "FieldLengthError", MinimumLength = 1)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        [Required()]
        [StringLength(100, MinimumLength = 1)]
        public string Password { get; set; }

        // [DataType(DataType.Password)]
        // [Display(Name = "ConfirmPassword")]
        // [Compare("Password", ErrorMessage = "PasswordsDontMatch")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        // [Display(Name = "Role")]
        [Required]
        public string Role { get; set; }

        [Required]
        public bool Approved { get; set; }
    }

    public class UpdateUserModel
    {
        public byte[] RowVersion { get; set; }

        public string Id { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [Required()]
        [EmailAddress()]
        public string Email { get; set; }

        [Required]
        [Display(Name = "ApplicationEditingAllowed")]
        public bool ApplicationEditingAllowed { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public bool Approved { get; set; }
    }
}
