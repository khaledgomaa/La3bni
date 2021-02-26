using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class BookingTeam
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}