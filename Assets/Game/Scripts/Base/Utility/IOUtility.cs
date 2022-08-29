using System.IO;
using System.Text;

namespace Game
{
    public static class IOUtility
    {
        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void SaveFileSafe(string path, string fileName, string text)
        {
            CreateDirectoryIfNotExists(path);

            string filePath = Path.Combine(path, fileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                UTF8Encoding utf8Encoding = new UTF8Encoding(false);
                using (StreamWriter writer = new StreamWriter(stream, utf8Encoding))
                {
                    writer.Write(text);
                }
            }
        }
    }
}
