using System.IO;

namespace ASID.Edge.Services
{
    public class LabelLoader
    {
        public string Load(string fileName)
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Labels",
                fileName);

            return File.ReadAllText(path);
        }
    }
}