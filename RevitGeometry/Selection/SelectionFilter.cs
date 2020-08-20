using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitGeometry.Selection
{
    public class SelectionFilter : ISelectionFilter
    {
        public SelectionFilter(Func<Element, bool> allowElementCondition = null
            , Func<Reference, XYZ, bool> allowReferenceCondition = null)
        {
            AllowElementCondition = allowElementCondition ?? (_ => true);
            AllowReferenceCondition = allowReferenceCondition ?? ((x1, x2) => true);
        }

        private Func<Element, bool> AllowElementCondition { get; }
        private Func<Reference, XYZ, bool> AllowReferenceCondition { get; }

        public bool AllowElement(Element elem)
        {
            return AllowElementCondition(elem);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return AllowReferenceCondition(reference, position);
        }
    }
}
