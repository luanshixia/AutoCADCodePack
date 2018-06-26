using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCADCommands
{
    /// <summary>
    /// Quick selection toolbox.
    /// </summary>
    public static class QuickSelection
    {
        // TODO: remove all obsolete methods.

        #region QWhere|QPick|QSelect

        /// <summary>
        /// QLinq Where.
        /// </summary>
        /// <param name="ids">The object IDs.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>Filtered IDs.</returns>
        public static IEnumerable<ObjectId> QWhere(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter)
        {
            return ids
                .QOpenForRead<Entity>()
                .Where(filter)
                .Select(entity => entity.ObjectId);
        }

        [Obsolete("Use QOpenForRead().")]
        public static ObjectId QPick(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter)
        {
            // TODO: verify that the default works.
            return ids
                .QOpenForRead<Entity>()
                .FirstOrDefault()
                .ObjectId;
        }

        /// <summary>
        /// QLinq Select.
        /// </summary>
        /// <param name="ids">The object IDs.</param>
        /// <param name="mapper">The mapper.</param>
        /// <returns>Mapped results.</returns>
        public static IEnumerable<TResult> QSelect<TResult>(this IEnumerable<ObjectId> ids, Func<Entity, TResult> mapper)
        {
            return ids
                .QOpenForRead<Entity>()
                .Select(mapper);
        }

        [Obsolete("Use QOpenForRead().")]
        public static TResult QSelect<TResult>(this ObjectId entId, Func<Entity, TResult> mapper)
        {
            var ids = new List<ObjectId> { entId };
            return ids.QSelect(mapper).First();
        }

        #endregion

        #region QOpenForRead

        /// <summary>
        /// Opens object for read.
        /// </summary>
        /// <param name="id">The object ID.</param>
        /// <returns>The opened object.</returns>
        public static DBObject QOpenForRead(this ObjectId id)
        {
            using (var trans = id.Database.TransactionManager.StartOpenCloseTransaction())
            {
                return trans.GetObject(id, OpenMode.ForRead);
            }
        }

        /// <summary>
        /// Opens object for read.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="id">The object ID.</param>
        /// <returns>The opened object.</returns>
        public static T QOpenForRead<T>(this ObjectId id) where T : DBObject // newly 20130122
        {
            return id.QOpenForRead() as T;
        }

        /// <summary>
        /// Opens objects for read.
        /// </summary>
        /// <param name="ids">The object IDs.</param>
        /// <returns>The opened object.</returns>
        public static DBObject[] QOpenForRead(this IEnumerable<ObjectId> ids) // newly 20120915
        {
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartTransaction())
            {
                return ids.Select(x => trans.GetObject(x, OpenMode.ForRead)).ToArray();
            }
        }

        /// <summary>
        /// Opens objects for read.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="ids">The object IDs.</param>
        /// <returns>The opened object.</returns>
        public static T[] QOpenForRead<T>(this IEnumerable<ObjectId> ids) where T : DBObject // newly 20130122
        {
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartTransaction())
            {
                return ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as T).ToArray();
            }
        }

        #endregion

        #region QOpenForWrite

        /// <summary>
        /// Opens object for write.
        /// </summary>
        /// <param name="id">The object ID.</param>
        /// <param name="action">The action.</param>
        public static void QOpenForWrite(this ObjectId id, Action<DBObject> action)
        {
            using (var trans = id.Database.TransactionManager.StartTransaction())
            {
                action(trans.GetObject(id, OpenMode.ForWrite));
                trans.Commit();
            }
        }

        /// <summary>
        /// Opens object for write.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="id">The object ID.</param>
        /// <param name="action">The action.</param>
        public static void QOpenForWrite<T>(this ObjectId id, Action<T> action) where T : DBObject // newly 20130411
        {
            using (var trans = id.Database.TransactionManager.StartTransaction())
            {
                action(trans.GetObject(id, OpenMode.ForWrite) as T);
                trans.Commit();
            }
        }

        /// <summary>
        /// Opens object for write.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="id">The object ID.</param>
        /// <param name="action">The action.</param>
        public static void QOpenForWrite<T>(this ObjectId id, Func<T, DBObject[]> action) where T : DBObject
        {
            using (var trans = id.Database.TransactionManager.StartTransaction())
            {
                var newObjects = action(trans.GetObject(id, OpenMode.ForWrite) as T).ToList();
                newObjects.ForEach(newObject => trans.AddNewlyCreatedDBObject(newObject, true));
                trans.Commit();
            }
        }

        /// <summary>
        /// Opens objects for write.
        /// </summary>
        /// <param name="ids">The object IDs.</param>
        /// <param name="action">The action.</param>
        public static void QOpenForWrite(this IEnumerable<ObjectId> ids, Action<DBObject[]> action) // newly 20120908
        {
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartTransaction())
            {
                var list = ids.Select(x => trans.GetObject(x, OpenMode.ForWrite)).ToArray();
                action(list);
                trans.Commit();
            }
        }

        /// <summary>
        /// Opens objects for write.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="ids">The object IDs.</param>
        /// <param name="action">The action.</param>
        public static void QOpenForWrite<T>(this IEnumerable<ObjectId> ids, Action<T[]> action) where T : DBObject // newly 20130411
        {
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartTransaction())
            {
                var list = ids.Select(x => trans.GetObject(x, OpenMode.ForWrite) as T).ToArray();
                action(list);
                trans.Commit();
            }
        }

        /// <summary>
        /// Opens objects for write.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="ids">The object IDs.</param>
        /// <param name="action">The action.</param>
        public static void QOpenForWrite<T>(this IEnumerable<ObjectId> ids, Func<T[], DBObject[]> action) where T : DBObject
        {
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartTransaction())
            {
                var list = ids.Select(x => trans.GetObject(x, OpenMode.ForWrite) as T).ToArray();
                var newObjects = action(list).ToList();
                newObjects.ForEach(newObject => trans.AddNewlyCreatedDBObject(newObject, true));
                trans.Commit();
            }
        }

        /// <summary>
        /// Opens objects for write (for each).
        /// </summary>
        /// <param name="ids">The object IDs.</param>
        /// <param name="action">The action.</param>
        public static void QForEach(this IEnumerable<ObjectId> ids, Action<DBObject> action)
        {
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartTransaction())
            {
                ids.Select(x => trans.GetObject(x, OpenMode.ForWrite)).ToList().ForEach(action);
                trans.Commit();
            }
        }

        /// <summary>
        /// Opens objects for write (for each).
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="ids">The object IDs.</param>
        /// <param name="action">The action.</param>
        public static void QForEach<T>(this IEnumerable<ObjectId> ids, Action<T> action) where T : DBObject // newly 20130520
        {
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartTransaction())
            {
                ids.Select(x => trans.GetObject(x, OpenMode.ForWrite) as T).ToList().ForEach(action);
                trans.Commit();
            }
        }

        #endregion

        #region Aggregation: QCount|QMin|QMax

        [Obsolete("Use QOpenForRead().")]
        public static int QCount(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter)
        {
            return ids
                .QOpenForRead<Entity>()
                .Count(filter);
        }

        [Obsolete("Use QOpenForRead().")]
        public static double QMin(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            return ids
                .QOpenForRead<Entity>()
                .Min(mapper);
        }

        [Obsolete("Use QOpenForRead().")]
        public static double QMax(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            return ids
                .QOpenForRead<Entity>()
                .Max(mapper);
        }

        [Obsolete("Use QOpenForRead().")]
        public static ObjectId QMinEntity(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            // Bad implementation.
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartOpenCloseTransaction())
            {
                var ents = ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList();
                double value = ents.Min(mapper);
                return ents.First(x => mapper(x) == value).ObjectId;
            }
        }

        [Obsolete("Use QOpenForRead().")]
        public static ObjectId QMaxEntity(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            // Bad implementation.
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartOpenCloseTransaction())
            {
                var ents = ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList();
                double value = ents.Max(mapper);
                return ents.First(x => mapper(x) == value).ObjectId;
            }
        }

        #endregion

        #region Factory methods

        /// <summary>
        /// Selects all entities in current editor.
        /// </summary>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] SelectAll()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var selRes = ed.SelectAll();
            if (selRes.Status == PromptStatus.OK)
            {
                return selRes.Value.GetObjectIds();
            }

            return Array.Empty<ObjectId>();
        }

        /// <summary>
        /// Selects all entities with specified DXF type in current editor.
        /// </summary>
        /// <param name="dxfType">The DXF type.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] SelectAll(string dxfType)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var selRes = ed.SelectAll(new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, dxfType) }));
            if (selRes.Status == PromptStatus.OK)
            {
                return selRes.Value.GetObjectIds();
            }

            return Array.Empty<ObjectId>();
        }

        private static IEnumerable<ObjectId> SelectAllInternal(this Database db, string block)
        {
            using (var trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                var bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelSpace = trans.GetObject(bt[block], OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId id in modelSpace)
                {
                    yield return id;
                }
            }
        }

        /// <summary>
        /// Selects all entities in specified database's model space.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] SelectAll(this Database db)
        {
            return db.SelectAllInternal(BlockTableRecord.ModelSpace).ToArray();
        }

        /// <summary>
        /// Selects all entities in specified database's specified block.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="block">The block.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] SelectAll(this Database db, string block)
        {
            return db.SelectAllInternal(block).ToArray();
        }

        #endregion
    }
}
