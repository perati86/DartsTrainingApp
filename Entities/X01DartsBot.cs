using DartsApp.Enumerations;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Entities
{
    public class X01DartsBot : X01DartsPlayer
    {
        internal readonly Random rn = new Random();

        internal int[] DartsTable = { 20, 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9, 12, 5 };

        public int Level { get; set; }

        // Level 1: 6.5%, Level 20: 54%
        internal double MasterHitChance => 0.04 + Level * 0.025;

        // Level 1: 38%, Level 20: 95%
        internal double SectorHitChance => 0.35 + Level * 0.03;

        // Level 1: 3.81%, Level 20: 0.2%
        internal double BounceOutChance => 0.04 - Level * 0.0019;

        //Level 1: 35%, Level 20: 4.6%
        internal double AdjacentSectorChance => 0.366 - Level * 0.016;

        //Level 1: 15%, Level 20: 0.14%
        internal double TwoInFavourSectorChance => 0.15784 - Level * 0.00784;

        //other random sector chance: Level 1: 10%, Level 20: 0.003%

        //Level 1: 3.4%, Level 20: 30%
        internal double BullHitChance => 0.02 + Level * 0.014;

        //Level 1: 5.95%, Level 20: 52.5%
        internal double GreenHitChance => BullHitChance * 1.75;

        //Level 1: 11.5%, Level 20: 2%
        internal double MasterInsteadOfStraightChance => 0.12 - Level * 0.005;

        internal double DoubleNoScoreChance => 0.4;

        public (int sector, ScoreType scoreType) NextTarget(int score, ThrowType inType, ThrowType outType, int startingPoints, int dartsLeft)
        {
            if (score == startingPoints && inType == ThrowType.Double)
                return (20, ScoreType.Double);

            if (score > 170)
                return (20, ScoreType.Triple);

            List<string> lines = new List<string>();

            string checkoutFilePath = outType == ThrowType.Straight ? "checkouts_straight.txt" : outType == ThrowType.Double ? "checkouts_double.txt" : "checkouts_master.txt";

            Stream fileStream = FileSystem.OpenAppPackageFileAsync(checkoutFilePath).GetAwaiter().GetResult();

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            List<string[]> checkouts = [];

            for (int i = 0; i < lines.Count; i++)
            {
                var row = lines[i].Split(' ');
                
                if (row[0] == score.ToString())
                    checkouts.Add(row);
                    
            }

            if (checkouts.Count == 0)
                return (20, ScoreType.Triple);

            if (checkouts.Where(x => x.Length - 1 == dartsLeft).Any())
            {
                var checkout = checkouts.Where(x => x.Length - 1 == dartsLeft).First();

                return checkout[1][0] == 'T' ? (int.Parse(checkout[1][1..]), ScoreType.Triple)
                            : checkout[1][0] == 'D' ? (int.Parse(checkout[1][1..]), ScoreType.Double)
                            : checkout[1][0] == 'S' ? (int.Parse(checkout[1][1..]), ScoreType.Straight)
                            : (int.Parse(checkout[1]), ScoreType.Straight);
            }

            var onlyCheckout = checkouts.OrderByDescending(x => x.Length).First();

            return onlyCheckout[1][0] == 'T' ? (int.Parse(onlyCheckout[1][1..]), ScoreType.Triple)
                        : onlyCheckout[1][0] == 'D' ? (int.Parse(onlyCheckout[1][1..]), ScoreType.Double)
                        : onlyCheckout[1][0] == 'S' ? (int.Parse(onlyCheckout[1][1..]), ScoreType.Straight)
                        : (int.Parse(onlyCheckout[1]), ScoreType.Straight);
        }

        public (int sector, ScoreType scoreType) PerformThrow(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentNullException("Target was null or empty");

            if (int.TryParse(target[1..], out int targetSector) == false && target.Length != 1)
                throw new ArgumentException("Target was in wrong format");

            double generatedNumber = rn.NextDouble();

            if (generatedNumber < BounceOutChance)
                return (-1, ScoreType.Straight);

            generatedNumber += BounceOutChance;

            if (target[0] == 'T')
                return TripleThrow(targetSector, generatedNumber);

            if (target[0] == 'D' && target[1..] != "25")
                return DoubleThrow(targetSector, generatedNumber);

            if (target[0] == 'D' && target[1..] == "25" || target == "25")
                return BullThrow(targetSector, generatedNumber);

            return StraightThrow(int.Parse(target), generatedNumber);
        }

        private (int, ScoreType) TripleThrow(int target, double generatedNumber)
        {
            if (generatedNumber < MasterHitChance)
                return (target, ScoreType.Triple);

            if (generatedNumber < SectorHitChance)
                return (target, ScoreType.Straight);

            int targetindex = GetSectorIndex(target);

            if (generatedNumber < SectorHitChance + AdjacentSectorChance)
            {
                return rn.Next(0, 2) == 0 ? GetMissedSectorThrow(targetindex - 1, ScoreType.Triple, MasterHitChance)
                    : GetMissedSectorThrow(targetindex + 1, ScoreType.Triple, MasterHitChance);
            }

            if (generatedNumber < SectorHitChance + AdjacentSectorChance + TwoInFavourSectorChance)
            {
                return rn.Next(0, 2) == 0 ? GetMissedSectorThrow(targetindex - 2, ScoreType.Triple, MasterHitChance)
                    : GetMissedSectorThrow(targetindex + 2, ScoreType.Triple, MasterHitChance);
            }

            return (DartsTable[rn.Next(0, DartsTable.Length)], ScoreType.Straight);
        }

        private (int, ScoreType) DoubleThrow(int target, double generatedNumber)
        {
            if (generatedNumber < MasterHitChance)
                return (target, ScoreType.Double);

            if (generatedNumber < MasterHitChance + DoubleNoScoreChance)
                return (0, ScoreType.Straight);

            if (generatedNumber < 0.95 - AdjacentSectorChance - TwoInFavourSectorChance)
                return (target, ScoreType.Straight);

            int targetindex = GetSectorIndex(target);

            if (generatedNumber < 0.95 - TwoInFavourSectorChance)
                return rn.Next(0, 2) == 0 ? GetMissedSectorThrow(targetindex - 1, ScoreType.Double, MasterHitChance)
                    : GetMissedSectorThrow(targetindex + 1, ScoreType.Double, MasterHitChance);

            if (generatedNumber < 0.95)
                return rn.Next(0, 2) == 0 ? GetMissedSectorThrow(targetindex - 2, ScoreType.Double, MasterHitChance)
                    : GetMissedSectorThrow(targetindex + 2, ScoreType.Double, MasterHitChance);

            return (DartsTable[rn.Next(0, DartsTable.Length)], ScoreType.Straight);
        }

        private (int, ScoreType) BullThrow(int target, double generatedNumber)
        {
            if (generatedNumber < BullHitChance)
                return (target, ScoreType.Double);

            if (generatedNumber < BullHitChance + GreenHitChance)
                return (target, ScoreType.Straight);

            return (DartsTable[rn.Next(0, DartsTable.Length)], ScoreType.Straight);
        }

        private (int, ScoreType) StraightThrow(int target, double generatedNumber)
        {
            if (generatedNumber < MasterInsteadOfStraightChance)
                return rn.Next(0, 2) == 0 ? (target, ScoreType.Triple) : (target, ScoreType.Double);

            if (generatedNumber < SectorHitChance)
                return (target, ScoreType.Straight);

            int targetindex = GetSectorIndex(target);

            if (generatedNumber < SectorHitChance + AdjacentSectorChance)
            {
                return rn.Next(0, 2) == 0 ? GetMissedSectorThrow(targetindex - 1, ScoreType.Triple, MasterInsteadOfStraightChance)
                    : GetMissedSectorThrow(targetindex + 1, ScoreType.Triple, MasterInsteadOfStraightChance);
            }

            if (generatedNumber < SectorHitChance + AdjacentSectorChance + TwoInFavourSectorChance)
            {
                return rn.Next(0, 2) == 0 ? GetMissedSectorThrow(targetindex - 2, ScoreType.Triple, MasterInsteadOfStraightChance)
                    : GetMissedSectorThrow(targetindex + 2, ScoreType.Triple, MasterInsteadOfStraightChance);
            }

            return (DartsTable[rn.Next(0, DartsTable.Length)], ScoreType.Straight);
        }

        private int GetSectorIndex(int sector)
        {
            for (int i = 0; i < DartsTable.Length; i++)
            {
                if (DartsTable[i] == sector)
                {
                    return i;
                }
            }

            return 0;
        }

        private (int, ScoreType) GetMissedSectorThrow(int sectorIndex, ScoreType scoreType, double masterHitChance)
        {
            if (sectorIndex > DartsTable.Length - 1)
                sectorIndex -= DartsTable.Length;

            if (sectorIndex < 0)
                sectorIndex += DartsTable.Length;

            var rn2 = new Random(rn.Next());
            double generatedNumber = rn2.NextDouble();

            if (generatedNumber < masterHitChance)
                return (DartsTable[sectorIndex], scoreType);

            return (DartsTable[sectorIndex], ScoreType.Straight);
        }
    }
}
