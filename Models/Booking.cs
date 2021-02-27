using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public int PlaygroundId { get; set; }
        public virtual Playground Playground { get; set; }

        public int? PlaygroundTimesId { get; set; }
        public PlaygroundTimes PlaygroundTimes { get; set; }

        public float Price { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookedDate { get; set; }

        public BookingStatus BookingStatus { get; set; }

        [Range(0, 9)]
        public int MaxNumOfPlayers { get; set; }

        public byte Paid { get; set; }
    }

    public enum BookingStatus
    {
        Private,
        Public
    }
}