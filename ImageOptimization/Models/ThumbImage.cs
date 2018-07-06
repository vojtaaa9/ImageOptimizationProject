using System;
using System.ComponentModel.DataAnnotations;
using ImageOptimization.Enums;

namespace ImageOptimization.Models
{
    public class ThumbImage
    {
        public int ID { get; set; }
        public int SourceID { get; set; }
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
        public int Quality { get; set; }
        public bool Stripped { get; set; }

        /// <summary>
        /// Get's File Size in human readable format (kB)
        /// </summary>
        /// <returns>File Size with two decimals</returns>
        public String getFileSize()
        {
            return (((float) FileSize) / 1024).ToString("0.00") + " kB";
        }

        public String getFormat()
        {
            return Format.ToString();
        }
    }

    
}