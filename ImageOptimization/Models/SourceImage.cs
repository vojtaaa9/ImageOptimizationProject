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
        public String Format { get; set; }

        public virtual List<ThumbImage> Thumbnails { get; set; }

        /// <summary>
        /// Get's Thumbnail Model for specified width or height. SVG Files returns themselves in form of ThumbImage Model
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>ThumbnailImage Model</returns>
        internal ThumbImage GetThumbnail(int width, int height = 0)
        {
            // SVG Format doesnt need any
            if (Format.Equals(".svg"))
                return GetThumbImage();

            // If Thumbnail list is not initialized, return null
            if (Thumbnails == null)
                return null;

            return Thumbnails.Find(w => (w.Width == width || w.Height == height));
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