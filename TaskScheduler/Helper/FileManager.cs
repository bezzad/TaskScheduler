using System;
using System.IO;
using System.Reflection;
using System.Text;
using NLog;

namespace TaskScheduler.Helper
{
    public static class FileManager
    {
        private static readonly Logger Nlogger = LogManager.GetCurrentClassLogger();

        public static bool WriteFileSafely(string path, string data)
        {
            try
            {
                CheckupDirectory(path);

                //Open the File
                File.WriteAllText(path, data, Encoding.UTF8);

                return true;
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
            }

            return false;
        }

        public static string ReadFileSafely(string path)
        {
            try
            {
                return !File.Exists(path) ? null : File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
            }

            return null;
        }

        public static string ReadResourceFile(string path)
        {
            var result = string.Empty;
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"{assembly.GetName().Name}.{path}";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                        using (var reader = new StreamReader(stream))
                        {
                            result = reader.ReadToEnd();
                            return result;
                        }
                }
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
            }

            return result;
        }

        public static void CheckupDirectory(string path)
        {
            var pathDir = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(pathDir)) return;


            if (!Directory.Exists(pathDir))
                Directory.CreateDirectory(pathDir);
        }
    }
}