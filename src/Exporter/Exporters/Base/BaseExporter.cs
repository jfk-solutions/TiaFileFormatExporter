using System.Text;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers;
using TiaFileFormat.Wrappers.Converters.Code;

namespace TiaFileFormatExporter.Exporters.Base
{
    public abstract class BaseExporter<T> : IExporter<T>
    {
        public abstract Task Export(StorageBusinessObject sb, T exportObject, string dir);

        public Task Export(StorageBusinessObject sb, object exportObject, string dir)
        {
            return Export(sb, (T)exportObject, dir);
        }

        protected Encoding encoding => Program.encoding;

        protected CodeBlockToSourceBlockConverter.ConvertOptions codeBlockConvertOptions => Program.codeBlockConvertOptions;

        protected ConvertOptions convertOptions => Program.convertOptions;

        protected Options parsedOptions => Program.parsedOptions;

        protected SemaphoreSlim maxBrowserTasks => Program.maxBrowserTasks;

        protected static string FixPath(string path)
        {
            return Program.ReplacePaths(path);
        }
    }
}
