using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace deepMutate.Models.Data
{
    public class Mutate
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

        public void ConvertFileToBinaryText(string sourcePath, string targetPath)
        {
            // opening source file
            using (FileStream fs = File.OpenRead(sourcePath))
            using (StreamWriter sw = new StreamWriter(targetPath, false, Encoding.UTF8))
            {
                int b;
                // reading byte by byte
                while ((b = fs.ReadByte()) != -1)
                {
                    /* mini magic trick: b,2 warrants that 5 will be converted into 101 i.E.
                    however, 255 would be 11111111, which means that if it follows to a 5, no
                    one clearly knows, if 101 is part of an own value or belongs to the next
                    .PadLeft therefore puts it into 8 bit lengths */
                    string binaryString = Convert.ToString(b, 2).PadLeft(8, '0');
                    sw.Write(binaryString);
                }
            }
        }


                public void ConvertFileToDNA(string sourcePath, string targetPath)
        {
            // opening source file
            using (FileStream fs = File.OpenRead(sourcePath))
            using (StreamWriter sw = new StreamWriter(targetPath, false, Encoding.UTF8))
            {

            }
        }
    }
}
