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
    /// 函数式管道调用风格的一组工具函数
    /// </summary>
    public static class QuickSelection
    {
        //
        // QWhere|QPick|QSelect
        //

        public static IEnumerable<ObjectId> QWhere(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                return ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList().Where(filter).Select(x => x.ObjectId);
            }
        }

        public static IEnumerable<ObjectId> QWhere(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter)
        {
            return ids.QWhere(filter, HostApplicationServices.WorkingDatabase);
        }

        public static ObjectId QPick(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                try
                {
                    return ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList().First(filter).ObjectId;
                }
                catch
                {
                    return ObjectId.Null;
                }
            }
        }

        public static ObjectId QPick(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter)
        {
            return ids.QPick(filter, HostApplicationServices.WorkingDatabase);
        }

        public static IEnumerable<TResult> QSelect<TResult>(this IEnumerable<ObjectId> ids, Func<Entity, TResult> mapper, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                return ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList().Select(mapper);
            }
        }

        public static IEnumerable<TResult> QSelect<TResult>(this IEnumerable<ObjectId> ids, Func<Entity, TResult> mapper)
        {
            return ids.QSelect(mapper, HostApplicationServices.WorkingDatabase);
        }

        public static TResult QSelect<TResult>(this ObjectId entId, Func<Entity, TResult> mapper, Database db)
        {
            List<ObjectId> ids = new List<ObjectId> { entId };
            return ids.QSelect(mapper, db).First();
        }

        public static TResult QSelect<TResult>(this ObjectId entId, Func<Entity, TResult> mapper)
        {
            return entId.QSelect(mapper, HostApplicationServices.WorkingDatabase);
        }

        //
        // QOpenForRead
        //

        public static DBObject QOpenForRead(this ObjectId dboId, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                return trans.GetObject(dboId, OpenMode.ForRead);
            }
        }

        public static DBObject QOpenForRead(this ObjectId dboId)
        {
            return dboId.QOpenForRead(HostApplicationServices.WorkingDatabase);
        }

        public static T QOpenForRead<T>(this ObjectId dboId, Database db) where T : DBObject // newly 20130122
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                return trans.GetObject(dboId, OpenMode.ForRead) as T;
            }
        }

        public static T QOpenForRead<T>(this ObjectId dboId) where T : DBObject // newly 20130122
        {
            return dboId.QOpenForRead(HostApplicationServices.WorkingDatabase) as T;
        }

        public static IEnumerable<DBObject> QOpenForRead(this IEnumerable<ObjectId> ids, Database db) // newly 20120915
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                return ids.Select(x => trans.GetObject(x, OpenMode.ForRead)).ToList();
            }
        }

        public static IEnumerable<DBObject> QOpenForRead(this IEnumerable<ObjectId> ids) // newly 20120915
        {
            return ids.QOpenForRead(HostApplicationServices.WorkingDatabase);
        }

        public static IEnumerable<T> QOpenForRead<T>(this IEnumerable<ObjectId> ids, Database db) where T : DBObject // newly 20130122
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                return ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as T).ToList();
            }
        }

        public static IEnumerable<T> QOpenForRead<T>(this IEnumerable<ObjectId> ids) where T : DBObject // newly 20130122
        {
            return ids.QOpenForRead<T>(HostApplicationServices.WorkingDatabase);
        }

        //
        // QOpenForWrite
        //

        public static void QOpenForWrite(this ObjectId dboId, Action<DBObject> action, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                action(trans.GetObject(dboId, OpenMode.ForWrite));
                trans.Commit();
            }
        }

        public static void QOpenForWrite(this ObjectId dboId, Action<DBObject> action)
        {
            dboId.QOpenForWrite(action, HostApplicationServices.WorkingDatabase);
        }

        public static void QOpenForWrite<T>(this ObjectId dboId, Action<T> action, Database db) where T : DBObject // newly 20130411
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                action(trans.GetObject(dboId, OpenMode.ForWrite) as T);
                trans.Commit();
            }
        }

        public static void QOpenForWrite<T>(this ObjectId dboId, Action<T> action) where T : DBObject // newly 20130411
        {
            dboId.QOpenForWrite(action, HostApplicationServices.WorkingDatabase);
        }

        public static void QOpenForWrite(this IEnumerable<ObjectId> ids, Action<List<DBObject>> action, Database db) // newly 20120908
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var list = ids.Select(x => trans.GetObject(x, OpenMode.ForWrite)).ToList();
                action(list);
                trans.Commit();
            }
        }

        public static void QOpenForWrite(this IEnumerable<ObjectId> ids, Action<List<DBObject>> action) // newly 20120908
        {
            ids.QOpenForWrite(action, HostApplicationServices.WorkingDatabase);
        }

        public static void QOpenForWrite<T>(this IEnumerable<ObjectId> ids, Action<List<T>> action, Database db) where T : DBObject // newly 20130411
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var list = ids.Select(x => trans.GetObject(x, OpenMode.ForWrite) as T).ToList();
                action(list);
                trans.Commit();
            }
        }

        public static void QOpenForWrite<T>(this IEnumerable<ObjectId> ids, Action<List<T>> action) where T : DBObject // newly 20130411
        {
            ids.QOpenForWrite(action, HostApplicationServices.WorkingDatabase);
        }

        public static void QForEach(this IEnumerable<ObjectId> ids, Action<DBObject> action, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ids.Select(x => trans.GetObject(x, OpenMode.ForWrite)).ToList().ForEach(action);
                trans.Commit();
            }
        }

        public static void QForEach(this IEnumerable<ObjectId> ids, Action<DBObject> action)
        {
            ids.QForEach(action, HostApplicationServices.WorkingDatabase);
        }

        public static void QForEach<T>(this IEnumerable<ObjectId> ids, Action<T> action, Database db) where T : DBObject // newly 20130520
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ids.Select(x => trans.GetObject(x, OpenMode.ForWrite) as T).ToList().ForEach(action);
                trans.Commit();
            }
        }

        public static void QForEach<T>(this IEnumerable<ObjectId> ids, Action<T> action) where T : DBObject // newly 20130520
        {
            ids.QForEach(action, HostApplicationServices.WorkingDatabase);
        }

        //
        // Aggregation: QCount|QMin|QMax
        //

        public static int QCount(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                return ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList().Count(filter);
            }
        }

        public static int QCount(this IEnumerable<ObjectId> ids, Func<Entity, bool> filter)
        {
            return ids.QCount(filter, HostApplicationServices.WorkingDatabase);
        }

        public static double QMin(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                return ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList().Min(mapper);
            }
        }

        public static double QMin(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            return ids.QMin(mapper, HostApplicationServices.WorkingDatabase);
        }

        public static double QMax(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                return ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList().Max(mapper);
            }
        }

        public static double QMax(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            return ids.QMax(mapper, HostApplicationServices.WorkingDatabase);
        }

        public static ObjectId QMinEntity(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                var ents = ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList();
                double value = ents.Min(mapper);
                return ents.First(x => mapper(x) == value).ObjectId;
            }
        }

        public static ObjectId QMinEntity(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            return ids.QMinEntity(mapper, HostApplicationServices.WorkingDatabase);
        }

        public static ObjectId QMaxEntity(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                var ents = ids.Select(x => trans.GetObject(x, OpenMode.ForRead) as Entity).ToList();
                double value = ents.Max(mapper);
                return ents.First(x => mapper(x) == value).ObjectId;
            }
        }

        public static ObjectId QMaxEntity(this IEnumerable<ObjectId> ids, Func<Entity, double> mapper)
        {
            return ids.QMaxEntity(mapper, HostApplicationServices.WorkingDatabase);
        }

        //
        // Factory
        //

        public static ObjectId[] SelectAll()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionResult selRes = ed.SelectAll();
            if (selRes.Status == PromptStatus.OK)
            {
                return selRes.Value.GetObjectIds();
            }
            else
            {
                return new ObjectId[0];
            }
        }

        public static ObjectId[] SelectAll(string dxfType)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionResult selRes = ed.SelectAll(new SelectionFilter(new TypedValue[] { new TypedValue(0, dxfType) }));
            if (selRes.Status == PromptStatus.OK)
            {
                return selRes.Value.GetObjectIds();
            }
            else
            {
                return new ObjectId[0];
            }
        }

        private static IEnumerable<ObjectId> SelectAllInternal(this Database db, string block)
        {
            using (Transaction trans = db.TransactionManager.StartOpenCloseTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = trans.GetObject(bt[block], OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId id in modelSpace)
                {
                    yield return id;
                }
            }
        }

        public static ObjectId[] SelectAll(this Database db)
        {
            return db.SelectAllInternal(BlockTableRecord.ModelSpace).ToArray();
        }

        public static ObjectId[] SelectAll(this Database db, string block)
        {
            return db.SelectAllInternal(block).ToArray();
        }
    }
}
