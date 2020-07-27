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
    class ToDirectShapeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var selection = commandData.Application.ActiveUIDocument.Selection;

            var pick = selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
            var elemsGeom = pick.Select(x => doc.GetElement(x).get_Geometry(new Options()));

            var tr = new Transaction(doc, "test");
            tr.Start();
            foreach (var e in elemsGeom)
            {
                var ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Walls));
                ds.SetShape(e.ToArray());
                selection.SetElementIds(new[] { ds.Id });
            }
            tr.Commit();
            return Result.Succeeded;
        }
    }
}
