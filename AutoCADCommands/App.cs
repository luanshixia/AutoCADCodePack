using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace AutoCADCommands
{
    /// <summary>
    /// 应用程序相关，并提供一组用于多文档开发的方法
    /// </summary>
    public static class App
    {
        /// <summary>
        /// 获取程序目录
        /// </summary>
        public static string CurrentFolder
        {
            get
            {
                string s = System.Reflection.Assembly.GetCallingAssembly().Location;
                return s.Remove(s.LastIndexOf('\\') + 1);
            }
        }

        /// <summary>
        /// 获取活动文档目录
        /// </summary>
        public static string DocumentFolder
        {
            get
            {
                string s = Application.DocumentManager.MdiActiveDocument.Name;
                if (s.Contains(':'))
                {
                    return s.Remove(s.LastIndexOf('\\') + 1);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 加载其他程序集
        /// </summary>
        /// <param name="dllName">程序集文件名（相对路径）</param>
        public static void LoadDll(string dllName)
        {
            try
            {
                System.Reflection.Assembly.LoadFrom(App.CurrentFolder + dllName);
            }
            catch
            {
            }
        }

        #region multi doc

        public static bool IsDocumentNew(Document doc = null) // newly 20140730
        {
            if (doc == null)
            {
                doc = Application.DocumentManager.MdiActiveDocument;
            }
            return !IsDocumentSaved(doc);
        }

        public static bool IsDocumentSaved(Document doc = null) // newly 20140730
        {
            if (doc == null)
            {
                doc = Application.DocumentManager.MdiActiveDocument;
            }
            return doc.Name != null && doc.Name.Contains(":");
        }

        private static IEnumerable<Document> GetAllOpenedDocInternal()
        {
            foreach (Document doc in Application.DocumentManager)
            {
                yield return doc;
            }
        }

        /// <summary>
        /// 获取所有打开的文档
        /// </summary>
        /// <returns>文档集合</returns>
        public static List<Document> GetAllOpenedDoc()
        {
            return GetAllOpenedDocInternal().ToList();
        }

        /// <summary>
        /// 锁定当前文档并执行操作
        /// </summary>
        /// <param name="action">要执行的函数</param>
        public static void LockAndExecute(Action action)
        {
            using (DocumentLock doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                action();
            }
        }

        /// <summary>
        /// 锁定制定文档并执行操作
        /// </summary>
        /// <param name="doc">要锁定的文档</param>
        /// <param name="action">要执行的函数</param>
        public static void LockAndExecute(Document doc, Action action)
        {
            using (DocumentLock doclock = doc.LockDocument())
            {
                action();
            }
        }

        /// <summary>
        /// 锁定当前文档并执行操作
        /// </summary>
        /// <typeparam name="T">操作返回值的类型</typeparam>
        /// <param name="function">要执行的操作</param>
        /// <returns>操作返回的结果</returns>
        public static T LockAndExecute<T>(Func<T> function)
        {
            using (DocumentLock doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                return function();
            }
        }

        /// <summary>
        /// 设置活动文档
        /// </summary>
        /// <param name="doc">文档</param>
        public static void SetActiveDocument(Document doc)
        {
            Application.DocumentManager.MdiActiveDocument = doc;
            HostApplicationServices.WorkingDatabase = doc.Database;
        }

        /// <summary>
        /// 设置工作数据库
        /// </summary>
        /// <param name="db">数据库</param>
        public static void SetWorkingDatabase(Database db)
        {
            HostApplicationServices.WorkingDatabase = db;
        }

        /// <summary>
        /// 打开文档
        /// </summary>
        /// <param name="file">文件名</param>
        /// <returns>打开的文档</returns>
        public static Document OpenDocument(string file)
        {
            return Application.DocumentManager.Open(file, false);
        }

        /// <summary>
        /// 打开或激活文档
        /// </summary>
        /// <param name="file">文件名</param>
        /// <returns>打开的文档</returns>
        public static Document OpenOrActivateDocument(string file)
        {
            Document doc = FindOpenedDocument(file);
            if (doc != null)
            {
                SetActiveDocument(doc);
                return doc;
            }
            else
            {
                return OpenDocument(file);
            }
        }

        /// <summary>
        /// 查找打开的文档
        /// </summary>
        /// <param name="file">文件名</param>
        /// <returns>找到的文档</returns>
        public static Document FindOpenedDocument(string file)
        {
            if (Application.DocumentManager.Cast<Document>().Any(x => x.Name == file))
            {
                return Application.DocumentManager.Cast<Document>().First(x => x.Name == file);
            }
            else
            {
                return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// 表示异常：多段线需要清理
    /// </summary>
    public class PolylineNeedCleanException : System.Exception
    {
        public PolylineNeedCleanException(string message)
            : base(message)
        {
        }

        public void ShowMessage()
        {
            Interaction.TaskDialog("多段线需要清理。", "去清理", "还是去清理", "AutoCAD", "请运行PolyClean命令。");
        }
    }

    /// <summary>
    /// 提供一组创建日志的方法
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// 日志文件名
        /// </summary>
        public const string LogFile = "C:\\AcadCodePack.log";

        /// <summary>
        /// 将对象写入日志
        /// </summary>
        /// <param name="o">对象</param>
        public static void Write(object o)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(LogFile, true);
            sw.WriteLine(string.Format("[{0} {1}] {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), o));
            sw.Close();
        }

        /// <summary>
        /// 将对象写入日志
        /// </summary>
        /// <param name="note">说明</param>
        /// <param name="o">对象</param>
        public static void Write(string note, object o)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(LogFile, true);
            sw.WriteLine(string.Format("[{0} {1}] {2}: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), note, o));
            sw.Close();
        }

        /// <summary>
        /// 获取基于时间的名称
        /// </summary>
        /// <returns>名称</returns>
        public static string GetTimeBasedName()
        {
            string s = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToLongTimeString();
            return new string(s.Where(x => char.IsDigit(x)).ToArray());
        }

        /// <summary>
        /// 纯文本表格
        /// </summary>
        public class LogTable
        {
            private int[] _colWidths;
            /// <summary>
            /// Tab字符的宽度，为8
            /// </summary>
            public const int TabWidth = 8;

            /// <summary>
            /// 初始化新实例
            /// </summary>
            /// <param name="colWidths">列宽，应为TabWidth的整数倍</param>
            public LogTable(params int[] colWidths)
            {
                _colWidths = colWidths;
            }

            /// <summary>
            /// 获取一行的字符串表示
            /// </summary>
            /// <param name="contents">行中元素</param>
            /// <returns>字符串表示</returns>
            public string GetRow(params object[] contents)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < contents.Length; i++)
                {
                    string content = contents[i].ToString();
                    sb.Append(content);
                    int nTab = (int)Math.Ceiling((double)(_colWidths[i] - GetStringWidth(content)) / TabWidth);
                    for (int j = 0; j < nTab; j++)
                    {
                        sb.Append('\t');
                    }
                }
                return sb.ToString();
            }

            /// <summary>
            /// 统计字符串宽度，ASCII字符宽度为1，其他字符宽度为2
            /// </summary>
            /// <param name="content">字符串</param>
            /// <returns>宽度</returns>
            public static int GetStringWidth(string content)
            {
                return content.Sum(c => c > 255 ? 2 : 1);
            }
        }
    }

    public static class Arx
    {
        [DllImport("acad.exe", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedCmd")]
        public static extern int acedCmd(System.IntPtr vlist);

        [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ads_queueexpr(string strExpr);

        [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acedPostCommand@@YAHPB_W@Z")]
        public static extern int acedPostCommand(string strExpr);
    }
}
