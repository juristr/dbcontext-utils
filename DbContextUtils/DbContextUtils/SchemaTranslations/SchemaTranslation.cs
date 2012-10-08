using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbContextUtils.SchemaTranslations
{

    internal class SchemaTranslation
    {
        public string OldName { get; private set; }
        public string NewName { get; private set; }

        public SchemaTranslation(string oldSchema, string newSchema)
        {
            this.OldName = oldSchema;
            this.NewName = newSchema;
        }
    }
}
