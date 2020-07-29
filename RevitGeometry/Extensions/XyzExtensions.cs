using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitGeometry.Extensions
{
    public static class XyzExtensions
    {
        public static void Deconstruct(this XYZ xyz, out double x, out double y, out double z)
        {
            x = xyz.X;
            y = xyz.Y;
            z = xyz.Z;
        }
    }
}
