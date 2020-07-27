using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitGeometry
{
    [Transaction(TransactionMode.Manual)]
    public class JeremyFirst : IExternalCommand
    {
        /// <summary>
        /// Set this to true to iterate through smaller 
        /// and smaller tetrahedron sizes until we hit
        /// Revit's precision limit.
        /// </summary>

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // Find GraphicsStyle

            FilteredElementCollector collector
              = new FilteredElementCollector(doc)
                .OfClass(typeof(GraphicsStyle));

            GraphicsStyle style = collector.Cast<GraphicsStyle>()
              .FirstOrDefault(gs => gs.Name.Equals("<Sketch>"));

            ElementId graphicsStyleId = null;

            if (style != null)
            {
                graphicsStyleId = style.Id;
            }

            // Modify document within a transaction

            using (var tx = new Transaction(doc))
            {
                tx.Start("Create DirectShape");

                double length = 10; // foot
                List<XYZ> args = new List<XYZ>(3);

                TessellatedShapeBuilder builder = new TessellatedShapeBuilder();

                builder.OpenConnectedFaceSet(false);

                args.Add(XYZ.Zero);
                args.Add(length * XYZ.BasisX);
                args.Add(length * XYZ.BasisY);
                builder.AddFace(new TessellatedFace(args, ElementId.InvalidElementId));

                args.Clear();
                args.Add(XYZ.Zero);
                args.Add(length * XYZ.BasisX);
                args.Add(length * XYZ.BasisZ);
                builder.AddFace(new TessellatedFace(args, ElementId.InvalidElementId));

                args.Clear();
                args.Add(XYZ.Zero);
                args.Add(length * XYZ.BasisY);
                args.Add(length * XYZ.BasisZ);
                builder.AddFace(new TessellatedFace(args, ElementId.InvalidElementId));

                args.Clear();
                args.Add(length * XYZ.BasisX);
                args.Add(length * XYZ.BasisY);
                args.Add(length * XYZ.BasisZ);
                builder.AddFace(new TessellatedFace(args, ElementId.InvalidElementId));

                builder.CloseConnectedFaceSet();
                builder.Build();
                TessellatedShapeBuilderResult result
                  = builder.GetBuildResult();

                ElementId categoryId = new ElementId(
                  BuiltInCategory.OST_GenericModel);

                DirectShape ds = DirectShape.CreateElement(doc, categoryId);

                ds.SetShape(result.GetGeometricalObjects());

                ds.Name = "Test";
                commandData.Application.ActiveUIDocument.Selection.SetElementIds(new[] { ds.Id });
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
