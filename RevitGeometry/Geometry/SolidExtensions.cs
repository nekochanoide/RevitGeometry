using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using RevitGeometry.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitGeometry.Geometry
{
    public enum SolidIntersection
    {
        /// <summary>
        /// Undefined or Revit Exception has occurred.
        /// </summary>
        Invalid,

        Intersecting,
        Touching,
        NonIntersecting
    }

    public static class SolidExtensions
    {
        public static Solid CreateCylindricalSolidFromLine(this Line line, double radius = 0.1)
        {
            line = line.Fix();
            Plane plane = Plane.CreateByNormalAndOrigin(line.Direction, line.Origin);
            CurveLoop profile = CurveLoop.Create(new[] {
                Arc.Create(plane, radius, 0, Math.PI),
                Arc.Create(plane, radius, Math.PI, 2 * Math.PI)
            });
            return GeometryCreationUtilities.CreateExtrusionGeometry(new[] { profile }, line.Direction, line.Length);
        }

        public static Solid CreateCylindricalSolidFromCurve(this Curve curve, double radius = 0.1)
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

        public static IEnumerable<Solid> GetAllSolids(this GeometryElement geometry)
        {
            foreach (GeometryObject gObject in geometry)
            {
                if (gObject is Solid solid)
                    yield return solid;
                GeometryElement geometryElement = default;
                if (gObject is GeometryInstance geometryInstance)
                    geometryElement = geometryInstance.GetInstanceGeometry();
                if (gObject is GeometryElement element)
                    geometryElement = element;
                if (geometryElement != default)
                    foreach (var deepSolid in GetAllSolids(geometryElement))
                        yield return deepSolid;
            }
        }

        public static SolidIntersection HasIntersection(this Solid solid0, Solid solid1, out Solid result)
        {
            result = null;
            try
            {
                result = BooleanOperationsUtils.ExecuteBooleanOperation(solid0, solid1, BooleanOperationsType.Intersect);
                if (Math.Abs(result.Volume) > 0.000001)
                {
                    // Intersecting.
                    return SolidIntersection.Intersecting;
                }
                result = BooleanOperationsUtils.ExecuteBooleanOperation
                    (solid0, solid1, BooleanOperationsType.Union);
                double dArea = Math.Abs(solid0.SurfaceArea + solid1.SurfaceArea - result.SurfaceArea);
                if (dArea < 0.00001 && solid0.Edges.Size + solid1.Edges.Size == result.Edges.Size)
                {
                    // Neither intersecting, nor touching.
                    return SolidIntersection.NonIntersecting;
                }
                else
                {
                    // Touching.
                    return SolidIntersection.Touching;
                }
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                return SolidIntersection.Invalid;
            }
        }

        public static Solid SolidBoundingBox(this BoundingBoxXYZ bbox)
        {
            // corners in BBox coords
            var (minX, minY, minZ) = bbox.Min;
            var (maxX, maxY, maxZ) = bbox.Max;
            Xyz pt0 = (minX, minY, minZ);
            Xyz pt1 = (maxX, minY, minZ);
            Xyz pt2 = (maxX, maxY, minZ);
            Xyz pt3 = (minX, maxY, minZ);
            //edges in BBox coords
            Line edge0 = Line.CreateBound(pt0, pt1);
            Line edge1 = Line.CreateBound(pt1, pt2);
            Line edge2 = Line.CreateBound(pt2, pt3);
            Line edge3 = Line.CreateBound(pt3, pt0);
            //create loop, still in BBox coords
            double height = maxZ - minZ;
            CurveLoop baseLoop = CurveLoop.Create(new[] { edge0, edge1, edge2, edge3 });
            Solid preTransformBox = GeometryCreationUtilities.CreateExtrusionGeometry(new[] { baseLoop }, XYZ.BasisZ, height);
            Solid transformBox = SolidUtils.CreateTransformed(preTransformBox, bbox.Transform);

            return transformBox;
        }

        public static Solid CreateSolidByLoopExtrusion(this Face face, double distance = 0.1)
        {
            CurveLoop loop = face.GetEdgesAsCurveLoops().First();
            return CreateSolidByLoopExtrusion(face.ComputeNormal(UV.Zero), loop, distance);
        }

        public static Solid CreateSolidByLoopExtrusion(XYZ normal, CurveLoop loop, double distance = 0.1)
        {
            return GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { loop }, normal, distance);
        }

        public static bool TryCutSolid(Solid beingCutted, Solid cuts)
        {
            try
            {
                BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid
                    (beingCutted, cuts, BooleanOperationsType.Difference);
                return true;
            }
            catch (InternalException)
            {
                return false;
            }
        }

        public static IEnumerable<Face> GetFaces(this Solid solid) => solid.Faces.Cast<Face>();

        public static IEnumerable<Face> GetFaces(this Solid solid, Func<Face, bool> predicate)
        {
            return solid.GetFaces().Where(x => predicate.Invoke(x));
        }
    }
}