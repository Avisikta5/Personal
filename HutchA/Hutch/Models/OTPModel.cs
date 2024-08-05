using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Hutch.Models
{
    public class OTPModel
    {
        [Display(Name = "Code")]
        [Required(ErrorMessage = "Code is required")]
        [StringLength(6,MinimumLength = 6, ErrorMessage ="Invalid code")]
        public string OTP { get; set; }
    }
}