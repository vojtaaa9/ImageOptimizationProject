namespace ImageOptimization.Migrations
{
    using ImageOptimization.Models;
    using ImageOptimization.Services;
    using System;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;
    using System.Collections.Generic;

    internal sealed class Configuration : DbMigrationsConfiguration<DataPersistenceLayer.ImageContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(DataPersistenceLayer.ImageContext context)
        {
            String absoluteDir = MapPath("~/images");
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
                    Thumbnails = new List<ThumbImage>()
                };

                imageEntities.SetValue(image, i);
                i++;
            }

            context.SourceImages.AddOrUpdate(imageEntities);
            context.SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seedFile"></param>
        /// <returns></returns>
        private string MapPath(string seedFile)
        {
            if (HttpContext.Current != null)
                return HostingEnvironment.MapPath(seedFile);

            var absolutePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            var path = absolutePath.Remove(absolutePath.Length - "bin/ImageOptimization.DLL".Length); 

            return path + "images";
        }
    }
}
