using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
    public class FeedBack
    {
        public int FeedBackId { get; set; }

        [Required(ErrorMessage = "Please Enter your Name!")]
        [Display(Name = "Name"), MinLength(7, ErrorMessage = "Name must be more than 7 charachters!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please Enter your Email Address!")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please Enter Message to send!")]
        [Display(Name = "Message"), MinLength(20, ErrorMessage = "Message must be at least 20 charachters!")]
        public string Message { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Please Choose Email Address!")]
        public List<string> SelectedEmails { get; set; }
    }
}