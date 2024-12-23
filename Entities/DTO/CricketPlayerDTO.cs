using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Entities.DTO
{
    public class CricketPlayerDTO
    {
        [Key]
        public Guid ID { get; set; }
        public Guid MatchID { get; set; }
        public string Name { get; set; }
        public bool LegWon { get; set; }
        public int Points { get; set; }
        public int Marks { get; set; }
        public int Rounds { get; set; }
        public DateTime PlayedDateTime { get; set; }
    }
}
