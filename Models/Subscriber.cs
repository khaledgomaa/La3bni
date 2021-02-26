using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
    public class Subscriber
    {
        public int SubscriberId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}