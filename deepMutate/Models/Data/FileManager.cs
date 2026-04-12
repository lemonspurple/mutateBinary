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

                        sw.Write(FileToDNAMapper((b >> 6) & 0b11));
                        sw.Write(FileToDNAMapper((b >> 4) & 0b11));
                        sw.Write(FileToDNAMapper((b >> 2) & 0b11));
                        sw.Write(FileToDNAMapper(b & 0b11));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encoding {sourcePath}: {ex.Message}");
            }
        }

        // Mapping Bits to DNA   A: 00, T: 11, C: 01, G: 10
        private static char FileToDNAMapper(int twoBits) => twoBits switch
        {
            0b00 => 'A',
            0b11 => 'T',
            0b01 => 'C',
            0b10 => 'G',
            _ => throw new NotImplementedException("Not a bit")
        };

        // #### Decoding

        public void ConvertDNAToFile(string sourcePath, string targetDirectory)
        {

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(sourcePath);
                string targetFilePath = Path.Combine(targetDirectory, fileName);

                using (StreamReader sr = new StreamReader(sourcePath, Encoding.UTF8))
                using (FileStream fsTarget = File.Create(targetFilePath))
                {
                    char[] buffer = new char[4];
                    int charsRead;

                    // Reads 4 chars per piece
                    while ((charsRead = sr.Read(buffer, 0, 4)) == 4)
                    {

                        // every base delivers two bits back
                        int reconstructedByte =
                            (DNAToFileMapper(buffer[0]) << 6) |
                            (DNAToFileMapper(buffer[1]) << 4) |
                            (DNAToFileMapper(buffer[2]) << 2) |
                            DNAToFileMapper(buffer[3]);

                        fsTarget.WriteByte((byte)reconstructedByte);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decoding {sourcePath}: {ex.Message}");
            }
        }

        // Reverse Mapping DNA to Bits   00: A, 11: T, 01: C, 10: G
        private static int DNAToFileMapper(char DNA) => DNA switch
        {
            'A' => 0b00,
            'T' => 0b11,
            'C' => 0b01,
            'G' => 0b10,
            _ => throw new NotImplementedException("No valid DNA input. Must be either (A,T,C or G)")
        };

    }
}
