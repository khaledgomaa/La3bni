using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ViewModels
{
    public class StatisticsViewModel
    {
        public int NumberOfPlayers { get; set; }

        public int NumberOfOwners { get; set; }

        public int NumberOfStadiums { get; set; }

        public int NumberOfBookings { get; set; }
    }
}