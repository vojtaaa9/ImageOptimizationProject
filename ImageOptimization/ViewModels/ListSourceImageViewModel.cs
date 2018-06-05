using ImageOptimization.Models;
using System.Collections.Generic;

namespace ImageOptimization.ViewModels
{
    public class ListSourceImageViewModel
    {
        public List<ThumbImage> ImageItems { get; set; }
        public int Page { get; set; }
    }
}