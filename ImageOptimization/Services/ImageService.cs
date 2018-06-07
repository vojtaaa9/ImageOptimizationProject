using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Hosting;
using ImageOptimization.Models;
using ImageOptimization.ViewModels;
using ImageOptimization.Enums;
using NetVips;

namespace ImageOptimization.Services
{
    public class ImageService
    {
        public static SourceImageViewModel GetSourceImageViewModel(SourceImage sourceImage, int width = 0, int height = 0)
        {
            if (sourceImage == null)
                return null;

            // Build sizes component
            StringBuilder sizes = new StringBuilder();

            foreach(ThumbImage thumb in sourceImage.Thumbnails)
            {
                sizes.Append(thumb.RelativePath);
                sizes.Append(" ");
                sizes.Append(thumb.Width);
                sizes.Append("w,");
            }

            return new SourceImageViewModel()
            {
                ID = sourceImage.ID,
                FileName = sourceImage.FileName,
                AltText = sourceImage.AltText,
                FallbackPath = sourceImage.RelativePath,
                Width = sourceImage.Width.ToString(),
                Height = sourceImage.Height.ToString(),
                FileFormat = sourceImage.Format.ToString(),
                Thumbnails = sourceImage.Thumbnails,
                Sizes = sizes.ToString()
            };
        }

        /// <summary>
        /// Generates thumbnail using vips of the source image, always downscales (no upscale), saves the generated
        /// file to thumbnails folder and returns new ThumbImage Model
        /// </summary>
        /// <param name="src">SourceImage</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Optional Height of the image</param>
        /// <returns>Empty ThumbImage if no file exists or Created ThumbImage</returns>
        internal static ThumbImage GenerateThumbnail(SourceImage src, int width, int? height)
        {
            // Checks, if the file exists
            if (src == null && File.Exists(src.AbsolutePath))
                return new ThumbImage();

            // Create Thumbnail using vips
            Image thumbnail = Image.Thumbnail(src.AbsolutePath, width, height, "down", true);

            string fileName = GenerateThumbnailFilename(thumbnail.Width, thumbnail.Height, src.FileName);
            String filePath = FileService.CombineDirectoryAndFilename(GetThumbnailPath(), fileName);

            // Create corresponding file
            try
            {
                // If the file already exists, don't create new one
                if (!File.Exists(filePath))
                {
                    var file = File.Create(filePath);
                    file.Close();
                    thumbnail.WriteToFile(filePath);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

            var thumb = new ThumbImage()
            {
                AbsolutePath = filePath,
                AltText = src.AltText,
                FileName = fileName,
                RelativePath = $"/thumbnails/{fileName}",
                Height = thumbnail.Height,
                Width = thumbnail.Width,
                SourceImageID = src.ID,
                Format = FileService.ParseFileFormat(Path.GetExtension(filePath))
            };

            return thumb;
        }

        public static ThumbImage ConvertToFormat(SourceImage sourceImage, Format format)
        {
            // Checks, if the file exists or convert isn't necessary
            if (sourceImage == null && File.Exists(sourceImage.AbsolutePath) 
                || sourceImage.Format == format)
                return new ThumbImage();

            // No uknown formats shall pass!
            if (sourceImage.Format == Format.Unknown || format == Format.Unknown)
                return null;

            // Create File
            String fileName = GenerateThumbnailFilename(sourceImage.Width, sourceImage.Height, sourceImage.AltText) + "." + format.ToString().ToLower();
            String filePath = FileService.CombineDirectoryAndFilename(GetThumbnailPath(), fileName);
            FileService.CreateFile(filePath);

            // Load Image to Vips
            Image image = Image.NewFromFile(sourceImage.AbsolutePath);


            switch (format)
            {
                case Format.JPEG:
                    image.Jpegsave(filePath, null, 100, null, false, true, true, null, null, false, null, false);
                    break;
                case Format.GIF:
                    // TODO: Houston, we've got a problem! Vips cant handle GIFs
                    image.Magicksave(filePath, "gif", 100);
                    break;
                case Format.PNG:
                    image.Pngsave(filePath, 0, false, strip: null);
                    break;
                case Format.WebP:
                    image.Webpsave(filePath, null, 100, true, nearLossless: true, strip: false);
                    break;
            }

            // Create new ThumbImage Model
            var thumb = new ThumbImage()
            {
                AbsolutePath = filePath,
                AltText = sourceImage.AltText,
                FileName = fileName,
                RelativePath = $"/thumbnails/{fileName}",
                Height = image.Height,
                Width = image.Width,
                SourceImageID = sourceImage.ID,
                Format = format
            };

            return thumb;
        }

        private static String GenerateThumbnailFilename(int width, int height, String srcFileName)
        {
            return $"th_{width}x{height}_{srcFileName}";
        }

        private static String GetThumbnailPath()
        {
            return HostingEnvironment.MapPath("~/thumbnails");
        }
    }
}