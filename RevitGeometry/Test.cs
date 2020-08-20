using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitGeometry.Extensions;
using RevitGeometry.Geometry;
using RevitGeometry.Selection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RevitGeometry
{
    [Transaction(TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var sel = commandData.Application.ActiveUIDocument.Selection;
            var e = sel.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new SelectionFilter(x => x is FamilyInstance));
            var tasks = e.Select(x => doc.GetElement(x) as FamilyInstance);
            //var solids = tasks.Select(x => x.get_Geometry(new Options()).First() as Solid);
            var solids = tasks.Select(x => x.GetGeometry().First(y => y.Volume > 0));
            var orientaion = solids.First().Faces.Cast<Face>().Select(x => x.ComputeNormal(UV.Zero)).First(x => x.X > 0 && x.Y > 0);
            var angle = XYZ.BasisX.AngleTo(orientaion);
            var t = Transform.CreateRotation(XYZ.BasisZ, -angle);
            var backT = Transform.CreateRotation(XYZ.BasisZ, angle);
            var tPoints = solids.SelectMany(x => x.Edges.Cast<Edge>().Select(y => y.AsCurve().GetEndPoint(0)).Select(y => t.OfPoint(y)));
            var pointsSolids = tPoints.Select(x => SolidExtensions.GetBoxAroundPoint(x).SolidBoundingBox());

            var min = Min(tPoints);
            var max = Max(tPoints);
            var bbox = new BoundingBoxXYZ
            {
                Min = min,
                Max = max
            };
            //bbox.Transform = backT;
            var solid = bbox.SolidBoundingBox();
            var backSolid = SolidUtils.CreateTransformed(solid, backT);
            using (var tr = new Transaction(doc, "test"))
            {
                tr.Start();
                var ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Floors));
                ds.SetShape(new[] { backSolid });
                tr.Commit();
                System.Windows.Clipboard.SetText(ds.Id.ToString());
            }
            //var view3D = View3D.CreateIsometric(doc, new ElementId(69339));
            //var viewBB = backSolid.GetBoundingBox();
            //XYZ xYZ = new XYZ(1, 1, 1);
            //viewBB.Min -= xYZ;
            //viewBB.Max += xYZ;
            //view3D.SetSectionBox(viewBB);
            return Result.Succeeded;
        }

        private XYZ Max(IEnumerable<XYZ> points)
        {
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var maxZ = double.MinValue;
            foreach (var point in points)
            {
                maxX = point.X > maxX ? point.X : maxX;
                maxY = point.Y > maxY ? point.Y : maxY;
                maxZ = point.Z > maxZ ? point.Z : maxZ;
            }

            return new XYZ(maxX, maxY, maxZ);
        }

        private XYZ Min(IEnumerable<XYZ> points)
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var minZ = double.MaxValue;
            foreach (var point in points)
            {
                minX = point.X < minX ? point.X : minX;
                minY = point.Y < minY ? point.Y : minY;
                minZ = point.Z < minZ ? point.Z : minZ;
            }

            return new XYZ(minX, minY, minZ);
        }

        //public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        //{
        //    var doc = commandData.Application.ActiveUIDocument.Document;
        //    var sel = commandData.Application.ActiveUIDocument.Selection;
        //    var floors = sel.GetElementIds().Select(x => doc.GetElement(x) as Wall);
        //    var solids = floors.Select(x => x.get_Geometry(new Options()).First() as Solid);
        //    var angle = XYZ.BasisX.AngleTo(floors.First().Orientation);
        //    var t = Transform.CreateRotation(XYZ.BasisZ, angle);
        //    var backT = Transform.CreateRotation(XYZ.BasisZ, -angle);
        //    var tPoints = solids.SelectMany(x => x.Edges.Cast<Edge>().Select(y => y.AsCurve().GetEndPoint(0)).Select(y => t.OfPoint(y)));
        //    var min = Min(tPoints);
        //    var max = Max(tPoints);
        //    var bbox = new BoundingBoxXYZ
        //    {
        //        Min = min,
        //        Max = max
        //    };
        //    //bbox.Transform = backT;
        //    var solid = bbox.SolidBoundingBox();
        //    var backSolid = SolidUtils.CreateTransformed(solid, backT);
        //    using (var tr = new Transaction(doc, "test"))
        //    {
        //        tr.Start();
        //        var ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Floors));
        //        ds.SetShape(new[] { backSolid });
        //        tr.Commit();
        //    }
        //    return Result.Succeeded;
        //}
    }
}