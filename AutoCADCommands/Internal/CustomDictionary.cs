using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace Dreambuild.AutoCAD.Internal
{
    /// <summary>
    /// DWG global flexible data storage.
    /// </summary>
    /// <remarks>
    /// A simple way to store data in DWG. The global data entries are saved in "CustomDictionaries" in the named object dictionary.
    /// </remarks>
    public static class CustomDictionary
    {
        private const string DictionaryRoot = "CustomDictionaries";

        /// <summary>
        /// Gets value.
        /// </summary>
        /// <param name="dictionary">The dictionary name.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public static string GetValue(string dictionary, string key)
        {
            return CustomDictionary.GetEntry(CustomDictionary.GetDictionaryId(dictionary), key);
        }

        /// <summary>
        /// Sets value.
        /// </summary>
        /// <param name="dictionary">The dictionary name.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetValue(string dictionary, string key, string value)
        {
            CustomDictionary.SetEntry(CustomDictionary.GetDictionaryId(dictionary), key, value);
        }

        /// <summary>
        /// Gets all dictionary names.
        /// </summary>
        /// <returns>The names.</returns>
        public static IEnumerable<string> GetDictionaryNames()
        {
            using (var trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var nod = trans.GetObject(HostApplicationServices.WorkingDatabase.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                if (!nod.Contains(CustomDictionary.DictionaryRoot))
                {
                    yield break;
                }
                else
                {
                    var dictRoot = trans.GetObject(nod.GetAt(CustomDictionary.DictionaryRoot), OpenMode.ForRead) as DBDictionary;
                    foreach (var entry in dictRoot)
                    {
                        yield return entry.Key;
                    }
                }
            }
        }

        internal static IEnumerable<string> GetEntryNames(ObjectId dictId)
        {
            var dict = dictId.QOpenForRead<DBDictionary>();
            foreach (var entry in dict)
            {
                yield return entry.Key;
            }
        }

        internal static void SetEntry(ObjectId dictId, string key, string value)
        {
            using (var trans = dictId.Database.TransactionManager.StartTransaction())
            {
                var dict = trans.GetObject(dictId, OpenMode.ForWrite) as DBDictionary;
                if (dict.Contains(key))
                {
                    trans.GetObject(dict.GetAt(key), OpenMode.ForWrite).Erase();
                }
                var entry = new Xrecord
                {
                    Data = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataAsciiString, value))
                };
                var entryId = dict.SetAt(key, entry);
                trans.AddNewlyCreatedDBObject(entry, true);
                trans.Commit();
            }
        }

        internal static string GetEntry(ObjectId dictId, string key)
        {
            using (var trans = dictId.Database.TransactionManager.StartTransaction())
            {
                var dict = trans.GetObject(dictId, OpenMode.ForRead) as DBDictionary;
                if (dict.Contains(key))
                {
                    var entryId = dict.GetAt(key);
                    var entry = trans.GetObject(entryId, OpenMode.ForRead) as Xrecord;
                    if (entry != null)
                    {
                        return entry.Data.AsArray().First().Value.ToString();
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Removes entry from dictionary.
        /// </summary>
        /// <param name="dictName">The dictionary name.</param>
        /// <param name="key">The key.</param>
        public static void RemoveEntry(string dictName, string key) // newly 20111206
        {
            using (var trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var dict = trans.GetObject(GetDictionaryId(dictName), OpenMode.ForWrite) as DBDictionary;
                if (dict.Contains(key))
                {
                    dict.Remove(key);
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// Gets all keys from a dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary name.</param>
        /// <returns>The keys.</returns>
        public static IEnumerable<string> GetEntryNames(string dictionary)
        {
            return CustomDictionary.GetEntryNames(CustomDictionary.GetDictionaryId(dictionary));
        }

        private static ObjectId GetDictionaryId(string dictionaryName)
        {
            using (var doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (var trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    var nod = trans.GetObject(HostApplicationServices.WorkingDatabase.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                    DBDictionary dictRoot;
                    if (!nod.Contains(DictionaryRoot))
                    {
                        dictRoot = new DBDictionary();
                        nod.UpgradeOpen();
                        nod.SetAt(DictionaryRoot, dictRoot);
                        trans.AddNewlyCreatedDBObject(dictRoot, true);
                    }
                    else
                    {
                        dictRoot = trans.GetObject(nod.GetAt(DictionaryRoot), OpenMode.ForWrite) as DBDictionary;
                    }
                    if (!dictRoot.Contains(dictionaryName))
                    {
                        var dictEntry = new DBDictionary();
                        dictRoot.SetAt(dictionaryName, dictEntry);
                        trans.AddNewlyCreatedDBObject(dictEntry, true);
                    }
                    trans.Commit();
                    return dictRoot.GetAt(dictionaryName);
                }
            }
        }
    }

    /// <summary>
    /// DB object flexible data storage.
    /// </summary>
    /// <remarks>
    /// A simple way to attach data to DB objects. The DB object entries are stored in the object's XData.
    /// </remarks>
    public static class CustomObjectDictionary
    {
        /// <summary>
        /// Gets value.
        /// </summary>
        /// <param name="id">The object ID.</param>
        /// <param name="dictionary">The dictionary name.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public static string GetValue(ObjectId id, string dictionary, string key)
        {
            return CustomDictionary.GetEntry(CustomObjectDictionary.GetDictionaryId(id, dictionary), key);
        }

        /// <summary>
        /// Sets value.
        /// </summary>
        /// <param name="id">The object ID.</param>
        /// <param name="dictionary">The dictionary name.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetValue(ObjectId id, string dictionary, string key, string value)
        {
            CustomDictionary.SetEntry(CustomObjectDictionary.GetDictionaryId(id, dictionary), key, value);
        }

        /// <summary>
        /// Removes entry from dictionary.
        /// </summary>
        /// <param name="id">The object ID.</param>
        /// <param name="dictName">The dictionary name.</param>
        /// <param name="key">The key.</param>
        public static void RemoveEntry(ObjectId id, string dictName, string key) // newly 20111206
        {
            using (var trans = id.Database.TransactionManager.StartTransaction())
            {
                var dict = trans.GetObject(GetDictionaryId(id, dictName), OpenMode.ForWrite) as DBDictionary;
                if (dict.Contains(key))
                {
                    dict.Remove(key);
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// Gets all dictionary names.
        /// </summary>
        /// <param name="id">The object ID.</param>
        /// <returns>The names.</returns>
        public static IEnumerable<string> GetDictionaryNames(ObjectId id)
        {
            using (var trans = id.Database.TransactionManager.StartTransaction())
            {
                var dbo = trans.GetObject(id, OpenMode.ForRead);
                var dictRoot = trans.GetObject(dbo.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                foreach (var entry in dictRoot)
                {
                    yield return entry.Key;
                }
            }
        }

        /// <summary>
        /// Gets all keys from a dictionary.
        /// </summary>
        /// <param name="id">The object ID.</param>
        /// <param name="dictionary">The dictionary name.</param>
        /// <returns>The keys.</returns>
        public static IEnumerable<string> GetEntryNames(ObjectId id, string dictionary)
        {
            return CustomDictionary.GetEntryNames(CustomObjectDictionary.GetDictionaryId(id, dictionary));
        }

        private static ObjectId GetDictionaryId(ObjectId id, string dictionaryName)
        {
            using (var doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (var trans = id.Database.TransactionManager.StartTransaction())
                {
                    var dbo = trans.GetObject(id, OpenMode.ForRead);
                    if (dbo.ExtensionDictionary == ObjectId.Null)
                    {
                        dbo.UpgradeOpen();
                        dbo.CreateExtensionDictionary();
                    }
                    var dictRoot = trans.GetObject(dbo.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                    if (!dictRoot.Contains(dictionaryName))
                    {
                        dictRoot.UpgradeOpen();
                        var dictEntry = new DBDictionary();
                        dictRoot.SetAt(dictionaryName, dictEntry);
                        trans.AddNewlyCreatedDBObject(dictEntry, true);
                    }
                    trans.Commit();
                    return dictRoot.GetAt(dictionaryName);
                }
            }
        }
    }
}
