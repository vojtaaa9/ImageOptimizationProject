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
            ThumbImage[] thumbEntities = new ThumbImage[fileEntries.Length];

            int i = 0;
            foreach (string path in fileEntries)
            {

                var image = new SourceImage()
                {
                    ID = i+1,
                    AbsolutePath = path,
                    FileName = Path.GetFileName(path),
                    RelativePath = "/images/" + Path.GetFileName(path),
                    AltText = Path.GetFileNameWithoutExtension(path),
                    Thumbnails = new List<ThumbImage>(),
                    Formats = new List<ThumbImage>(),
                    Metadata= new List<ThumbImage>(),
                    Compression = new List<ThumbImage>(),
                    Format = FileService.ParseFileFormat(Path.GetExtension(path)),
                    FileSize = new FileInfo(path).Length
                };

                if (image.Format == Format.SVG)
                {
                    var minPath = absoluteDir + "/out/" + Path.GetFileNameWithoutExtension(path) + ".min.svg";

                    var optimized = new ThumbImage()
                    {
                        FileName = Path.GetFileName(minPath),
                        AltText = image.AltText,
                        SourceID = image.ID,
                        RelativePath = "/images/out/" + Path.GetFileName(minPath),
                        AbsolutePath = minPath,
                        Format = Format.SVG,
                        FileSize = new FileInfo(minPath).Length,
                        Stripped = false
                    };

                    image.Thumbnails.Add(optimized);
                    thumbEntities.SetValue(optimized, i);
                    
                }

                imageEntities.SetValue(image, i);
                i++;
            }

            // Wrap everything in transaction
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    context.SourceImages.AddOrUpdate(imageEntities);
                    context.ThumbImages.AddOrUpdate(thumbEntities);
                    context.SaveChanges();

                    transaction.Commit();

                    Console.WriteLine("Seed succesfull.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message, e.InnerException);
                    transaction.Rollback();
                }
            }

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
