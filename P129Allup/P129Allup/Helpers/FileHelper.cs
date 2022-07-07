using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace P129Allup.Helpers
{
    public class FileHelper
    {
        public static void DeleteFile(IWebHostEnvironment env, string filename, params string[] folders)
        {
            string path = Path.Combine(env.WebRootPath);

            foreach (string folder in folders)
            {
                path = Path.Combine(path, folder);
            }

            path = Path.Combine(path, filename);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
