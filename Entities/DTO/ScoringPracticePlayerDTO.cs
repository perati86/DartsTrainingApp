using System;
using System.ComponentModel.DataAnnotations;

namespace DartsApp.Entities.DTO
{
    public class ScoringPracticePlayerDTO
    {
        [Key]
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string T20Statistics { get; set; }
        public string T19Statistics { get; set; }
        public string T18Statistics { get; set; }
        public DateTime PlayedDateTime { get; set; }
    }
}
