using DartsApp.Entities;
using DartsApp.Entities.DTO;
using DartsApp.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Services
{
    public class DbDataContext : DbContext
    {
        public DbSet<X01PlayerDTO> X01Legs { get; set; }
        public DbSet<CricketPlayerDTO> CricketLegs { get; set; }
        public DbSet<ScoringPracticePlayerDTO> ScoringPracticeLegs { get; set; }
        public DbSet<DoublesPracticePlayerDTO> DoublesPracticeLegs { get; set; }
        public DbDataContext()
        {
            SQLitePCL.Batteries_V2.Init();
            this.Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "legs.db13");

            optionsBuilder
              .UseSqlite($"Filename={dbPath}");
        }
    }
}
