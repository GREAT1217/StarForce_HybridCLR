using System.IO;

namespace Game.Editor
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
    }
}
