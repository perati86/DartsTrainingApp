using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Entities.DTO
{
    public class DoublesPracticePlayerDTO
    {
        [Key]
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string D20Statistics { get; set; }
        public string D16Statistics { get; set; }
        public string D10Statistics { get; set; }
        public string D8Statistics { get; set; }
        public string D4Statistics { get; set; }
        public DateTime PlayedDateTime { get; set; }
    }
}
