using System.Text.Json;
using System.Text.Json.Serialization;
using TiaFileFormat.Database;
using TiaFileFormat.Wrappers.Converters.AutomationMl;
using TiaFileFormat.Wrappers.CrossReferences;
using TiaFileFormat.Wrappers.Hardware;

namespace TiaFileFormatExporter.Exporters;

public static class ExportProjectData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task ExportAml(TiaDatabaseFile database, string directory)
    {
        var conversion = new TiaHardwareProject(database).ConvertAll();
        Directory.CreateDirectory(directory);
        using (var stream = File.Create(Path.Combine(directory, "Devices.aml")))
            AutomationMlConverter.WriteAutomationMl(conversion, stream,
                new AutomationMlWriterOptions { FileName = "Devices.aml" });

        await WriteJson(Path.Combine(directory, "Devices.diagnostics.json"), new
        {
            ExportedDevices = conversion.Project.Devices.Count,
            SkippedDevices = conversion.SkippedDevices.Select(x => new { x.Name, x.SourceId }),
            Diagnostics = conversion.Diagnostics.Select(x => new
            {
                x.Code, x.Severity, x.SourceId, x.SourceName, x.Message
            })
        });
        Console.WriteLine($"AML: {conversion.Project.Devices.Count} devices exported, {conversion.SkippedDevices.Count} skipped.");
    }

    public static async Task ExportXref(TiaDatabaseFile database, string directory)
    {
        var catalog = CrossReferenceCatalogBuilder.Build(database);
        Directory.CreateDirectory(directory);
        // Use catalog IDs for links; storage objects and parent/child references form cycles.
        await WriteJson(Path.Combine(directory, "CrossReferences.json"), new
        {
            catalog.DataSource,
            catalog.Completeness,
            catalog.Diagnostics,
            Objects = catalog.Objects.Select(x => new
            {
                x.CatalogId, x.StorageInstanceId, x.StorageTypeId, x.Name, x.Address,
                x.TypeName, x.Device, x.Path, ParentId = x.Parent?.CatalogId,
                x.ResolutionKind, x.Origin
            }),
            Relations = catalog.Relations.Select(x => new
            {
                x.CatalogId, SourceId = x.Source.CatalogId, TargetId = x.Target.CatalogId,
                BindingObjectId = x.BindingObject?.CatalogId, x.ReferenceType,
                x.ResolutionKind, x.Origin,
                Locations = x.Locations.Select(location => new
                {
                    location.Name, location.ReferenceLocation, location.ReferencedAsName,
                    location.Address, location.TypeName, location.ReferenceType,
                    location.Access, location.ResolutionKind
                })
            }),
            Addresses = catalog.Addresses.Select(x => new
            {
                ObjectId = x.Object?.CatalogId, x.Type, x.Range, x.FirstBit, x.LastBit,
                x.ResolutionKind, x.Origin
            })
        });
        Console.WriteLine($"XRef: {catalog.Objects.Count} objects, {catalog.Relations.Count} relations ({catalog.DataSource}).");
        foreach (var diagnostic in catalog.Diagnostics)
            Console.Error.WriteLine($"XRef {diagnostic.Code}: {diagnostic.Message}");
    }

    private static async Task WriteJson<T>(string path, T value)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, JsonOptions);
    }
}
