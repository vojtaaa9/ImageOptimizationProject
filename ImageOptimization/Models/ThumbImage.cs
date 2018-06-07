using ImageOptimization.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ImageOptimization.Models
{
    public class ThumbImage
    {
        public int ID { get; set; }
        public int SourceImageID { get; set; }
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
    }
}