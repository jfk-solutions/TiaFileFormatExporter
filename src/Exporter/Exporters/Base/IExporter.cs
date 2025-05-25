using TiaFileFormat.Database.StorageTypes;

namespace TiaFileFormatExporter.Exporters.Base
{
    public interface IExporter<T> : IExporter
    {
        Task Export(StorageBusinessObject sb, T exportObject, string dir);

        //bool ShouldBeExported(StorageBusinessObject sb);
    }

    public interface IExporter
    {
        Task Export(StorageBusinessObject sb, object exportObject, string dir);
    }
}
