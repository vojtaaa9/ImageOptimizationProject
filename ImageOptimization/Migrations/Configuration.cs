namespace ImageOptimization.Migrations
{
    using ImageOptimization.Models;
    using ImageOptimization.Services;
    using System;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Reflection;
    using System.Collections.Generic;
    using ImageOptimization.Enums;

    internal sealed class Configuration : DbMigrationsConfiguration<DataPersistenceLayer.ImageContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(DataPersistenceLayer.ImageContext context)
        {
            String absoluteDir = MapPath("images");
            string[] fileEntries = FileService.GetAllFilesInDir(absoluteDir);
            
            SourceImage[] imageEntities = new SourceImage[fileEntries.Length];

            int i = 0;
            foreach (string path in fileEntries)
            {

                var image = new SourceImage()
                {
                    AbsolutePath = path,
                    FileName = Path.GetFileName(path),
                    RelativePath = "/images/" + Path.GetFileName(path),
                    AltText = Path.GetFileNameWithoutExtension(path),
                    Thumbnails = new List<ThumbImage>(),
                    Format = FileService.ParseFileFormat(Path.GetExtension(path))
                };

                imageEntities.SetValue(image, i);
                i++;
            }

            context.SourceImages.AddOrUpdate(imageEntities);
            context.SaveChanges();

            // Create Directory for thumbnails
            Directory.CreateDirectory(MapPath("thumbnails"));
        }

        /// <summary>
        /// Manual mapping of absolute path.
        /// </summary>
        /// <param name="seedFile"></param>
        /// <returns>Returns absolute path to project's directory and includes passed folder</returns>
        private string MapPath(string folder)
        {
            var absolutePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            var path = absolutePath.Remove(absolutePath.Length - "bin/ImageOptimization.DLL".Length); 

            return path + folder;
        }
    }
}
