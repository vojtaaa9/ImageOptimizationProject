using System.Data.Entity;
using ImageOptimization.Models;

namespace ImageOptimization.DataPersistenceLayer
{
    public class ImageContext : DbContext
    {
        /// <summary>
        /// Initialiser for ImageOptimization Database. Automatic Migrations are enabled.
        /// </summary>
        public ImageContext() : base("ImageOptimizationDb")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ImageContext>());
        }

        public DbSet<SourceImage> SourceImages { get; set; }
        public DbSet<ThumbImage> ThumbImages { get; set; }
        public DbSet<CompareImage> CompareImages { get; set; }
    }
}