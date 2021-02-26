using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
    public class CreateRole
    {
        [Required]
        public string RoleName { get; set; }
    }
}