using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Hutch.Models
{
    public class NewPasswordModel
    {
        [Display(Name = "New Password")]
        [Required(ErrorMessage = "New Password required", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength =6, ErrorMessage ="Password should be 6 characters atlest")]
        public string NewPassword { get; set;}

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set;}
    }
}