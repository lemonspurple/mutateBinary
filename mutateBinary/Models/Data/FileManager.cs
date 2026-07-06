using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using mutateBinary.Models.Functions;

namespace mutateBinary.Models.Data
{
    public class FileManager
    {
        /*
        #### #### #### ####
         Directory Management
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

        public void ConvertFileToDNA(string sourcePath, string targetDirectory, DnaMapping? mapping = null)
        {
            try
            {
                // generate output filename: originalname.dna
                string fileName = Path.GetFileName(sourcePath) + ".dna";
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

                        sw.Write(FileToDNAMapper((b >> 6) & 0b11, mapping));
                        sw.Write(FileToDNAMapper((b >> 4) & 0b11, mapping));
                        sw.Write(FileToDNAMapper((b >> 2) & 0b11, mapping));
                        sw.Write(FileToDNAMapper(b & 0b11, mapping));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encoding {sourcePath}: {ex.Message}");
            }
        }

        // Mapping Bits to DNA   A: 00, T: 11, C: 01, G: 10
        private static char FileToDNAMapper(int twoBits, DnaMapping? mapping) => twoBits switch
        {
            0b00 => mapping?.Map00 ?? 'A',
            0b01 => mapping?.Map01 ?? 'C',
            0b10 => mapping?.Map10 ?? 'G',
            0b11 => mapping?.Map11 ?? 'T',
            _ => throw new NotImplementedException("Not a bit")
        };

        // #### Decoding

        public void ConvertDNAToFile(string sourcePath, string targetDirectory, DnaMapping? mapping = null)
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
                            (DNAToFileMapper(buffer[0], mapping) << 6) |
                            (DNAToFileMapper(buffer[1], mapping) << 4) |
                            (DNAToFileMapper(buffer[2], mapping) << 2) |
                            DNAToFileMapper(buffer[3], mapping);

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
        private static int DNAToFileMapper(char dna, DnaMapping? mapping)
        {
            if (mapping != null)
            {
                if (dna == mapping.Map00) return 0b00;
                if (dna == mapping.Map01) return 0b01;
                if (dna == mapping.Map10) return 0b10;
                if (dna == mapping.Map11) return 0b11;
                throw new NotImplementedException($"No valid DNA input for custom mapping: {dna}");
            }
            return Char.ToUpper(dna) switch
            {
                'A' => 0b00,
                'T' => 0b11,
                'C' => 0b01,
                'G' => 0b10,
                _ => throw new NotImplementedException("No valid DNA input.")
            };
        }

    }
}