using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitGeometry.Geometry
{
    public struct Xyz
    {
        public Xyz(double x, double y, double z) => (X, Y, Z) = (x, y, z);
        public Xyz(XYZ xyz) : this(xyz.X, xyz.Y, xyz.Z) { }

        public XYZ XYZ => new XYZ(X, Y, Z);

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public static implicit operator (double x, double y, double z)(Xyz value)
        {
            return (value.X, value.Y, value.Z);
        }

        public static implicit operator Xyz((double x, double y, double z) value)
        {
            return new Xyz(value.x, value.y, value.z);
        }

        public static implicit operator Xyz(XYZ value)
        {
            return (value.X, value.Y, value.Z);
        }

        public static implicit operator XYZ(Xyz value)
        {
            return new XYZ(value.X, value.Y, value.Z);
        }

        public void Deconstruct(out double x, out double y, out double z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public override bool Equals(object obj)
        {
            return obj is Xyz other &&
                   X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }

        public override int GetHashCode()
        {
            int hashCode = 373119288;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }
    }
}
