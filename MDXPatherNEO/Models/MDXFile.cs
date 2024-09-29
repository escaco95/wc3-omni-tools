using System.IO;

namespace MDXPatherNEO.Models
{
    public class MDXFile
    {
        public string Path { get; private set; }

        public MDXData Data { get; private set; }

        private MDXFile(string path, MDXData data)
        {
            Path = path;
            Data = data;
        }

        public static MDXFile Load(string path)
        {
            byte[] fileBytes = File.ReadAllBytes(path);
            using MemoryStream stream = new(fileBytes);
            using BinaryReader reader = new(stream);
            return new MDXFile(path, MDXData.Load(reader));
        }

        public void Save()
        {
            // Write the MDXData to the Path
            using FileStream stream = new(Path, FileMode.Create);
            using BinaryWriter writer = new(stream);
            Data.Save(writer);
        }
    }
}
