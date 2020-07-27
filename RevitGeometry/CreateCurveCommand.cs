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
    public class CreateCurveCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.UI.Selection.Selection selection = commandData.Application.ActiveUIDocument.Selection;
            IList<Reference> select = selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Edge);
            var doc = commandData.Application.ActiveUIDocument.Document;
            var curves = select.Select(x => ((Edge)doc.GetElement(x).GetGeometryObjectFromReference(x)).AsCurve());
            var solids = curves
                //.Where(x => x is Line)
                .Select(x => CreateCylindricalSolidFromLine(x));
            //doc.MakeTransientElements(new TransientElementMaker(() =>
            //{
            var tr = new Transaction(doc, "create solids");
            tr.Start();
            foreach (Solid solid in solids)
            {
                var ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Walls));
                ds.SetShape(new[] { solid });
                var e = ds.GetOptions();
                ICollection<ElementId> elementIds = selection.GetElementIds();
                elementIds.Add(ds.Id);
                selection.SetElementIds(elementIds);
            }
            tr.Commit();
            //}));
            return Result.Succeeded;
        }

        public static Solid CreateCylindricalSolidFromLine(Line line, double radius = 0.1)
        {
            CurveLoop profile = new CurveLoop();
            line = Line.CreateBound(line.GetEndPoint(0), line.GetEndPoint(1));
            var normal = line.Direction;
            var location = line.Origin;
            Plane plane = Plane.CreateByNormalAndOrigin(normal, location);
            Arc arc1 = Arc.Create(plane, radius, 0, Math.PI);
            Arc arc2 = Arc.Create(plane, radius, Math.PI, 2 * Math.PI);
            profile.Append(arc1);
            profile.Append(arc2);
            double length = line.Length;
            var path = new CurveLoop();
            path.Append(line);
            return GeometryCreationUtilities.CreateExtrusionGeometry(new[] { profile }, normal, length);
        }

        public static Solid CreateCylindricalSolidFromLine(Curve curve, double radius = 0.1)
        {
            var trans = curve.ComputeDerivatives(0, true);
            Plane plane = Plane.CreateByNormalAndOrigin(trans.BasisX.Normalize(), curve.GetEndPoint(0));
            CurveLoop profile = CurveLoop.Create(new[] {
                Arc.Create(plane, radius, 0, Math.PI),
                Arc.Create(plane, radius, Math.PI, 2 * Math.PI)
            });
            var path = CurveLoop.Create(new[] { curve });
            return GeometryCreationUtilities.CreateSweptGeometry(path, 0, curve.GetEndParameter(0), new[] { profile });
        }
    }
}
