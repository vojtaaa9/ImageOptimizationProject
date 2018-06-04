using System;

namespace ImageOptimization.ViewModels
{
    public class SourceImageViewModel
    {
        public int ID { get; set; }
        public String FileName { get; set; }
        public String ServerPath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public String FileFormat { get; set; }
        public String AltText { get; set; }
    }
}