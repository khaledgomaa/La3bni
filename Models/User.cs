using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
    public class User
    {
        [Required(ErrorMessage = "You must choose your type")]
        public UserType UserType { get; set; }

        [Required(ErrorMessage = "You must enter your User Name")]
        [Display(Name = "User Name")]
        [RegularExpression(@"^[a-zA-Z0-9]+$",ErrorMessage ="User Name should contains only letters or digits")]
        [Remote(action: "Name_Unique", controller: "Account", ErrorMessage = "this Username is already taken please try a different one")]
        public string Username { get; set; }

        [Required(ErrorMessage = "You must enter your email")]
        [EmailAddress]
        [Remote(action: "Email_Unique", controller: "Account", ErrorMessage = "this email is already rergistered")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must enter your Password")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "password length must be more the 5")]
        public string Password { get; set; }

        [Required(ErrorMessage = "You must confirm  your Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password doesn't match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "You must choose  your city")]
        public City City { get; set; }

        [Required(ErrorMessage = "You must choose  your Gender")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "You must Enter  your Phone number")]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [MinLength(11, ErrorMessage = "Not valid Number"), MaxLength(11, ErrorMessage = "Not valid Number")]
        public string PhoneNumber { get; set; }

        public string ImagePath { get; set; }

        [NotMapped]
        [DisplayName("Upload Image")]
        
        public IFormFile ImageFile { get; set; }
    }

    public enum UserType
    {
        Owner,
        Player
    }
}