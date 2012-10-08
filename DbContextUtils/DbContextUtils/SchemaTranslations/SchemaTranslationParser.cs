using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DbContextUtils.SchemaTranslations
{
    public static class SchemaTranslationParser
    {
        private static string TRANSLATION_REGEX_PATTERN = @";Schema Translations=((,?)([\w]*->[\w]*))*";
        private static string TRANSLATION_SCHEMA = @"[\w]*->[\w]*";
        private static string[] SPLIT_CHAR_SEQUENCE = new string[] { "->" };

        /// <summary>
        /// Returns the translated schema if translations are present
        /// </summary>
        /// <param name="schemaName">the schema name to be translated</param>
        /// <param name="connectionString">the connection string containing the potential translations</param>
        /// <returns>the translated schema name if any are present, the passed schema name otherwise</returns>
        public static string TranslateSchema(string schemaName, string connectionString)
        {
            IEnumerable<SchemaTranslation> schemaTranslations;
            ResolveSchemaTranslations(connectionString, out schemaTranslations);
            if (schemaTranslations == null)
                return schemaName;


            var result = schemaTranslations.FirstOrDefault(x => x.OldName == schemaName);
            if (result == null)
            {
                return schemaName;
            }
            else
            {
                return result.NewName;
            }
        }

        /// <summary>
        /// Returns a set of schema translations
        /// </summary>
        /// <param name="connectionString">the connection string to search for schema translations</param>
        /// <param name="schemaTranslations">the set of found and parsed translations</param>
        /// <returns>the cleaned connection string</returns>
        internal static string ResolveSchemaTranslations(string connectionString, out IEnumerable<SchemaTranslation> schemaTranslations)
        {
            var regex = new Regex(TRANSLATION_REGEX_PATTERN);

            Match match = regex.Match(connectionString);
            if (match.Success)
            {
                connectionString = connectionString.Replace(match.Value, "");

                schemaTranslations = ExtractSchemaTranslations(match.Value);
            }
            else
            {
                schemaTranslations = null;
            }

            return connectionString;
        }


        private static IEnumerable<SchemaTranslation> ExtractSchemaTranslations(string schemaTranslations)
        {
            var singleTranslationRegex = new Regex(TRANSLATION_SCHEMA);
            var matches = singleTranslationRegex.Matches(schemaTranslations);
            if (matches.Count > 0)
            {
                for (var i = 0; i < matches.Count; i++)
                {
                    var split = matches[i].Value.Split(SPLIT_CHAR_SEQUENCE, StringSplitOptions.None);
                    yield return new SchemaTranslation(split[0], split[1]);
                }
            }
        }

    }
}
