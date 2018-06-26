using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCADCommands
{
    /// <summary>
    /// Database operation helpers.
    /// </summary>
    public static class DbHelper
    {
        #region symbol tables & dictionaries

        /// <summary>
        /// Gets all records of a symbol table.
        /// </summary>
        /// <param name="symbolTableId">The symbol table ID.</param>
        /// <returns>The record IDs.</returns>
        public static ObjectId[] GetSymbolTableRecords(ObjectId symbolTableId)
        {
            using (var trans = symbolTableId.Database.TransactionManager.StartTransaction())
            {
                var table = (SymbolTable)trans.GetObject(symbolTableId, OpenMode.ForRead);
                return table.Cast<ObjectId>().ToArray();
            }
        }

        /// <summary>
        /// Gets all record names of a symbol table.
        /// </summary>
        /// <param name="symbolTableId">The symbol table ID.</param>
        /// <returns>The record names.</returns>
        public static string[] GetSymbolTableRecordNames(ObjectId symbolTableId)
        {
            return DbHelper
                .GetSymbolTableRecords(symbolTableId)
                .QOpenForRead<SymbolTableRecord>()
                .Select(record => record.Name)
                .ToArray();
        }

        /// <summary>
        /// Gets a symbol table record by name.
        /// </summary>
        /// <param name="symbolTableId">The symbol table ID.</param>
        /// <param name="name">The record name.</param>
        /// <param name="defaultValue">The default value if not found.</param>
        /// <param name="create">The factory method if not found.</param>
        /// <returns>The record ID.</returns>
        public static ObjectId GetSymbolTableRecord(ObjectId symbolTableId, string name, ObjectId? defaultValue = null, Func<SymbolTableRecord> create = null)
        {
            using (var trans = symbolTableId.Database.TransactionManager.StartTransaction())
            {
                var table = (SymbolTable)trans.GetObject(symbolTableId, OpenMode.ForRead);
                if (table.Has(name))
                {
                    return table[name];
                }

                if (create != null)
                {
                    var record = create();
                    table.UpgradeOpen();
                    var result = table.Add(record);
                    trans.AddNewlyCreatedDBObject(record, true);
                    trans.Commit();
                    return result;
                }
            }

            return defaultValue.Value;
        }

        /// <summary>
        /// Gets layer ID by name. Creates new if not found.
        /// </summary>
        /// <param name="layerName">The layer name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The layer ID.</returns>
        public static ObjectId GetLayerId(string layerName, Database db = null)
        {
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: (db ?? HostApplicationServices.WorkingDatabase).LayerTableId,
                name: layerName,
                create: () => new LayerTableRecord { Name = layerName });
        }

        /// <summary>
        /// Gets all layer IDs.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The layer IDs.</returns>
        public static ObjectId[] GetAllLayerIds(Database db = null)
        {
            return DbHelper.GetSymbolTableRecords((db ?? HostApplicationServices.WorkingDatabase).LayerTableId);
        }

        /// <summary>
        /// Gets all layer names.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The layer names.</returns>
        public static string[] GetAllLayerNames(Database db = null)
        {
            return DbHelper.GetSymbolTableRecordNames((db ?? HostApplicationServices.WorkingDatabase).LayerTableId);
        }

        /// <summary>
        /// Ensures a layer is visible.
        /// </summary>
        /// <param name="layerName">The layer name.</param>
        public static void EnsureLayerOn(string layerName)
        {
            var id = DbHelper.GetLayerId(layerName);
            id.QOpenForWrite<LayerTableRecord>(layer =>
            {
                layer.IsFrozen = false;
                layer.IsHidden = false;
                layer.IsOff = false;
            });
        }

        /// <summary>
        /// Gets block table record ID by block name.
        /// </summary>
        /// <param name="blockName">The block name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The block table ID.</returns>
        public static ObjectId GetBlockId(string blockName, Database db = null)
        {
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: (db ?? HostApplicationServices.WorkingDatabase).BlockTableId,
                name: blockName,
                defaultValue: ObjectId.Null);
        }

        /// <summary>
        /// Gets all block table record IDs.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The object ID array.</returns>
        public static ObjectId[] GetAllBlockIds(Database db = null)
        {
            return DbHelper.GetSymbolTableRecords((db ?? HostApplicationServices.WorkingDatabase).BlockTableId);
        }

        /// <summary>
        /// Gets all block names.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The block name array.</returns>
        public static string[] GetAllBlockNames(Database db = null)
        {
            return DbHelper.GetSymbolTableRecordNames((db ?? HostApplicationServices.WorkingDatabase).BlockTableId);
        }

        /// <summary>
        /// Gets linetype ID by name. Returns the continuous linetype as default if not found.
        /// </summary>
        /// <param name="linetypeName">The linetype name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The linetype ID.</returns>
        public static ObjectId GetLinetypeId(string linetypeName, Database db = null)
        {
            db = db ?? HostApplicationServices.WorkingDatabase;
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: db.LinetypeTableId,
                name: linetypeName,
                defaultValue: db.ContinuousLinetype);
        }

        /// <summary>
        /// Gets text style ID by name. Returns the current TEXTSTYLE as default if not found.
        /// </summary>
        /// <param name="textStyleName">The text style name.</param>
        /// <param name="createIfNotFound">Whether to create new if not found.</param>
        /// <param name="db">The database.</param>
        /// <returns>The text style ID.</returns>
        public static ObjectId GetTextStyleId(string textStyleName, bool createIfNotFound = false, Database db = null)
        {
            db = db ?? HostApplicationServices.WorkingDatabase;
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: db.TextStyleTableId,
                name: textStyleName,
                create: () => new TextStyleTableRecord { Name = textStyleName },
                defaultValue: db.Textstyle);
        }

        /// <summary>
        /// Gets dimension style ID by name. Returns the current DIMSTYLE as default if not found.
        /// </summary>
        /// <param name="dimStyleName">The dimension style name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The dimension style ID.</returns>
        public static ObjectId GetDimstyleId(string dimStyleName, Database db = null)
        {
            db = db ?? HostApplicationServices.WorkingDatabase;
            return DbHelper.GetSymbolTableRecord(
                symbolTableId: db.DimStyleTableId,
                name: dimStyleName,
                defaultValue: db.Dimstyle);
        }

        /// <summary>
        /// Gets a dictionary object.
        /// </summary>
        /// <param name="dictionaryId">The dictionary ID.</param>
        /// <param name="name">The entry name.</param>
        /// <param name="defaultValue">The default value if not found.</param>
        /// <param name="create">The factory method if not found.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId GetDictionaryObject(ObjectId dictionaryId, string name, ObjectId? defaultValue = null, Func<DBObject> create = null)
        {
            using (var trans = dictionaryId.Database.TransactionManager.StartTransaction())
            {
                var dictionary = (DBDictionary)trans.GetObject(dictionaryId, OpenMode.ForRead);
                if (dictionary.Contains(name))
                {
                    return dictionary.GetAt(name);
                }

                if (create != null)
                {
                    var dictObject = create();
                    dictionary.UpgradeOpen();
                    var result = dictionary.SetAt(name, dictObject);
                    trans.AddNewlyCreatedDBObject(dictObject, true);
                    trans.Commit();
                    return result;
                }
            }

            return defaultValue.Value;
        }

        /// <summary>
        /// Gets group ID by name.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="db">The database.</param>
        /// <returns>The group ID.</returns>
        public static ObjectId GetGroupId(string groupName, Database db = null)
        {
            return DbHelper.GetDictionaryObject(
                dictionaryId: (db ?? HostApplicationServices.WorkingDatabase).GroupDictionaryId,
                name: groupName,
                defaultValue: ObjectId.Null);
        }

        /// <summary>
        /// Gets group ID by entity ID.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <returns>The group ID.</returns>
        public static ObjectId GetGroupId(ObjectId entityId)
        {
            var groupDict = entityId.Database.GroupDictionaryId.QOpenForRead<DBDictionary>();
            var entity = entityId.QOpenForRead<Entity>();
            try
            {
                return groupDict
                    .Cast<DBDictionaryEntry>()
                    .First(entry => entry.Value.QOpenForRead<Group>().Has(entity))
                    .Value;
            }
            catch
            {
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// Gets all entity IDs in a group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>The entity IDs.</returns>
        public static IEnumerable<ObjectId> GetEntityIdsInGroup(ObjectId groupId)
        {
            var group = groupId.QOpenForRead<Group>();
            if (group != null)
            {
                return group.GetAllEntityIds();
            }

            return Array.Empty<ObjectId>();
        }

        #endregion

        #region xdata

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static string GetFirstXData(this DBObject dbo, string appName)
        {
            var xdataForApp = dbo.GetXDataForApplication(appName);
            if (xdataForApp == null)
            {
                return string.Empty;
            }
            foreach (var value in xdataForApp.AsArray())
            {
                if (value.TypeCode != (int)DxfCode.ExtendedDataRegAppName)
                {
                    return value.Value.ToString();
                }
            }
            return string.Empty;
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static object GetFirstXDataT(this DBObject dbo, string appName)
        {
            var xdataForApp = dbo.GetXDataForApplication(appName);
            if (xdataForApp == null)
            {
                return null;
            }
            foreach (var value in xdataForApp.AsArray())
            {
                if (value.TypeCode != (int)DxfCode.ExtendedDataRegAppName)
                {
                    return value.Value;
                }
            }
            return null;
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void SetFirstXData(this DBObject dbo, string appName, string value)
        {
            dbo.XData = new ResultBuffer(new[]
            {
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, appName),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, value)
            });
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void SetFirstXDataT(this DBObject dbo, string appName, object value) // newly 20111207
        {
            var typeCode = value is Int16
                ? (int)DxfCode.ExtendedDataInteger16
                : value is Int32
                    ? (int)DxfCode.ExtendedDataInteger32
                    : value is Double
                        ? (int)DxfCode.ExtendedDataReal
                        : (int)DxfCode.ExtendedDataAsciiString;

            dbo.XData = new ResultBuffer(new[]
            {
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, appName),
                new TypedValue(typeCode, value)
            });
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static string GetFirstXData(this ObjectId dboId, string appName)
        {
            return dboId.QOpenForRead().GetFirstXData(appName);
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void SetFirstXData(this ObjectId dboId, string appName, string value)
        {
            dboId.QOpenForWrite(dbo => dbo.SetFirstXData(appName, value));
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void SetFirstXDataT(this ObjectId dboId, string appName, object value) // newly 20111207
        {
            dboId.QOpenForWrite(dbo => dbo.SetFirstXDataT(appName, value));
        }

        /// <summary>
        /// Makes sure app names are registered.
        /// </summary>
        /// <param name="appNames">The app names.</param>
        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void AffirmRegApp(params string[] appNames) // newly 20130122
        {
            DbHelper.AffirmRegApp(HostApplicationServices.WorkingDatabase, appNames);
        }

        /// <summary>
        /// Makes sure app names are registered.
        /// </summary>
        /// <param name="db">The database to register to.</param>
        /// <param name="appNames">The app names.</param>
        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void AffirmRegApp(Database db, params string[] appNames) // newly 20130122
        {
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var table = trans.GetObject(db.RegAppTableId, OpenMode.ForRead) as RegAppTable;
                foreach (string appName in appNames)
                {
                    if (!table.Has(appName))
                    {
                        table.UpgradeOpen();
                        var record = new RegAppTableRecord
                        {
                            Name = appName
                        };
                        table.Add(record);
                        trans.AddNewlyCreatedDBObject(record, true);
                    }
                }
                trans.Commit();
            }
        }

        #endregion

        #region code

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void SetCode(this Entity entity, string code)
        {
            entity.XData = new ResultBuffer(new[]
            {
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, Consts.AppNameForCode),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, code)
            });
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void SetCode(this ObjectId entityId, string code)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = (Entity)trans.GetObject(entityId, OpenMode.ForWrite);
                DbHelper.SetCode(entity, code);
                trans.Commit();
            }
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static string GetCode(this Entity entity)
        {
            var resBuf = entity.GetXDataForApplication(Consts.AppNameForCode);
            if (resBuf == null)
            {
                return null;
            }
            foreach (var tValue in resBuf.AsArray())
            {
                if (tValue.TypeCode == (int)DxfCode.ExtendedDataAsciiString)
                {
                    return tValue.Value.ToString();
                }
            }
            return null;
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static string GetCode(this ObjectId entityId)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = (Entity)trans.GetObject(entityId, OpenMode.ForRead);
                return DbHelper.GetCode(entity);
            }
        }

        #endregion

        #region block attribute

        /// <summary>
        /// Returns a dictionary of a block reference's block attribute names and ObjectIds.
        /// </summary>
        /// <param name="blockReference">The block reference.</param>
        /// <returns>The result.</returns>
        public static Dictionary<string, ObjectId> GetBlockAttributeIds(this BlockReference blockReference)
        {
            var attrs = new Dictionary<string, ObjectId>();
            foreach (ObjectId attrId in blockReference.AttributeCollection)
            {
                // if block reference is already write enabled, trying to OpenForRead will throw.
                if (blockReference.IsWriteEnabled)
                {
                    attrId.QOpenForWrite<AttributeReference>(attr =>
                    {
                        attrs.Add(attr.Tag, attrId);
                    });
                }
                else
                {
                    var attr = attrId.QOpenForRead<AttributeReference>();
                    attrs.Add(attr.Tag, attrId);

                }
            }
            return attrs;
        }

        /// <summary>
        /// Gets block attributes.
        /// </summary>
        /// <param name="blockReference">The block reference.</param>
        /// <returns>The result.</returns>
        public static Dictionary<string, string> GetBlockAttributes(this BlockReference blockReference)
        {
            var attrs = new Dictionary<string, string>();
            foreach (ObjectId attrId in blockReference.AttributeCollection)
            {
                // if block reference is already write enabled, trying to OpenForRead will throw.
                if (blockReference.IsWriteEnabled)
                {
                    attrId.QOpenForWrite<AttributeReference>(attr =>
                   {
                       attrs.Add(attr.Tag, attr.TextString);
                   });
                }
                else
                {
                    var attr = attrId.QOpenForRead<AttributeReference>();
                    attrs.Add(attr.Tag, attr.TextString);
                }
            }
            return attrs;
        }

        /// <summary>
        /// Get block attribute.
        /// </summary>
        /// <param name="blockReference">The block reference.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>The value.</returns>
        public static string GetBlockAttribute(this BlockReference blockReference, string tag)
        {
            var attrs = DbHelper.GetBlockAttributes(blockReference);
            if (attrs.ContainsKey(tag))
            {
                return attrs[tag];
            }

            return null;
        }

        /// <summary>
        /// Defines a block attribute.
        /// </summary>
        /// <param name="blockName">The block name.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value.</param>
        /// <param name="prompt">The prompt.</param>
        /// <param name="position">The position.</param>
        /// <param name="style">The style.</param>
        public static void DefineBlockAttribute(string blockName, string tag, string value, string prompt, Point3d position, ObjectId style)
        {
            var ad = new AttributeDefinition(position, value, tag, prompt, style);
            using (var trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var block = trans.GetObject(DbHelper.GetBlockId(blockName), OpenMode.ForWrite) as BlockTableRecord;
                block.AppendEntity(ad);
                trans.AddNewlyCreatedDBObject(ad, true);
                trans.Commit();
            }
        }

        /// <summary>
        /// Appends an attribute to a block reference. This extends the native AppendAttribute Method.
        /// </summary>
        /// <param name="blockReference">The block reference.</param>
        /// <param name="attributeReference">The attribute definition.</param>
        /// <param name="overwrite">Overwrite if the attribute already exists.</param>
        /// <param name="createIfMissing">Create the attribute if it doesn't already exist.</param>
        public static void AppendAttribute(this BlockReference blockReference, AttributeReference attributeReference, bool overwrite = true, bool createIfMissing = true)
        {
            using (var trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var attrs = blockReference.GetBlockAttributes();
                if (!attrs.ContainsKey(attributeReference.Tag))
                {
                    if (createIfMissing)
                    {
                        blockReference.AttributeCollection.AppendAttribute(attributeReference);
                    }
                }
                else
                {
                    if (overwrite)
                    {
                        blockReference.AttributeCollection.AppendAttribute(attributeReference);
                    }
                }
                trans.Commit();
            }
        }

        #endregion

        #region ezdata

        public static string GetData(this ObjectId id, string dict, string key)
        {
            return CustomObjectDictionary.GetValue(id, dict, key);
        }

        public static void SetData(this ObjectId id, string dict, string key, string value)
        {
            CustomObjectDictionary.SetValue(id, dict, key, value);
        }

        #endregion

        #region tags

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static bool HasTag(this DBObject dbo, string tag)
        {
            var buffer = dbo.GetXDataForApplication(Consts.AppNameForTags);
            return buffer.AsArray().Any(x => x.TypeCode == (int)DxfCode.ExtendedDataAsciiString
                && x.Value == tag);
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static bool HasTag(this ObjectId id, string tag)
        {
            using (var trans = id.Database.TransactionManager.StartTransaction())
            {
                var dbo = (DBObject)trans.GetObject(id, OpenMode.ForWrite);
                return DbHelper.HasTag(dbo, tag);
            }
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void AddTag(this DBObject dbo, string tag)
        {
            var buffer = dbo.GetXDataForApplication(Consts.AppNameForTags);
            buffer.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, tag));
            dbo.XData = buffer;
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void AddTag(this ObjectId id, string tag)
        {
            using (var trans = id.Database.TransactionManager.StartTransaction())
            {
                var dbo = (DBObject)trans.GetObject(id, OpenMode.ForWrite);
                DbHelper.AddTag(dbo, tag);
                trans.Commit();
            }
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void RemoveTag(this DBObject dbo, string tag)
        {
            var buffer = dbo.GetXDataForApplication(Consts.AppNameForTags);
            var data = buffer.AsArray().Where(x => x.TypeCode == (int)DxfCode.ExtendedDataAsciiString
                && x.Value != tag).ToArray();
            dbo.XData = new ResultBuffer(data);
        }

        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void RemoveTag(this ObjectId id, string tag)
        {
            using (var trans = id.Database.TransactionManager.StartTransaction())
            {
                var dbo = (DBObject)trans.GetObject(id, OpenMode.ForWrite);
                DbHelper.RemoveTag(dbo, tag);
                trans.Commit();
            }
        }

        #endregion

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <remarks>
        /// Call this at the launch of your app and each time you create new doc.
        /// </remarks>
        [Obsolete("Legacy data store mechanism. Use FlexDataStore instead.")]
        public static void InitializeDatabase(Database db = null)
        {
            DbHelper.AffirmRegApp(db ?? HostApplicationServices.WorkingDatabase, new[]
            {
                Consts.AppNameForCode,
                Consts.AppNameForID,
                Consts.AppNameForName,
                Consts.AppNameForTags
            });
        }

        internal static Database GetDatabase(IEnumerable<ObjectId> objectIds)
        {
            return objectIds.Select(id => id.Database).Single();
        }
    }
}
