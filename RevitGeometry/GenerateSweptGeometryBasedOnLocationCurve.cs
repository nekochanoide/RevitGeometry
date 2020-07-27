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
    [Regeneration(RegenerationOption.Manual)]
    public class GenerateSweptGeometryBasedOnLocationCurve : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Autodesk.Revit.UI.Selection.Selection sel = commandData.Application.ActiveUIDocument.Selection;
            Reference reference = sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            Transaction trans = new Transaction(doc);
            Element elem = doc.GetElement(reference);
            trans.Start("Create solid");
            LocationCurve lc = elem.Location as LocationCurve;
            if (lc == null)
            {
                throw new InvalidOperationException("Current element is not a curve-dirven element");

            }
            //Create the sweep path
            var pathCurveLoop = new CurveLoop();
            Curve curve = lc.Curve;
            pathCurveLoop.Append(curve);
            //get the point to create a plane for the profile.
            XYZ xyzFirstPoint = curve.GetEndPoint(0);
            //Create the plane
            Transform transform = curve.ComputeDerivatives(0, true);
            XYZ xyzDirection = transform.BasisX;
            Plane profilePlane = Plane.CreateByNormalAndOrigin(xyzDirection.Normalize(), transform.Origin);
            //Create a profile which is a rectangle with height and width of 2000mm and 900mm

            double dblHeight = 2000 / 304.8;
            double dblWidth = 900 / 304.8;
            UV uvPt0 = new UV(0, 0) - new UV(dblWidth / 2, 0);
            UV uvPt1 = uvPt0 + new UV(dblWidth, 0);
            UV uvPt2 = uvPt1 + new UV(0, dblHeight);
            UV uvPt3 = uvPt2 - new UV(dblWidth, 0);
            XYZ xVec = profilePlane.XVec;
            XYZ yVec = profilePlane.YVec;
            XYZ xyzPt0 = profilePlane.Origin + profilePlane.XVec.Multiply(uvPt0.U) + profilePlane.YVec.Multiply(uvPt0.V);
            XYZ xyzPt1 = profilePlane.Origin + profilePlane.XVec.Multiply(uvPt1.U) + profilePlane.YVec.Multiply(uvPt1.V);
            XYZ xyzPt2 = profilePlane.Origin + profilePlane.XVec.Multiply(uvPt2.U) + profilePlane.YVec.Multiply(uvPt2.V);
            XYZ xyzPt3 = profilePlane.Origin + profilePlane.XVec.Multiply(uvPt3.U) + profilePlane.YVec.Multiply(uvPt3.V);
            Line l1 = Line.CreateBound(xyzPt0, xyzPt1);
            Line l2 = Line.CreateBound(xyzPt1, xyzPt2);
            Line l3 = Line.CreateBound(xyzPt2, xyzPt3);
            Line l4 = Line.CreateBound(xyzPt3, xyzPt0);
            CurveLoop loop = new CurveLoop();
            loop.Append(l1);
            loop.Append(l2);
            loop.Append(l3);
            loop.Append(l4);
            List<CurveLoop> newloops = new List<CurveLoop>() { loop };
            //Create the swept solid
            Solid sweepSolid = GeometryCreationUtilities.CreateSweptGeometry(pathCurveLoop, 0, lc.Curve.GetEndParameter(0), newloops);
            //Create a directShape to visualize the solid
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            ds.AppendShape(new Solid[1] { sweepSolid });
            trans.Commit();
            return Result.Succeeded;
        }
    }
}
