using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.Xml.XPath;
using System.Data.Mapping;
using System.Configuration;
using DbContextUtils.SchemaTranslations;

namespace DbContextUtils.ConnectionAdapter
{

    public class DbContextConnectionAdapter
    {
        private static XNamespace StoreNamespace = "http://schemas.microsoft.com/ado/2009/02/edm/ssdl";
        private static IDictionary<string, MetadataWorkspace> metadataMapping = new Dictionary<string, MetadataWorkspace>();
        private static readonly object lockObj = new object();
        private Assembly resourceAssembly;

        public DbContextConnectionAdapter(Assembly resourceAssembly)
        {
            this.resourceAssembly = resourceAssembly;
        }

        public DbConnection AdaptConnection(string connectionString)
        {
            var connectionData = new EntityConnectionStringBuilder(connectionString);

            if (!string.IsNullOrEmpty(connectionData.Name))
                connectionData = new EntityConnectionStringBuilder(ConfigurationManager.ConnectionStrings[connectionData.Name].ConnectionString);

            IEnumerable<SchemaTranslation> schemaTranslations = null;
            connectionData.ProviderConnectionString = SchemaTranslationParser.ResolveSchemaTranslations(connectionData.ProviderConnectionString, out schemaTranslations);

            var connection = DbProviderFactories.GetFactory(connectionData.Provider).CreateConnection();
            connection.ConnectionString = connectionData.ProviderConnectionString;

            EntityConnection resultConn = null;
            //it happened to throw exceptions when inserting in the cache dictionary; hence the lock here
            lock (lockObj)
            {
                resultConn = new EntityConnection(AdaptWorkspace(connectionData, schemaTranslations), connection);
            }

            return resultConn;
        }

        private string ResolveSchemaTranslations(string p, out IEnumerable<SchemaTranslation> schemaTranslations)
        {
            schemaTranslations = null;
            return p;
        }

        private MetadataWorkspace AdaptWorkspace(EntityConnectionStringBuilder connectionData, IEnumerable<SchemaTranslation> schemaTranslations)
        {
            MetadataWorkspace workspace;

            // If our metadata dictionary already contains the workspace, use it; otherwise walk
            if (!metadataMapping.TryGetValue(connectionData.Metadata, out workspace))
            {

                // Load our workspace (utilizing the resourceAssembly, when appropriate)
                var metaDataSource = new MetadataArtifacts(connectionData.Metadata, this.resourceAssembly);

                // Create our workspace after splitting into conceptual (CSDL), storage (SSDL), and mapping (MSL) subtrees
                //workspace = CreateWorkspace(
                //    metaDataSource.ConceptualXml.CreateNavigator().ReadSubtree(),
                //    AdaptStorageMetadata(metaDataSource.StorageXml.CreateNavigator().ReadSubtree()),
                //    AdaptMappingMetadata(metaDataSource.MappingXml.CreateNavigator().ReadSubtree()));

                //I'm only adapting the storage metadata here
                workspace = CreateWorkspace(
                    metaDataSource.ConceptualXml.CreateNavigator().ReadSubtree(),
                    AdaptStorageMetadata(metaDataSource.StorageXml.CreateNavigator().ReadSubtree(), schemaTranslations),
                    metaDataSource.MappingXml.CreateNavigator().ReadSubtree());

                // Cache our walked workspace for future performance gains
                metadataMapping[connectionData.Metadata] = workspace;
            }

            return workspace;
        }

        private XmlReader AdaptStorageMetadata(XmlReader storageReader, IEnumerable<SchemaTranslation> schemaTranslations)
        {
            var xml = XElement.Load(storageReader);

            if (schemaTranslations != null && schemaTranslations.Count() > 0)
            {
                var schemaTransl = schemaTranslations.ToList<SchemaTranslation>();
                // Walk the SSDL EntitySets and adapt
                foreach (var storeEntitySet in xml.Descendants(StoreNamespace + "EntitySet"))
                {
                    var currentSchema = storeEntitySet.Attribute("Schema").Value;
                    var translation = schemaTransl.FirstOrDefault(x => x.OldName == currentSchema);
                    if (translation != null)
                    {
                        storeEntitySet.Attribute("Schema").Value = translation.NewName;
                    }

                    //ModelAdapter.AdaptStoreEntitySet(storeEntitySet);
                }
            }


            // Walk the associative endpoints and adapt 
            //foreach (var associationEnd in xml.Descendants(StoreNamespace + "AssociationSet").Descendants(StoreNamespace + "End"))
            //    ModelAdapter.AdaptStoreAssociationEnd(associationEnd);

            return xml.CreateReader();
        }

        private static MetadataWorkspace CreateWorkspace(XmlReader conceptualReader, XmlReader storageReader, XmlReader mappingReader)
        {
            var workspace = new MetadataWorkspace();

            // Convert our XML data into workspace collections (the enumerable XmlReaders will be singletons)
            var conceptualCollection = new EdmItemCollection(conceptualReader.ToEnumerable());
            var storageCollection = new StoreItemCollection(storageReader.ToEnumerable());
            var mappingCollection = new StorageMappingItemCollection(conceptualCollection, storageCollection,
                mappingReader.ToEnumerable());

            // Register our collections in the workspace
            workspace.RegisterItemCollection(conceptualCollection);
            workspace.RegisterItemCollection(storageCollection);
            workspace.RegisterItemCollection(mappingCollection);

            return workspace;
        }
    }
}
