using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace deepMutate.Models.Data
{
    public class FileManager
    {
        /*
        #### #### #### ####
         General UI Stuff
        #### #### #### ####
        */

        // Select Folder Function
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

        /*
        #### #### #### ####
           DNA Conversion
        #### #### #### ####
        */


        // ### Encoding

        public void ConvertFileToDNA(string sourcePath, string targetDirectory)
        {
            try
            {
                // generate output filename: originalname.dna
                string fileName = Path.GetFileNameWithoutExtension(sourcePath) + ".dna";
                string targetFilePath = Path.Combine(targetDirectory, fileName);

                using (FileStream fs = File.OpenRead(sourcePath))
                using (StreamWriter sw = new StreamWriter(targetFilePath, false, Encoding.UTF8))
                {
                    int b;
                    while ((b = fs.ReadByte()) != -1)
                    {
                        //  Bit Manipulation
                        //  Dividing Byte (8 bits) into 4 pairs of 2 bits.
                        //  1 Byte = 4 DNA signs

                        // THe right shift opperator (>>) is used to support the filestream approach
                        // It would work otherwise too, but require the entire file to be processed in ram
                        // instead of cpu. It is nondestructive

                        sw.Write(DNAMapper((b >> 6) & 0b11));
                        sw.Write(DNAMapper((b >> 4) & 0b11));
                        sw.Write(DNAMapper((b >> 2) & 0b11));
                        sw.Write(DNAMapper(b & 0b11));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encoding {sourcePath}: {ex.Message}");
            }
        }

        // Mapping Bits to DNA   A: 00, T: 11, C: 01, G: 10
        private static char DNAMapper(int twoBits) => twoBits switch
        {
            0b00 => 'A',
            0b11 => 'T',
            0b01 => 'C',
            0b10 => 'G',
            _ => throw new NotImplementedException("Not a bit")
        };

        // #### Decoding

        //TODO

    }
}
