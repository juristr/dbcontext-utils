using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbContextUtils.SchemaTranslations;

namespace DbContextUtils.UnitTests.SchemaTranslations
{
    public class SchemaTranslationParserTest
    {
        [TestClass]
        public class TheResolveSchemaTranslationsMethod
        {
            [TestMethod]
            public void ShouldReturnPassedConnectionStringWhenNoTranslationSpecified()
            {
                string connectionString = "User Id=IAM_V4_1_ADM;Password=mypass;Server=dev10g;Home=OraClient11gR2;Persist Security Info=True";

                IEnumerable<SchemaTranslation> schemaTranslations = null;
                string result = SchemaTranslationParser.ResolveSchemaTranslations(connectionString, out schemaTranslations);
                Assert.AreEqual(connectionString, result);
                Assert.IsNull(schemaTranslations);
            }

            [TestMethod]
            public void ShouldReturnCleanedConnectionString()
            {
                string connectionString = "User Id=IAM_V4_1_ADM;Password=mypass;Server=dev10g;Home=OraClient11gR2;Persist Security Info=True;Schema Translations=IAM_V4_1_ADM->IAM_V4_ADM";
                string cleanConnString = connectionString.Replace(";Schema Translations=IAM_V4_1_ADM->IAM_V4_ADM", "");

                IEnumerable<SchemaTranslation> schemaTranslations = null;
                string result = SchemaTranslationParser.ResolveSchemaTranslations(connectionString, out schemaTranslations);
                
                Assert.AreEqual(cleanConnString, result);
            }

            [TestMethod]
            public void ShouldReturnOneSchemaTranslation()
            {
                string connectionString = "User Id=IAM_V4_1_ADM;Password=mypass;Server=dev10g;Home=OraClient11gR2;Persist Security Info=True;Schema Translations=IAM_V4_1_ADM->IAM_V4_ADM";
                string cleanConnString = connectionString.Replace(";Schema Translations=IAM_V4_1_ADM->IAM_V4_ADM", "");

                IEnumerable<SchemaTranslation> schemaTranslations = null;
                string result = SchemaTranslationParser.ResolveSchemaTranslations(connectionString, out schemaTranslations);

                Assert.AreEqual(1, schemaTranslations.Count(), "There should be 1 schema translation");
                Assert.AreEqual("IAM_V4_1_ADM", schemaTranslations.ElementAt(0).OldName, "old name should match");
                Assert.AreEqual("IAM_V4_ADM", schemaTranslations.ElementAt(0).NewName, "new name should match");
            }

            [TestMethod]
            public void ShouldReturnMultipleSchemaTranslations()
            {
                string connectionString = "User Id=IAM_V4_1_ADM;Password=mypass;Server=dev10g;Home=OraClient11gR2;Persist Security Info=True;Schema Translations=IAM_V4_1_ADM->IAM_V4_ADM,IAM_X->IAM_Y";
                string cleanConnString = connectionString.Replace(";Schema Translations=IAM_V4_1_ADM->IAM_V4_ADM,IAM_X->IAM_Y", "");

                IEnumerable<SchemaTranslation> schemaTranslations = null;
                string result = SchemaTranslationParser.ResolveSchemaTranslations(connectionString, out schemaTranslations);

                Assert.AreEqual(2, schemaTranslations.Count(), "There should be 2 schema translation");
            }

        }

        [TestClass]
        public class TheTranslateSchemaMethod
        {
            [TestMethod]
            public void ShouldReturnTranslatedSchemaWhenPassingAValidSchema()
            {
                string connectionString = "User Id=IAM_V4_1_ADM;Password=mypass;Server=dev10g;Home=OraClient11gR2;Persist Security Info=True;Schema Translations=IAM_V4_1_ADM->IAM_V4_ADM";

                IEnumerable<SchemaTranslation> schemaTranslations = null;
                string result = SchemaTranslationParser.TranslateSchema("IAM_V4_1_ADM", connectionString);

                Assert.AreEqual("IAM_V4_ADM", result);
            }

            [TestMethod]
            public void ShouldPassedSchemaNameWhenNoTranslationsSpecified()
            {
                string connectionString = "User Id=IAM_V4_1_ADM;Password=mypass;Server=dev10g;Home=OraClient11gR2;Persist Security Info=True";

                IEnumerable<SchemaTranslation> schemaTranslations = null;
                string result = SchemaTranslationParser.TranslateSchema("IAM_V4_1_ADM", connectionString);

                Assert.AreEqual("IAM_V4_1_ADM", result);
            }
        }
    }
}
