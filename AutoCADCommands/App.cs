using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Dreambuild.AutoCAD
{
    /// <summary>
    /// Application and multi-docs.
    /// </summary>
    public static class App
    {
        /// <summary>
        /// Gets the current folder.
        /// </summary>
        public static string CurrentFolder
        {
            get
            {
                string s = Assembly.GetCallingAssembly().Location;
                return s.Remove(s.LastIndexOf('\\') + 1);
            }
        }

        /// <summary>
        /// Gets the document folder.
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
        /// Loads another assembly.
        /// </summary>
        /// <param name="dllName">The assembly file name (relative path).</param>
        public static void LoadDll(string dllName)
        {
            try
            {
                Assembly.LoadFrom(Path.Combine(App.CurrentFolder, dllName));
            }
            catch
            {
            }
        }

        #region multi doc

        /// <summary>
        /// Determines if a document is newly created.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>The result.</returns>
        public static bool IsDocumentNew(Document doc = null) // newly 20140730
        {
            return !App.IsDocumentSaved(doc ?? Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Determines if a document is saved.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>The result.</returns>
        public static bool IsDocumentSaved(Document doc = null) // newly 20140730
        {
            doc = doc ?? Application.DocumentManager.MdiActiveDocument;
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
        /// Gets all opened documents.
        /// </summary>
        /// <returns>All opened documents.</returns>
        public static List<Document> GetAllOpenedDocuments()
        {
            return App.GetAllOpenedDocInternal().ToList();
        }

        /// <summary>
        /// Locks the current document and do things.
        /// </summary>
        /// <param name="action">The things to do.</param>
        public static void LockAndExecute(Action action)
        {
            using (var doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                action();
            }
        }

        /// <summary>
        /// Locks a specified document and do things.
        /// </summary>
        /// <param name="doc">The document to lock.</param>
        /// <param name="action">Then things to do.</param>
        public static void LockAndExecute(Document doc, Action action)
        {
            using (var doclock = doc.LockDocument())
            {
                action();
            }
        }

        /// <summary>
        /// Locks the current document and do things.
        /// </summary>
        /// <typeparam name="T">The type of the return value.</typeparam>
        /// <param name="function">The things to do.</param>
        /// <returns>The value returned by the action.</returns>
        public static T LockAndExecute<T>(Func<T> function)
        {
            using (var doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                return function();
            }
        }

        /// <summary>
        /// Sets the active document.
        /// </summary>
        /// <param name="doc">The document to be set as active.</param>
        public static void SetActiveDocument(Document doc)
        {
            Application.DocumentManager.MdiActiveDocument = doc;
            HostApplicationServices.WorkingDatabase = doc.Database;
        }

        /// <summary>
        /// Sets the working database.
        /// </summary>
        /// <param name="db">The database to be set as the working database.</param>
        public static void SetWorkingDatabase(Database db)
        {
            HostApplicationServices.WorkingDatabase = db;
        }

        /// <summary>
        /// Opens a file as a document.
        /// </summary>
        /// <param name="file">The file name.</param>
        /// <returns>The opened document.</returns>
        public static Document OpenDocument(string file)
        {
            return Application.DocumentManager.Open(file, false);
        }

        /// <summary>
        /// Opens or activates a file as a document.
        /// </summary>
        /// <param name="file">The file name.</param>
        /// <returns>The opened document.</returns>
        public static Document OpenOrActivateDocument(string file)
        {
            var doc = App.FindOpenedDocument(file);
            if (doc != null)
            {
                App.SetActiveDocument(doc);
                return doc;
            }

            return App.OpenDocument(file);
        }

        /// <summary>
        /// Finds an opened document by file name.
        /// </summary>
        /// <param name="file">The file name.</param>
        /// <returns>The document found.</returns>
        public static Document FindOpenedDocument(string file)
        {
            return Application.DocumentManager
                .Cast<Document>()
                .FirstOrDefault(document => document.Name == file);
        }

        #endregion
    }

    /// <summary>
    /// The polyline needs cleanup exception.
    /// </summary>
    public class PolylineNeedsCleanupException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        public PolylineNeedsCleanupException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        /// <param name="message">The message.</param>
        public PolylineNeedsCleanupException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PolylineNeedsCleanupException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Shows a message dialog of this exception.
        /// </summary>
        public void ShowMessage()
        {
            Interaction.TaskDialog("Polyline needs a clean-up", "Go to clean it up.", "Go to clean it up.", "AutoCAD", "Please run `PolyClean`.");
        }
    }

    /// <summary>
    /// The log manager.
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// The log file name.
        /// </summary>
        public const string LogFile = "C:\\AcadCodePack.log";

        /// <summary>
        /// Writes object to logs.
        /// </summary>
        /// <param name="o">The object.</param>
        public static void Write(object o)
        {
            using (var sw = new StreamWriter(LogFile, append: true))
            {
                sw.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {o}");
            }
        }

        /// <summary>
        /// Writes object to logs.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="o">The object.</param>
        public static void Write(string note, object o)
        {
            using (var sw = new StreamWriter(LogFile, append: true))
            {
                sw.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {note}: {o}");
            }
        }

        /// <summary>
        /// Gets a time based name.
        /// </summary>
        /// <returns>The name.</returns>
        public static string GetTimeBasedName()
        {
            string timeString = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToLongTimeString();
            return new string(timeString.Where(c => char.IsDigit(c)).ToArray());
        }

        /// <summary>
        /// The log table.
        /// </summary>
        public class LogTable
        {
            private readonly int[] _colWidths;

            /// <summary>
            /// The tab width.
            /// </summary>
            public const int TabWidth = 8;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="colWidths">The column widths. The values should be multiples of the tab width.</param>
            public LogTable(params int[] colWidths)
            {
                _colWidths = colWidths;
            }

            /// <summary>
            /// Gets the string representation of a row.
            /// </summary>
            /// <param name="contents">The contents.</param>
            /// <returns>The result.</returns>
            public string GetRow(params object[] contents)
            {
                var sb = new StringBuilder();
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
            /// Gets string width: ASCII as 1, others as 2.
            /// </summary>
            /// <param name="content">The string.</param>
            /// <returns>The width.</returns>
            public static int GetStringWidth(string content)
            {
                return content.Sum(c => c > 255 ? 2 : 1);
            }
        }
    }

    /// <summary>
    /// ObjectARX C functions wrapper.
    /// </summary>
    public static class Arx
    {
        [DllImport("acad.exe", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedCmd")]
        public static extern int acedCmd(IntPtr vlist);

        [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ads_queueexpr(string strExpr);

        [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acedPostCommand@@YAHPB_W@Z")]
        public static extern int acedPostCommand(string strExpr);
    }
}
