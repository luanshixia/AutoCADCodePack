// CustomDictionary.cs
// 
// 提供一种简单的保存数据于DWG中的方式，称为“简单字典”。
// 简单字典巧妙利用AutoCAD的DBDictionary架构实现。
// 简单字典包括全局简单字典和对象简单字典两种形式，均提供人性化的访问方式。

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace AutoCADCommands
{
    /// <summary>
    /// DWG中的全局简单字典，保存在DWG命名对象字典中的"CustomDictionaries"条目中
    /// </summary>
    public class CustomDictionary
    {
        private const string DictionaryRoot = "CustomDictionaries";

        /// <summary>
        /// 获取字典值
        /// </summary>
        /// <param name="dictionary">字典名</param>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public static string GetValue(string dictionary, string key)
        {
            return GetEntry(getDictionaryId(dictionary), key);
        }

        /// <summary>
        /// 设置字典值
        /// </summary>
        /// <param name="dictionary">字典名</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void SetValue(string dictionary, string key, string value)
        {
            SetEntry(getDictionaryId(dictionary), key, value);
        }

        /// <summary>
        /// 获取所有字典名
        /// </summary>
        /// <returns>字典名列表</returns>
        public static IEnumerable<string> GetDictionaryNames()
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                DBDictionary nod = trans.GetObject(HostApplicationServices.WorkingDatabase.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                if (!nod.Contains(DictionaryRoot))
                {
                    yield break;
                }
                else
                {
                    DBDictionary dictRoot = trans.GetObject(nod.GetAt(DictionaryRoot), OpenMode.ForRead) as DBDictionary;
                    foreach (var entry in dictRoot)
                    {
                        yield return entry.Key;
                    }
                }
            }
        }

        internal static IEnumerable<string> GetEntryNames(ObjectId dictId)
        {
            DBDictionary dict = dictId.QOpenForRead<DBDictionary>();
            foreach (var entry in dict)
            {
                yield return entry.Key;
            }
        }

        internal static void SetEntry(ObjectId dictId, string key, string value)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                DBDictionary dict = trans.GetObject(dictId, OpenMode.ForWrite) as DBDictionary;
                if (dict.Contains(key))
                {
                    trans.GetObject(dict.GetAt(key), OpenMode.ForWrite).Erase();
                }
                Xrecord entry = new Xrecord();
                entry.Data = new ResultBuffer(new TypedValue(1000, value));
                ObjectId entryId = dict.SetAt(key, entry);
                trans.AddNewlyCreatedDBObject(entry, true);
                trans.Commit();
            }
        }

        internal static string GetEntry(ObjectId dictId, string key)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                DBDictionary dict = trans.GetObject(dictId, OpenMode.ForRead) as DBDictionary;
                if (dict.Contains(key))
                {
                    ObjectId entryId = dict.GetAt(key);
                    var entry = trans.GetObject(entryId, OpenMode.ForRead) as Xrecord;
                    if (entry != null)
                    {
                        return entry.Data.AsArray().First().Value.ToString();
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 从字典中移除条目
        /// </summary>
        /// <param name="dictName">字典名</param>
        /// <param name="key">键</param>
        public static void RemoveEntry(string dictName, string key) // newly 20111206
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                DBDictionary dict = trans.GetObject(getDictionaryId(dictName), OpenMode.ForWrite) as DBDictionary;
                if (dict.Contains(key))
                {
                    dict.Remove(key);
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// 获取字典中的所有键名
        /// </summary>
        /// <param name="dictionary">字典名</param>
        /// <returns>键名列表</returns>
        public static IEnumerable<string> GetEntryNames(string dictionary)
        {
            return GetEntryNames(getDictionaryId(dictionary));
        }

        private static ObjectId getDictionaryId(string dictionaryName)
        {
            using (DocumentLock doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    DBDictionary nod = trans.GetObject(HostApplicationServices.WorkingDatabase.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
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
                        DBDictionary dictEntry = new DBDictionary();
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
    /// 数据库对象的简单字典，保存在其扩展字典中。
    /// </summary>
    public class CustomObjectDictionary
    {
        /// <summary>
        /// 获取字典值
        /// </summary>
        /// <param name="id">对象ID</param>
        /// <param name="dictionary">字典名</param>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public static string GetValue(ObjectId id, string dictionary, string key)
        {
            return CustomDictionary.GetEntry(getDictionaryId(id, dictionary), key);
        }

        /// <summary>
        /// 设置字典值
        /// </summary>
        /// <param name="id">对象ID</param>
        /// <param name="dictionary">字典名</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void SetValue(ObjectId id, string dictionary, string key, string value)
        {
            CustomDictionary.SetEntry(getDictionaryId(id, dictionary), key, value);
        }

        /// <summary>
        /// 从字典中移除条目
        /// </summary>
        /// <param name="id">对象ID</param>
        /// <param name="dictName">字典名</param>
        /// <param name="key">键</param>
        public static void RemoveEntry(ObjectId id, string dictName, string key) // newly 20111206
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                DBDictionary dict = trans.GetObject(getDictionaryId(id, dictName), OpenMode.ForWrite) as DBDictionary;
                if (dict.Contains(key))
                {
                    dict.Remove(key);
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// 获取所有字典名
        /// </summary>
        /// <param name="id">对象ID</param>
        /// <returns>字典名列表</returns>
        public static IEnumerable<string> GetDictionaryNames(ObjectId id)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                DBObject dbo = trans.GetObject(id, OpenMode.ForRead);
                DBDictionary dictRoot = trans.GetObject(dbo.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                foreach (var entry in dictRoot)
                {
                    yield return entry.Key;
                }
            }
        }

        /// <summary>
        /// 获取字典的所有键名
        /// </summary>
        /// <param name="id">对象ID</param>
        /// <param name="dictionary">字典名</param>
        /// <returns>键名列表</returns>
        public static IEnumerable<string> GetEntryNames(ObjectId id, string dictionary)
        {
            return CustomDictionary.GetEntryNames(getDictionaryId(id, dictionary));
        }

        private static ObjectId getDictionaryId(ObjectId id, string dictionaryName)
        {
            using (DocumentLock doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    DBObject dbo = trans.GetObject(id, OpenMode.ForRead);
                    if (dbo.ExtensionDictionary == ObjectId.Null)
                    {
                        dbo.UpgradeOpen();
                        dbo.CreateExtensionDictionary();
                    }
                    DBDictionary dictRoot = trans.GetObject(dbo.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                    if (!dictRoot.Contains(dictionaryName))
                    {
                        dictRoot.UpgradeOpen();
                        DBDictionary dictEntry = new DBDictionary();
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
