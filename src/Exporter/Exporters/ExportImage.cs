using ImageMagick;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Images;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportCodeBlock : BaseExporter<Image>
    {
        public override async Task Export(StorageBusinessObject sb, Image image, string dir)
        {
            var file = FixPath(Path.Combine(dir, image.Name.FixFileName() + "." + image.ImageType.ToString().ToLowerInvariant()));
            File.WriteAllBytes(file, image.Data);
            if (parsedOptions.ConvertMetafilesToSvg && (image.ImageType == ImageType.EMF || image.ImageType == ImageType.WMF))
            {
                using var ms = new MemoryStream();
                using var magickImage = new MagickImage(image.Data);
                magickImage.Write(ms, MagickFormat.Svg);
                ms.Position = 0;
                using var sr = new StreamReader(ms);
                var text = sr.ReadToEnd();
                var file2 = FixPath(Path.Combine(dir, image.Name.FixFileName() + ".svg"));
                File.WriteAllText(file2, text);
            }
        }
    }
}
