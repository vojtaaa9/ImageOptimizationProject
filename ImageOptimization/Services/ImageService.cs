using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Hosting;
using ImageOptimization.Models;
using ImageOptimization.ViewModels;
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
                FileFormat = sourceImage.Format,
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
        /// <param name="height">Height of the image</param>
        /// <returns>Empty ThumbImage if no file exists or Created ThumbImage</returns>
        internal static ThumbImage GenerateThumbnail(SourceImage src, int width, int? height)
        {
            // Checks, if the file exists
            if (src == null && File.Exists(src.AbsolutePath))
                return new ThumbImage();

            // Create Thumbnail using vips
            Image thumbnail = Image.Thumbnail(src.AbsolutePath, width, height, "down", true);
            string ThumbnailPath = HostingEnvironment.MapPath("~/thumbnails");
            string FileName = $"th_{thumbnail.Width}x{thumbnail.Height}_{src.FileName}";
            string path = $"{ThumbnailPath}\\{FileName}";

            // Create corresponding file
            try
            {
                // If the file already exists, don't create new one
                if (!File.Exists(path))
                {
                    var file = File.Create(path);
                    file.Close();
                    thumbnail.WriteToFile(path);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

            var thumb = new ThumbImage()
            {
                AbsolutePath = path,
                AltText = src.AltText,
                FileName = FileName,
                RelativePath = $"/thumbnails/{FileName}",
                Height = thumbnail.Height,
                Width = thumbnail.Width,
                SourceImageID = src.ID
            };

            return thumb;
        }
    }
}