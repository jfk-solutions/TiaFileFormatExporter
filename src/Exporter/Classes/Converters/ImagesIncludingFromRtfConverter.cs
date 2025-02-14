using RtfDomParser;
using Siemens.Simatic.Hmi.Utah.Globalization;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.ExtensionMethods;
using TiaFileFormat.Helper;
using TiaFileFormat.Wrappers;
using TiaFileFormat.Wrappers.Images;

namespace TiaFileFormatExporter.Classes.Converters
{
    public class ImagesIncludingFromRtfConverter : ImagesConverter
    {
        override public IHighLevelObject Convert(StorageBusinessObject storageBusinessObject, ConvertOptions convertOptions)
        {
            if (storageBusinessObject?.GetChild<HmiInternalImageAttributes>() != null)
            {
                var imgDataAttr = storageBusinessObject.GetChild<HmiInternalImageAttributes>();
                var imgData = imgDataAttr.GenuineContent.Data;
                
                if (RichTextFormatHelper.IsRtf(imgData.Span))
                {
                    using var tr = new StringReader(imgDataAttr.GenuineContent.DataAsString);
                    var d = new RTFDomDocument();
                    d.Load(tr);
                    var image = d.Elements.Traverse<RTFDomElement>(x => x.Elements).OfType<RTFDomImage>().FirstOrDefault();
                    if (image.PicType == RTFPicType.Wmetafile)
                    {
                        return new Image(storageBusinessObject) { Name = storageBusinessObject.ProcessedName, Data = image.Data, ImageType = ImageType.WMF };
                    }
                    else if (image.PicType == RTFPicType.Emfblip)
                    {
                        return new Image(storageBusinessObject) { Name = storageBusinessObject.ProcessedName, Data = image.Data, ImageType = ImageType.EMF };
                    }
                    else if (image.PicType == RTFPicType.Pngblip)
                    {
                        return new Image(storageBusinessObject) { Name = storageBusinessObject.ProcessedName, Data = image.Data, ImageType = ImageType.PNG };
                    }
                    else if (image.PicType == RTFPicType.Wbitmap)
                    {
                        return new Image(storageBusinessObject) { Name = storageBusinessObject.ProcessedName, Data = image.Data, ImageType = ImageType.BMP };
                    }
                }
            }
            return base.Convert(storageBusinessObject, convertOptions);
        }
    }
}
