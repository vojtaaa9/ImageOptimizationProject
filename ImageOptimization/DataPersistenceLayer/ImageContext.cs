using System.Data.Entity;
using ImageOptimization.Models;

namespace ImageOptimization.DataPersistenceLayer
{
    public class ImageContext : DbContext, IStoreAppContext
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
        public void MarkAsModified(SourceImage item)
        {
            Entry(item).State = EntityState.Modified;
        }

        public bool IsModified(SourceImage item)
        {
            return Entry(item).State == EntityState.Modified;
        }
    }
}