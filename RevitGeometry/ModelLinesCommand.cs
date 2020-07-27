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
    class ModelLinesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            IList<Reference> selection = commandData.Application.ActiveUIDocument.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
            var doc = commandData.Application.ActiveUIDocument.Document;
            var modelLines = selection.Select(x => ((ModelLine)doc.GetElement(x)));
            //modelLines.Select(x => doc.)
            var tr = new Transaction(doc, "create solids");
            tr.Start();
            return Result.Cancelled;
        }
    }
}
