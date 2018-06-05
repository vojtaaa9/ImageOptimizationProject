﻿using System;
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

        internal ThumbImage GetThumbnail(int width, int height)
        {
            if (Thumbnails == null)
                return null;

            return Thumbnails.Find(w => (w.Width == width && w.Height == height));
        }
    }
}