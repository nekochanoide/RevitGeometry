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
    class ChangeColorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var selection = commandData.Application.ActiveUIDocument.Selection;
            var select = selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Face);
            var faces = select.Select(x => doc.GetElement(x).GetGeometryObjectFromReference(x) as Face);
            var tr = new Transaction(doc, "test");
            tr.Start();
            for (int i = 0; i < select.Count; i++)
            {
                doc.Paint(select[i].ElementId, faces.ElementAt(i), new ElementId(443044));
            }
            tr.Commit();
            return Result.Succeeded;
        }
    }
}
