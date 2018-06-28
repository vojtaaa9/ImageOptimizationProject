namespace ImageOptimization.Models
{
    public class CompareImage
    {
        public int ID { get; set; }
        public ThumbImage Image1 { get; set; }
        public ThumbImage Image2 { get; set; }
        public double? SSIM { get; set; }
    }
}