using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Hosting;
using ImageOptimization.Models;
using ImageOptimization.ViewModels;
using ImageOptimization.Enums;
using NetVips;
using ImageMagick;

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
                Sizes = sizes.ToString(),
                FileSize = $"{sourceImage.getFileSize()}, ({sourceImage.FileSize} Bytes)"
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
        internal static ThumbImage GenerateThumbnail(SourceImage src, int width, int? height = null, int q = 100, Format format = Format.JPEG)
        {
            // Checks, if the file exists
            if (src == null && File.Exists(src.AbsolutePath))
                return new ThumbImage();

            // Create Thumbnail using vips
            Image thumbnail = Image.Thumbnail(src.AbsolutePath, width, height, "down", true);

            string fileName = GenerateThumbnailFilename(thumbnail.Width, thumbnail.Height, Path.GetFileNameWithoutExtension(src.FileName), format);
            String filePath = FileService.CombineDirectoryAndFilename(GetThumbnailPath(), $"{fileName}");

            // Create corresponding file
            try
            {
                // Interlace JPEG's
                var jpeg = new VOption {
                    { "interlace", true}
                };

                thumbnail.WriteToFile(filePath, format == Format.JPEG ? jpeg : null);
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
                Format = FileService.ParseFileFormat(Path.GetExtension(filePath)),
                FileSize = new FileInfo(filePath).Length,
                Quality = q
            };

            return thumb;
        }

        public static ThumbImage ConvertToFormat(SourceImage sourceImage, Format format, bool strip = false, int q = 100)
        {
            // Checks, if the file exists or convert isn't necessary
            if (sourceImage == null || (sourceImage == null && File.Exists(sourceImage.AbsolutePath)))
                return new ThumbImage();

            // No uknown formats shall pass!
            if (sourceImage.Format == Format.Unknown || format == Format.Unknown)
                return null;

            // Create File
            String fileName = GenerateThumbnailFilename(sourceImage.Width, sourceImage.Height, sourceImage.AltText, format);
            String filePath = FileService.CombineDirectoryAndFilename(GetThumbnailPath(), fileName);

            // Load Image to Vips
            Image image = Image.NewFromFile(sourceImage.AbsolutePath);

            switch (format)
            {
                case Format.JPEG:
                    image.Jpegsave(filePath, null, q, null, true, true, false, true, null, true, null, strip);
                    break;
                case Format.GIF:
                    using (MagickImage imImage = new MagickImage(sourceImage.AbsolutePath))
                    {
                        imImage.Quality = q;
                        if(strip)
                            imImage.Strip();
                        imImage.Write(filePath);
                    }
                    break;
                case Format.PNG:
                    // Compression ratio is inverse of quality
                    int pngQ = (q - 100) * -1;

                    image.Pngsave(filePath, pngQ, false, strip: strip);
                    break;
                case Format.WebP:
                    if (q == 100)
                        image.Webpsave(filePath, null, q, true, nearLossless: true, strip: strip);
                    else
                        image.Webpsave(filePath, null, q, false, smartSubsample: true, strip: strip);
                    break;
                case Format.TIFF:
                    image.Tiffsave(filePath, strip: strip);
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
                Format = format,
                FileSize = new FileInfo(filePath).Length,
                Quality = q
            };

            return thumb;
        }

        private static String GenerateThumbnailFilename(int width, int height, String srcFileName, Format format)
        {
            return $"th_{width}x{height}_{srcFileName}.{format.ToString().ToLower()}";
        }

        private static String GetThumbnailPath()
        {
            return HostingEnvironment.MapPath("~/thumbnails");
        }
    }
}