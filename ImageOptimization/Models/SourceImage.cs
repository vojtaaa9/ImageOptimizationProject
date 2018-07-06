using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ImageOptimization.Enums;
using ImageOptimization.Services;

namespace ImageOptimization.Models
{
    public class SourceImage
    {
        public int ID { get; set; }
        [Required]
        public String AbsolutePath { get; set; }
        [Required]
        public String RelativePath { get; set; }
        [Required]
        public String FileName { get; set; }
        [Required]
        public String AltText { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Format Format { get; set; }
        public long FileSize { get; set; }

        public virtual List<ThumbImage> Thumbnails { get; set; }
        public virtual List<ThumbImage> Formats { get; set; }
        public virtual List<ThumbImage> Compression { get; set; }
        public virtual List<ThumbImage> Metadata { get; set; }

        /// <summary>
        /// Get's File Size in human readable format (kB)
        /// </summary>
        /// <returns>File Size with two decimals</returns>
        public String getFileSize()
        {
            return (((float)FileSize) / 1024).ToString("0.00") + " kB";
        }


        /// <summary>
        /// Return Image in specified params. For SVG return optimized SVG File. Always
        /// </summary>
        /// <param name="format">Format of the Image</param>
        /// <param name="width">Desired width</param>
        /// <param name="height">Desired height</param>
        /// <param name="q">Quality of the image</param>
        /// <param name="strip">True to strip all metadata</param>
        /// <returns></returns>
        internal ThumbImage GetImage(Format format, int width, List<ThumbImage> list, int height = 0, int q = 100, bool strip = false)
        {
            // If Format is SVG, return optimized SVG. 
            if (Format == Format.SVG)
                return GetOptimizedSVG();

            // Get first thumbnail that matches dimensions, format, quality and metadata settings
            var thumbnail = list.Find(
                w => w.Format == format
                && (w.Width == width || w.Height == height)
                && (w.Quality == q)
                && (w.Stripped == strip)
                );

            // If any thumbnail is found, return it
            if (thumbnail != null)
                return thumbnail;

            // no thumbnail exists, let vips/magick.NET generate a new one
            thumbnail = ImageService.CreateImage(this, width, height, q, format, strip);

            return thumbnail;
        }


        /// <summary>
        /// Get's optimized SVG File
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <returns></returns>
        internal ThumbImage GetOptimizedSVG()
        {
            return Thumbnails[0];
        }

        /// <summary>
        /// Get's this SourceImage as ThumbImage
        /// </summary>
        /// <returns></returns>
        private ThumbImage GetThumbImage()
        {
            return new ThumbImage
            {
                ID = ID,
                FileName = FileName,
                RelativePath = RelativePath,
                AbsolutePath = AbsolutePath,
                AltText = AltText,
                Width = Width,
                Height = Height
            };
        }
    }
}