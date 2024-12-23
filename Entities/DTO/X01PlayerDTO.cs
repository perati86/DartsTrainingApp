using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Entities.DTO
{
    public class X01PlayerDTO
    {
        [Key]
        public Guid ID { get; set; }
        public Guid MatchID { get; set; }
        public string Name {  get; set; }
        public bool LegWon { get; set; }
        public int Points { get; set; }
        public int First9DartsPoints { get; set; }
        public int DartsThrown { get; set; }
        public int CheckOutDartsThrown { get; set; }
        public int Checkout { get; set; }
        public int HighestScoreThisLeg { get; set; }
        public int HighestScoreCountThisLeg { get; set; }
        public int[] HighThrowCounts { get; set; }
        public DateTime PlayedDateTime { get; set; }
    }
}
