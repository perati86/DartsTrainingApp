﻿using DartsApp.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Entities
{
    public class X01Game : GameFormat
    {
        public ThrowType InType { get; set; }
        public ThrowType OutType { get; set; }
        public int Points { get; set; }
    }
}
