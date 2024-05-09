using System;
using System.Collections.Generic;
using System.Text;

namespace CAPISampleApplication
{
    public class Tube
    {
        // Inner diameter [m]
        public double id { get; set; }

        // Outer diameter [m]
        public double od { get; set; }

        // Bending radius [m]
        public double r { get; set; }

        // Curvature [m^-1]
        public double k { get; set; }

        // Straight section length [m]
        public double l { get; set; }

        // Curved section length [m]
        public double d { get; set; }

        // Young's modulus [Pa]
        public double E { get; set; }

        // Second moment of area [m^4]
        public double I { get; set; }

        // I don't remember. Not needed unless kinematics *including torsion* is used
        public double J { get; set; }

        // I don't remember. Not needed unless kinematics *including torsion* is used
        public double G { get; set; }

        public Tube(double id, double od, double r, double l, double d, double E)
        {
            this.id = id;
            this.od = od;
            this.r = r;
            this.k = 1 / r;
            this.l = l;
            this.d = d;
            this.E = E;
            this.I = (Math.PI / 64) * (Math.Pow(od, 4) - Math.Pow(id, 4));
            this.J = 2 * this.I;
            this.G = this.E / (2 * (1 + 0.217));
        }

    }
}
