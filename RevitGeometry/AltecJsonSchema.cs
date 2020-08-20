using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RevitGeometry
{
    public class AltecJsonSchema
    {
        private const string VendorId = "Altec.Systems.3.002";
        private Guid _schemaGuid = new Guid("FDE91B8C-D078-4297-997C-FB5575409337");
        private readonly Schema _schema;
        public AltecJsonSchema()
        {
            _schema = CheckOrInit();
        }

        public bool SetJson(Element element, string json)
        {
            Entity entity = element.GetEntity(_schema);
            if (!entity.IsValid())
            {
                entity = new Entity(_schema);
            }
            entity.Set("json", json);
            return true;
        }

        public string GetJson(Element element)
        {
            return element.GetEntity(_schema).Get<string>("json");
        }

        public Schema CheckOrInit()
        {
            var schema = Schema.Lookup(_schemaGuid);
            if (schema is null)
            {
                schema = Initialize();
            }
            return schema;
        }

        private Schema Initialize()
        {
            var e = new SchemaBuilder(_schemaGuid);
            e.SetReadAccessLevel(AccessLevel.Vendor);
            e.SetWriteAccessLevel(AccessLevel.Vendor);
            e.SetVendorId(VendorId);
            e.SetDocumentation("");
            e.AddSimpleField("json", typeof(string));
            return e.Finish();
        }
    }

    public static class AltecJsonSchemaExtensions
    {
        public static string GetJson(this AltecJsonSchema schema, Element element)
        {
            return schema.GetJson(element);
        }

        public static void SetJson(this AltecJsonSchema schema, Element element, string json)
        {
            schema.SetJson(element, json);
        }
    }
}
