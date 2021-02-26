using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
    public class LogIN
    {
        [Required(ErrorMessage = "You must enter  your email or  username")]
        [Display(Name = "User Name or Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must enter  your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool rememberMe { get; set; }
    }
}