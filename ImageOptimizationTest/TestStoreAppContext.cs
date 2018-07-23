using ImageOptimization.DataPersistenceLayer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageOptimization.Models;

namespace ImageOptimizationTest
{
    class TestStoreAppContext : IStoreAppContext
    {
        public TestStoreAppContext()
        {
            SourceImages = new TestSourceImageDbSet();
        }

        public DbSet<SourceImage> SourceImages { get; }
        public DbSet<ThumbImage> ThumbImages { get; }
        public DbSet<CompareImage> CompareImages { get; }

        public int SaveChanges()
        {
            return 0;
        }

        public void MarkAsModified(SourceImage item) { }

        public bool IsModified(SourceImage item)
        {
            return false; 
        }

        public void Dispose()
        {
            
        }
    }
}
