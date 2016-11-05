using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync
{
    public static class Utils
    {

        #region Property AppPath
        private static string _AppPath = null;
        public static string AppPath
        {
            get
            {
                if (_AppPath == null)
                {
                    _AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return _AppPath;
            }
        }

        #endregion


        #region Property LogPath
        private static string _LogPath = null;
        public static string LogPath
        {
            get
            {
                if (_LogPath == null)
                {
                    _LogPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Logs";
                    if (!Directory.Exists(_LogPath))
                        Directory.CreateDirectory(_LogPath);
                }
                return _LogPath;
            }
        }

        #endregion


        private static object logLock = new object();
        public static void Log(string p)
        {
            lock (logLock)
            {
                Trace.WriteLine(p);
                DateTime now = DateTime.Now;
                string logFile = string.Format("\\Log-{0:00}-{1:00}-{2:00}.txt", now.Year, now.Month, now.Day);

                using (StreamWriter sw = new StreamWriter(LogPath + logFile, true))
                {
                    sw.WriteLine(now.ToString() + "\t" + p);
                }
            }
        }
        public static void Execute(Action a)
        {
            try
            {
                a();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        public static void Execute(this SqlConnectionStringBuilder cb, string cmdText, Action<SqlDataReader> cmdAction)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(cb.ToString());
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;
                SqlDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                cmdAction(dr);
                conn.Close();
            }
            finally
            {
                conn.Close();
            }
        }

        public static void Execute(this SqlConnectionStringBuilder cb, string cmdText)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(cb.ToString());
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = int.MaxValue;
                cmd.CommandText = cmdText;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            finally
            {
                conn.Close();
            }
        }

    }
}
