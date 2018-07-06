using System.Collections.Generic;
using ImageOptimization.Models;

namespace ImageOptimization.ViewModels
{
    public class ListSourceImageViewModel
    {
        public List<ThumbImage> ImageItems { get; set; }
        public int Page { get; set; }
    }
}