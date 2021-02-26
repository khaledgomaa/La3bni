using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
    public class PlaygroundTimes
    {
        public int PlaygroundTimesId { get; set; }

        public int PlaygroundId { get; set; }
        public virtual Playground Playground { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime From { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime To { get; set; }

        public State State { get; set; }

        public override string ToString()
        {
            return $"{From:HH:mm} - {To:HH:mm} {State}";
        }
    }

    public enum State
    {
        AM,
        PM
    }
}