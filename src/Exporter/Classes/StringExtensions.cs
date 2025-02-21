namespace TiaFileFormatExporter.Classes
{
    public static class StringExtensions
    {
        public static string FixFileName(this string fileName, char fill = '_')
        {
            return string.Join(fill, fileName.Split(Path.GetInvalidFileNameChars()));
        }

        public static string FixPath(this string path, char fill = '_')
        {
            return string.Join(fill, path.Split(Path.GetInvalidPathChars()));
        }
    }
}
