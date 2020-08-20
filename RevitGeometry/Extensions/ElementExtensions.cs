using Autodesk.Revit.DB;
using RevitGeometry.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitGeometry.Extensions
{
    public static class ElementExtensions
    {
        public static void SetMaterial(this Element element, ElementId materialId)
        {
            Document doc = element.Document;
            var solids = element.GetGeometry();
            foreach (Solid solid in solids)
            {
                foreach (Face face in solid.GetFaces())
                    doc.Paint(element.Id, face, materialId);
            }
        }

        public static IEnumerable<Solid> GetGeometry(this Element element)
        {
            return element.get_Geometry(GeometryOptions.Options).GetAllSolids();
        }
    }
}
