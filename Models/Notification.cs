using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
    public class Notification
    {
        public int NotificationId { set; get; }

        [Required]
        public string Title { set; get; }

        [Required]
        public string Body { set; get; }

        public byte Seen { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}