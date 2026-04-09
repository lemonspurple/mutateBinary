using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace deepMutate.Models.Data
{
    public class FileManager
    {
        public List<string> GetFilesInFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    return new List<string>();

                return Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly).ToList();
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Fehler: {ex.Message}");
                return new List<string>();
            }
        }
    }
}
