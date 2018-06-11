using ImageOptimization.Enums;
using ImageOptimization.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        /// <summary>
        /// Get's Thumbnail Model for specified width or height. SVG Files returns themselves in form of ThumbImage Model
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>ThumbnailImage Model</returns>
        private ThumbImage GetThumbnail(int width, int height = 0, int quality = 100)
        {
            // SVG Format doesnt need any thumbnail
            if (Format == Format.SVG)
                return GetThumbImage();

            // If Thumbnail list is not initialized, return null
            if (Thumbnails == null)
                return new ThumbImage();

            // Get first thumbnail that matches dimensions is chosen format
            var thumbnail = Thumbnails.Find(
                w => w.Format == Format
                && (w.Width == width || w.Height == height)
                );

            // If any thumbnail is found, return it
            if (thumbnail != null)
                return thumbnail;

            // no thumbnail exists, let vips generate a new one
            thumbnail = ImageService.GenerateThumbnail(GetThumbImage(), width, q: quality);
            Thumbnails.Add(thumbnail);

            return thumbnail;
        }

        internal ThumbImage GetThumbnailInFormat(Format format, int width, int height = 0, bool strip = false, int q = 100)
        {
            // If no format change is requested or is SVG
            if (Format == format || format == Format.SVG || Format == Format.SVG)
                return GetThumbnail(width, height, q);

            // If Thumbnail list is not initialized, return empty thumbnail
            if (Thumbnails == null)
                return new ThumbImage();

            // Get first thumbnail that matches dimensions, format and quality
            var thumbnail = Thumbnails.Find(
                w => w.Format == format
                && (w.Width == width || w.Height == height)
                && (w.Quality == q)
                );

            // If any thumbnail is found, return it
            if (thumbnail != null)
                return thumbnail;

            // no thumbnail exists, let vips generate a new one
            var converted = ImageService.ConvertToFormat(this, format, strip, q);
            thumbnail = ImageService.GenerateThumbnail(converted, width, q: q);
            
            Thumbnails.Add(thumbnail);

            return thumbnail;
        }

        internal ThumbImage GetImageInFormat(Format format)
        {
            // Get first thumbnail that matches dimensions is chosen format
            var thumbnail = Thumbnails.Find(w => w.Format == format);

            // If any thumbnail is found, return it
            if (thumbnail != null)
                return thumbnail;

            // no thumbnail exists, let vips generate a new one
            thumbnail = ImageService.ConvertToFormat(this, format);
            Thumbnails.Add(thumbnail);

            return thumbnail;
        }

        private ThumbImage GetThumbImage()
        {
            return new ThumbImage()
            {
                ID = this.ID,
                SourceImageID = this.ID,
                FileName = this.FileName,
                RelativePath = this.RelativePath,
                AbsolutePath = this.AbsolutePath,
                AltText = this.AltText,
                Width = this.Width,
                Height = this.Height
            };
        }
    }
}