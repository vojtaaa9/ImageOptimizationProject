using System;
using System.Collections.Generic;
using ImageOptimization.Models;

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
        public String FileSize { get; set; }
        public String HumanSize { get; set; }
        public List<ThumbImage> Thumbnails { get; set; }
        public List<CompareImage> Formats { get; set; }
        public List<CompareImage> Compression { get; set; }
        public List<ThumbImage> Metadata { get; set; }
    }
}