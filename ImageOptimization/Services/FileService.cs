using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ImageOptimization.Services
{
    public class FileService
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public FileService()
        {

        }

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

    }
}