using ImageOptimization.Enums;
using System;
using System.IO;

namespace ImageOptimization.Services
{
    public class FileService
    {
        /// <summary>
        /// Returns all files 
        /// </summary>
        /// <param name="dir">Path to the directory</param>
        /// <returns></returns>
        public static string[] GetAllFilesInDir(String dir)
        {
            // if directory does not exists, return null
            if (!Directory.Exists(dir))
                return null;

             return Directory.GetFiles(dir);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns>Whether a file was created or not</returns>
        public static bool CreateFile(String filepath)
        {
            // If the file already exists, don't create new one
            if (!File.Exists(filepath))
            {
                var file = File.Create(filepath);
                file.Close();
                return true;
            }

            return false;
        }

        public static String CombineDirectoryAndFilename(String path, String filename)
        {
            return $"{path}\\{filename}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileEnding"></param>
        /// <returns></returns>
        public static Format ParseFileFormat(string fileEnding)
        {
            fileEnding = fileEnding.ToLower();

            switch (fileEnding)
            {
                case ".svg":
                    return Format.SVG;
                case ".jpg":
                case ".jpeg":
                    return Format.JPEG;
                case ".png":
                    return Format.PNG;
                case ".webp":
                    return Format.WebP;
                case ".gif":
                    return Format.GIF;
                default:
                    return Format.Unknown;
            }
        }
    }
}