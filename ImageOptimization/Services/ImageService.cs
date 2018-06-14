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
using System.Collections.Generic;

namespace ImageOptimization.Services
{
    public class ImageService
    {

        /// <summary>
        /// Create SourceImageViewModel for Detail View
        /// </summary>
        /// <param name="sourceImage">sourceImage</param>
        /// <returns>SourceImageViewModel with all data ready to be displayed</returns>
        public static SourceImageViewModel GetSourceImageViewModel(SourceImage sourceImage, List<ThumbImage> thumbs)
        {
            if (sourceImage == null)
                return null;

            // Build sizes component
            StringBuilder sizes = new StringBuilder();

            foreach(ThumbImage thumb in thumbs)
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
                Thumbnails = thumbs,
                Formats = sourceImage.Formats,
                Metadata = sourceImage.Metadata,
                Compression = sourceImage.Compression,
                Sizes = sourceImage.Format == Format.SVG ? "" : sizes.ToString(),
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
        internal static ThumbImage CreateImage(SourceImage src, int width, int? height = null, int q = 100, Format format = Format.JPEG, bool strip = false)
        {
            // Checks, if the source file exists
            if (src == null && File.Exists(src.AbsolutePath))
                return new ThumbImage();

            // No uknown formats shall pass!
            if (src.Format == Format.Unknown || format == Format.Unknown)
                return new ThumbImage();

            // Load Image to Vips
            Image image = Image.Thumbnail(src.AbsolutePath, width, height, "down", true);
            image = image.Copy();
            int h = image.Height;
            int w = image.Width;

            // Create FileName and Path
            String fileName = CreateThumbFilename(src, w, h, q, format.ToString().ToLower());
            String filePath = FileService.CombineDirectoryAndFilename(GetThumbnailPath(), fileName);

            switch (format)
            {
                case Format.JPEG:
                    image.Jpegsave(filePath, null, q, null, true, true, false, true, null, true, null, strip, new double[] { 255.0, 255.0, 255.0 });
                    break;
                case Format.GIF:
                    using (MagickImage imImage = new MagickImage(src.AbsolutePath))
                    {
                        imImage.Resize(w, h);
                        imImage.Quality = q;
                                                
                        if (strip)
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
                case Format.SVG:
                    return src.GetOptimizedSVG();
            }

            // Create new ThumbImage Model
            var thumb = new ThumbImage()
            {
                AbsolutePath = filePath,
                AltText = src.AltText,
                FileName = fileName,
                RelativePath = $"/thumbnails/{fileName}",
                Height = h,
                Width = w,
                SourceID = src.ID,
                Format = format,
                FileSize = new FileInfo(filePath).Length,
                Quality = q,
                Stripped = strip
            };

            return thumb;
        }

        /// <summary>
        /// Creates thubnail file name base on dupplied params
        /// </summary>
        /// <param name="q"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static String CreateThumbFilename(SourceImage src,int w, int h, int q, String format)
        {
            int opt = 1;

            // Check for existing files
            while(File.Exists($"{GetThumbnailPath()}/th_{w}x{h}_{q}_{src.AltText}{opt}.{format}"))
            {
                opt++;
            }

            // Extension string
            string ext = "";

            // In case that that is existing file, make appendix
            if (1 != opt)
                ext = opt.ToString();

            return $"th_{w}x{h}_{q}_{src.AltText}{opt}.{format}";
        }

        private static String GetThumbnailPath()
        {
            return HostingEnvironment.MapPath("~/thumbnails");
        }
    }
}