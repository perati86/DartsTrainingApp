using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Entities
{
    public class X01DartsPlayer : DartsPlayer
    {
        public int TotalPoints { get; set; }
        public int CurrentPoints { get; set; }
        public int First9DartsPoints { get; set; }
        public int TotalDartsThrown { get; set; }
        public int DartsThrownThisLeg { get; set; }
        public int Checkout { get; set; }
        public int TotalCheckoutDartsThrown { get; set; }
        public int CheckoutDartsThrownThisLeg { get; set; }
        public int HighestScoreThisLeg { get; set; }
        public int HighestScoreCountThisLeg {  get; set; }
        public int[] HighThrowCounts { get; set; } = [0, 0, 0, 0];
        public double Average => Math.Round(3 * (double)TotalPoints / TotalDartsThrown, 2);
    }
}
