using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
    public class Playground
    {
        public int PlaygroundId { get; set; }

        [Display(Name = "OwnerId")]
        public string ApplicationUserId { get; set; }

        [Display(Name = "Owner")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public City City { get; set; }

        [Required]
        public string OverView { get; set; }

        public int StadiumArea { get; set; }

        public int NumOfPlayers { get; set; }

        public float AmPrice { get; set; }

        public float PmPrice { get; set; }

        [Required]
        public Services Services { get; set; }

        public float Rate { get; set; }

        [Required]
        public Status PlaygroundStatus { get; set; }

        public byte IsOffered { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Required]
        public string ImagePath { get; set; }

        [NotMapped]
        [DisplayName("Upload Image")]
        public IFormFile ImageFile { get; set; }
    }

    public enum City
    {
        Cairo,
        Giza,
        Alexandria,
        Benisuef,
        Banha
    }

    [Flags]
    public enum Services
    {
        WIFI = 1,
        WC = 2,
        Cafteria = 4,
    }

    public enum Status
    {
        Available,
        Busy
    }
}