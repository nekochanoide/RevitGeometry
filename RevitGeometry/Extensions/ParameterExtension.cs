using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitGeometry.Extensions
{
    public static class ParameterExtension
    {
        public static void SetDouble(this Parameter parameter, double value)
        {
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    parameter.Set(value);
                    break;
                case StorageType.String:
                    parameter.Set(value.ToString());
                    break;
                case StorageType.Integer:
                    parameter.Set((int)value);
                    break;
            }
        }
    }
}