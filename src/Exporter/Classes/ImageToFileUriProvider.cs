using Siemens.Simatic.Hmi.Utah.Globalization;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Hmi;

namespace TiaFileFormatExporter.Classes
{
    public class ImageToFileUriProvider : IImageUriProvider
    {
        public string GetImageUrl(StorageBusinessObject storageObject)
        {
            var url = "../../../";
            var parent = storageObject;
            while(parent!=null)
            {
                url += "../";
                parent = parent.Parent;
            }
            var att = storageObject.GetChild<HmiInternalImageAttributes>();

            return url + "Images/" + storageObject.ProcessedName + att.FileExtension;
        }
    }
}
