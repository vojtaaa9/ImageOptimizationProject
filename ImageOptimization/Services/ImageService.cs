using ImageOptimization.Models;
using ImageOptimization.ViewModels;

namespace ImageOptimization.Services
{
    public class ImageService
    {
        public static SourceImageViewModel GetSourceImageViewModel(SourceImage sourceImage)
        {
            if (sourceImage == null)
                return null;

            return new SourceImageViewModel()
            {
                ID = sourceImage.ID,
                FileName = sourceImage.FileName,
                AltText = sourceImage.AltText,
                ServerPath = sourceImage.RelativePath
            };
        }

    }
}