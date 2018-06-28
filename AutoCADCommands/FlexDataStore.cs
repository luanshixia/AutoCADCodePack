using Autodesk.AutoCAD.DatabaseServices;
using System.Linq;

namespace Dreambuild.AutoCAD
{
    /// <summary>
    /// Flexible data store. FDS is our v3 data store mechanism. Old ways FXD (v1) and CD (v2) should be deprecated.
    /// </summary>
    public class FlexDataStore
    {
        private ObjectId DictionaryId { get; }

        internal FlexDataStore(ObjectId dictionaryId)
        {
            this.DictionaryId = dictionaryId;
        }

        /// <summary>
        /// Gets a value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public string GetValue(string key)
        {
            var dictionary = this.DictionaryId.QOpenForRead<DBDictionary>();
            if (dictionary.Contains(key))
            {
                var record = dictionary.GetAt(key).QOpenForRead<Xrecord>();
                return record.Data.AsArray().First().Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Sets a value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The flex data store.</returns>
        public FlexDataStore SetValue(string key, string value)
        {
            using (var trans = this.DictionaryId.Database.TransactionManager.StartTransaction())
            {
                var dictionary = trans.GetObject(this.DictionaryId, OpenMode.ForWrite) as DBDictionary;
                if (dictionary.Contains(key))
                {
                    trans.GetObject(dictionary.GetAt(key), OpenMode.ForWrite).Erase();
                }

                var record = new Xrecord
                {
                    Data = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataAsciiString, value))
                };

                dictionary.SetAt(key, record);
                trans.AddNewlyCreatedDBObject(record, true);
                trans.Commit();
            }

            return this;
        }
    }

    /// <summary>
    /// The flex data store extensions.
    /// </summary>
    public static class FlexDataStoreExtensions
    {
        internal const string DwgGlobalStoreName = "FlexDataStore";

        /// <summary>
        /// Gets the DWG global flex data store.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The flex data store.</returns>
        public static FlexDataStore FlexDataStore(this Database db)
        {
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var namedObjectsDict = trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                if (!namedObjectsDict.Contains(FlexDataStoreExtensions.DwgGlobalStoreName))
                {
                    namedObjectsDict.UpgradeOpen();
                    var dwgGlobalStore = new DBDictionary();
                    var storeId = namedObjectsDict.SetAt(FlexDataStoreExtensions.DwgGlobalStoreName, dwgGlobalStore);
                    trans.AddNewlyCreatedDBObject(dwgGlobalStore, true);
                    trans.Commit();
                    return new FlexDataStore(storeId);
                }

                trans.Abort();
                return new FlexDataStore(namedObjectsDict.GetAt(FlexDataStoreExtensions.DwgGlobalStoreName));
            }
        }

        /// <summary>
        /// Gets an object's flex data store.
        /// </summary>
        /// <param name="id">The object ID.</param>
        /// <returns>The flex data store.</returns>
        public static FlexDataStore FlexDataStore(this ObjectId id)
        {
            using (var trans = id.Database.TransactionManager.StartTransaction())
            {
                var dbo = trans.GetObject(id, OpenMode.ForRead);
                if (dbo.ExtensionDictionary == ObjectId.Null)
                {
                    dbo.UpgradeOpen();
                    dbo.CreateExtensionDictionary();
                    trans.Commit();
                    return new FlexDataStore(dbo.ExtensionDictionary);
                }

                trans.Abort();
                return new FlexDataStore(dbo.ExtensionDictionary);
            }
        }
    }
}
