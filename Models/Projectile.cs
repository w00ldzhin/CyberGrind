using System;
using System.Collections.Generic;
using System.Text;

namespace igraUP.Models
{
    public class Projectile
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double DirX { get; set; }

        public double DirY { get; set; }

        public bool IsActive { get; set; } = true;
    }
}