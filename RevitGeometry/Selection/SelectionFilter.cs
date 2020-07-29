using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitGeometry.Selection
{
    class SelectionFilter : ISelectionFilter
    {
        public SelectionFilter(Func<Element, bool> allowElementCondition
            , Func<Reference, XYZ, bool> allowReferenceCondition)
        {
            AllowElementCondition = allowElementCondition;
            AllowReferenceCondition = allowReferenceCondition;
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
