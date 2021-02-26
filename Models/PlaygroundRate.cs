using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
    public class PlaygroundRate
    {
        public float Rate { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public int PlaygroundId { get; set; }

        public virtual Playground Playground { get; set; }

        public override bool Equals(object obj)
        {
            PlaygroundRate playgroundRate = obj as PlaygroundRate;
            if (playgroundRate == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            if (object.ReferenceEquals(this, obj)) return false;
            return playgroundRate.ApplicationUser == this.ApplicationUser
                   && playgroundRate.Playground == this.Playground
                   && playgroundRate.Rate == this.Rate;
        }
    }
}