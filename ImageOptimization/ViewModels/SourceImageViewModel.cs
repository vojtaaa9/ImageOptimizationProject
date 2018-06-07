using ImageOptimization.Models;
using System;
using System.Collections.Generic;

namespace ImageOptimization.ViewModels
{
    public class SourceImageViewModel
    {
        public int ID { get; set; }
        public String FileName { get; set; }
        public String FallbackPath { get; set; }
        public String Width { get; set; }
        public String Height { get; set; }
        public String FileFormat { get; set; }
        public String AltText { get; set; }
        public String Sizes { get; set; }
        public List<ThumbImage> Thumbnails { get; set; }
    }
}