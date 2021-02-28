using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ViewModels
{
    public class BookingViewModel
    {
        public bool BookingExist { get; set; }
        public int NumOfPlayers { get; set; }

        public BookingStatus BookingStatus { get; set; }

        public Status PlaygroundStatus { get; set; }

        public int MaxNumOfPlayers { get; set; }

        public int BookingId { get; set; }

        public bool BookingOwner { get; set; }

        public byte Paid { get; set; }
    }
}