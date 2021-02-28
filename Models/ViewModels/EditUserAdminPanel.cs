﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.ViewModels
{
    public class EditUserAdminPanel
    {
        public EditUserAdminPanel()
        {
            Claims = new List<string>();
            Roles = new List<string>();
        }

        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public City City { get; set; }

        public List<string> Claims { get; set; }

        public IList<string> Roles { get; set; }
    }
}