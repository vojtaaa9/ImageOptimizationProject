using System;
using System.Diagnostics;
using System.IO;
using System.Web.Hosting;
using ImageOptimization.Models;
using ImageOptimization.ViewModels;
using NetVips;

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
                ServerPath = sourceImage.RelativePath,
                Width = sourceImage.Width,
                Height = sourceImage.Height,
                FileFormat = sourceImage.Format
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src">SourceImage</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <returns></returns>
        internal static ThumbImage GenerateThumbnail(SourceImage src, int width, int height)
        {
            // Checks, if the file exists
            if (src == null && File.Exists(src.AbsolutePath))
                return new ThumbImage();

            // Create Thumbnail using vips
            Image thumbnail = Image.Thumbnail(src.AbsolutePath, width, height, "down", true);
            string AppDataFolder = HostingEnvironment.MapPath("~/images");
            string FileName = $"th_{width}x{height}_{src.FileName}";
            string path = $"{AppDataFolder}\\{FileName}";

            // Create corresponding file
            try
            {
                var file = File.Create(path);
                file.Close();
                thumbnail.WriteToFile(path);
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
                RelativePath = $"/images/{FileName}",
                Height = height,
                Width = width,
                SourceImageID = src.ID
            };

            return thumb;
        }
    }
}