using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitGeometry.Extensions
{
    public static class LineExtensions
    {
        /// <summary>
        /// Recreates line for proper properties.
        /// </summary>
        /// <param name="line">Line to fix.</param>
        /// <returns></returns>
        public static Line Fix(this Line line)
        {
            if (line.IsBound)
                return Line.CreateBound(line.GetEndPoint(0), line.GetEndPoint(1));
            else
                return line;
        }
    }
}
