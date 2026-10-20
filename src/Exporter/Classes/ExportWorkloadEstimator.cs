using System.Xml.Linq;
using Siemens.Automation.ObjectFrame;
using Siemens.Simatic.Lang.Model.Structures;
using TiaFileFormat.Database.Business;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.CodeBlocks;

namespace TiaFileFormatExporter.Classes;

internal sealed class ExportWorkloadEstimator
{
    private const long Limit = 1_000_000_000_000;
    private readonly Dictionary<StorageBusinessObject, long> estimates = new();

    public long Estimate(StorageBusinessObject source)
    {
        var storedSize = Math.Max(1L, source.Header?.StorageObjectLength ?? 0);
        if (source.Header == null) return storedSize;
        try
        {
            if (!CodeBlockConverter.IsConvertableObject(source)) return storedSize;
            var declaration = source.GetRelationsWithNameResolved(Relation.Names.CurrentInterface).FirstOrDefault()
                ?? source.GetRelationsWithNameResolved(Relation.Names.CurrentLibraryInterface).FirstOrDefault();
            return Math.Max(storedSize, declaration == null ? 0 : EstimatePart(declaration, new()));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // Estimation must never prevent the normal converter from reporting
            // unsupported or malformed data through the existing export path.
            return storedSize;
        }
    }

    private long EstimatePart(StorageBusinessObject source, HashSet<StorageBusinessObject> path)
    {
        if (estimates.TryGetValue(source, out var estimate)) return estimate;
        if (path.Count >= 128 || !path.Add(source)) return 0;
        try
        {
            var part = source.GetChild<InterfacePartData>();
            if (part == null) return 0;
            var xml = part.Payload?.DataAsString;
            long size = xml?.Length ?? 0;
            var used = source.GetRelationsWithNameResolved(Relation.Names.UsedParts).ToArray();
            if (part.Kind == InterfacePartKind.Values)
            {
                foreach (var referenced in used)
                    if (referenced?.GetChild<InterfacePartData>()?.Kind is InterfacePartKind.BlockSource or InterfacePartKind.DataTypeSource)
                        size = Add(size, EstimatePart(referenced, path));
            }
            else if (!string.IsNullOrEmpty(xml))
            {
                foreach (var member in XElement.Parse(xml).DescendantsAndSelf())
                {
                    if (((string?)member.Attribute("Type"))?.StartsWith("REF_TO", StringComparison.Ordinal) == true)
                        continue;
                    if (int.TryParse((string?)member.Attribute("SubPartIndex"), out var index) &&
                        index >= 0 && index < used.Length && used[index] != null)
                        size = Add(size, EstimatePart(used[index], path));
                }
            }
            estimates[source] = size;
            return size;
        }
        finally { path.Remove(source); }
    }

    private static long Add(long left, long right) => Math.Min(Limit, left + Math.Min(Limit, right));
}
