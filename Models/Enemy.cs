using System;
using System.Collections.Generic;
using System.Text;

namespace igraUP.Models
{
    public class Enemy
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; } = -0.3;

        public bool IsDead { get; set; } = false;

        public bool IsDying { get; set; } = false;

        public bool IsAwake { get; set; } = false;

        public double PathTimer { get; set; } = 0;

        public (int X, int Y) NextStep { get; set; } = (-1, -1);

        public bool IsAttacking { get; set; } = false;

        public int AnimFrame { get; set; } = 0;

        public double AnimTimer { get; set; } = 0;

        public bool HasFired { get; set; } = false;

        public double ShootTimer { get; set; } = 0;

        public double OriginX { get; set; }

        public double OriginY { get; set; }

        public int Ammo { get; set; } = 3;

        public string State { get; set; } = "idle";

        public double DodgeTimer { get; set; } = 0;

        public double DodgeTargetX { get; set; }

        public double DodgeTargetY { get; set; }
    }
}