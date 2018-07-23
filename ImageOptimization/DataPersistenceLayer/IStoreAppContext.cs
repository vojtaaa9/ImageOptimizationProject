using System;
using System.Data.Entity;
using ImageOptimization.Models;

namespace ImageOptimization.DataPersistenceLayer
{
    public interface IStoreAppContext :IDisposable
    {
        DbSet<SourceImage> SourceImages { get; }
        DbSet<ThumbImage> ThumbImages { get; }
        DbSet<CompareImage> CompareImages { get; }
        int SaveChanges();
        void MarkAsModified(SourceImage item);
        bool IsModified(SourceImage item);
    }
}