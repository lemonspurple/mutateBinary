using System;
using System.IO;
using System.Text;

/* 
                A 1. Point mutations: 
                                        GCA to 
                                        GCC

                B 2. Indels
                Frameshift mutations:   ATG-CGT-ACG to 
                                        ATC-GTA-CG

                C In frame insert / delete:   ATG-CGT-ACG to
                                            ATG-GGC-CGT-ACG 

                3. Structural chromeosome mutations
                D Duplications:   ABCDEF to 
                                ABCDEFDEF

                E Deletion:       ABCDEF to
                                ABEF

                F Inversion:      ABCDEF to
                                ABFEDC  
                G Translokation:  ABCDEF is translocated to a completely
                                different part

                Cycles: How often are the mutations applied
*/

namespace mutateBinary.Models.Functions
{
    public class MutateFuncs
    {
        float pointValue = default;
        float frameshiftValue = default;
        float frameInsertDeleteValue = default;
        float duplicationValue = default;
        float deletionValue = default;
        float inversionValue = default;
        float translocationValue = default;
        int cyclesValue = default;
        private static readonly Random rng = new Random();

        /* ---------------------------------------------------
                Constructor to draw values from UI
        --------------------------------------------------- */
        public MutateFuncs(float _pointValue, float _frameshiftValue, float _frameInsertDeleteValue, float _duplicationValue, float _deletionValue, float _inversionValue, float _translocationValue, int _cyclesValue)
        {
            pointValue = _pointValue;
            frameshiftValue = _frameshiftValue;
            frameInsertDeleteValue = _frameInsertDeleteValue;
            duplicationValue = _duplicationValue;
            deletionValue = _deletionValue;
            inversionValue = _inversionValue;
            translocationValue = _translocationValue;
            cyclesValue = _cyclesValue;
        }

        public void printMutateValuesToDebug()
        {

        }

        /* ---------------------------------------------------
                Main Method that collects all mutations
        --------------------------------------------------- */

        public void MutateDNAFile(string sourcePath, string outputPath)
        {
            string tempPath = sourcePath + ".tmp";

            for (int cycle = 0; cycle < cyclesValue; cycle++)
            {
                ApplyPerBaseMutations(sourcePath, tempPath);
                ApplyStructuralMutations(tempPath, sourcePath);
                File.Delete(tempPath);
            }

            File.Move(sourcePath, outputPath, overwrite: true);
        }

        /* ---------------------------------------------------
                        Per base mutations
        --------------------------------------------------- */

        /* Helpers */
        private static readonly char[] Bases = ['A', 'T', 'C', 'G'];
        private static char RandomBase() => Bases[rng.Next(4)];
        private static char RandomDifferentBase(char current)
        {
            char result;
            do { result = RandomBase(); } while (result == current);
            return result;
        }
        private static bool Roll(float probability) => rng.NextDouble() * 100.0 < probability;
        /* Point / Framshift */
        private void ApplyPerBaseMutations(string sourcePath, string targetPath)
        {
            using var sr = new StreamReader(sourcePath, Encoding.ASCII);
            using var sw = new StreamWriter(targetPath, false, Encoding.ASCII);

            int charInt;
            while ((charInt = sr.Read()) != -1)
            {
                char base_ = (char)charInt;

                // 1. Point Mutation
                if (Roll(pointValue))
                    base_ = RandomDifferentBase(base_);

                // 2. Frameshift
                if (Roll(frameshiftValue))
                {
                    if (rng.Next(2) == 0)
                        sw.Write(RandomBase());  // Inserts random base
                    else
                        continue;               // Skip current base
                }

                sw.Write(base_);
            }
        }

        private void ApplyInFrameInsertDelete(string sourcePath, string targetPath)
        {
            using var sr = new StreamReader(sourcePath, Encoding.ASCII);
            using var sw = new StreamWriter(targetPath, false, Encoding.ASCII);

            char[] codon = new char[3];
            int read;
            while ((read = sr.Read(codon, 0, 3)) > 0)
            {
                if (read == 3 && Roll(frameInsertDeleteValue))
                {
                    if (rng.Next(2) == 0)
                        sw.Write(new[] { RandomBase(), RandomBase(), RandomBase() }); // insert 3
                    else
                        continue; // delete codon
                }
                sw.Write(codon, 0, read); // always write the last incomplete chunk
            }
        }

        /* ---------------------------------------------------
                        Per structure mutations
        --------------------------------------------------- */

        /* Helpers */
        private (long start, int length) PickSegment(long fileLength)
        {
            int segLen = (int)Math.Max(4, fileLength * rng.Next(1, 11) / 100L);
            segLen = (segLen / 4) * 4;
            if (fileLength - segLen <= 0) return (0, (int)fileLength); // guards against division by 0
            long segStart = (rng.NextInt64(0, fileLength - segLen) / 4) * 4;
            return (segStart, segLen);
        }

        private static char Complement(char b) => b switch
        {
            'A' => 'T',
            'T' => 'A',
            'C' => 'G',
            'G' => 'C',
            _ => b
        };

        private void ApplyDeletion(string sourcePath, string targetPath)
        {
            if (!Roll(deletionValue)) { File.Copy(sourcePath, targetPath, true); return; }

            long fileLen = new FileInfo(sourcePath).Length;
            var (segStart, segLen) = PickSegment(fileLen);

            using var sr = new StreamReader(sourcePath, Encoding.ASCII);
            using var sw = new StreamWriter(targetPath, false, Encoding.ASCII);

            long pos = 0;
            int charInt;
            while ((charInt = sr.Read()) != -1)
            {
                if (pos < segStart || pos >= segStart + segLen)
                    sw.Write((char)charInt);
                pos++;
            }
        }

        private void ApplyDuplication(string sourcePath, string targetPath)
        {
            if (!Roll(duplicationValue)) { File.Copy(sourcePath, targetPath, true); return; }

            long fileLen = new FileInfo(sourcePath).Length;
            var (segStart, segLen) = PickSegment(fileLen);
            char[] segBuffer = new char[segLen]; // only loads segment to memory

            using var sr = new StreamReader(sourcePath, Encoding.ASCII);
            using var sw = new StreamWriter(targetPath, false, Encoding.ASCII);

            long pos = 0;
            int charInt;
            while ((charInt = sr.Read()) != -1)
            {
                char c = (char)charInt;
                if (pos >= segStart && pos < segStart + segLen)
                    segBuffer[pos - segStart] = c;

                sw.Write(c);

                // Insert after duplicate
                if (pos == segStart + segLen - 1)
                    sw.Write(segBuffer);

                pos++;
            }
        }


        private void ApplyInversion(string sourcePath, string targetPath)
        {
            if (!Roll(inversionValue)) { File.Copy(sourcePath, targetPath, true); return; }

            long fileLen = new FileInfo(sourcePath).Length;
            var (segStart, segLen) = PickSegment(fileLen);
            char[] segBuffer = new char[segLen];

            using var sr = new StreamReader(sourcePath, Encoding.ASCII);
            using var sw = new StreamWriter(targetPath, false, Encoding.ASCII);

            long pos = 0;
            int charInt;
            while ((charInt = sr.Read()) != -1)
            {
                char c = (char)charInt;
                if (pos >= segStart && pos < segStart + segLen)
                {
                    segBuffer[pos - segStart] = c;
                    if (pos == segStart + segLen - 1)
                    {
                        // Segment invert + complement
                        for (int i = segLen - 1; i >= 0; i--)
                            sw.Write(Complement(segBuffer[i]));
                    }
                }
                else
                {
                    sw.Write(c);
                }
                pos++;
            }
        }

        private void ApplyTranslocation(string sourcePath, string targetPath)
        {
            if (!Roll(translocationValue)) { File.Copy(sourcePath, targetPath, true); return; }

            long fileLen = new FileInfo(sourcePath).Length;
            if (fileLen < 8) { File.Copy(sourcePath, targetPath, true); return; }

            var (segStart, segLen) = PickSegment(fileLen);
            char[] segBuffer = new char[segLen];

            string tmpPath = sourcePath + ".tl";
            using (var sr = new StreamReader(sourcePath, Encoding.ASCII))
            using (var sw = new StreamWriter(tmpPath, false, Encoding.ASCII))
            {
                long pos = 0;
                int charInt;
                while ((charInt = sr.Read()) != -1)
                {
                    char c = (char)charInt;
                    if (pos >= segStart && pos < segStart + segLen)
                        segBuffer[pos - segStart] = c;
                    else
                        sw.Write(c);
                    pos++;
                }
            }

            long shortenedLen = fileLen - segLen;
            long insertPos = (rng.NextInt64(0, shortenedLen + 1) / 4) * 4;

            using (var sr = new StreamReader(tmpPath, Encoding.ASCII))
            using (var sw = new StreamWriter(targetPath, false, Encoding.ASCII))
            {
                long pos = 0;
                int charInt;
                while ((charInt = sr.Read()) != -1)
                {
                    if (pos == insertPos)
                        sw.Write(segBuffer); // Write before chat
                    sw.Write((char)charInt);
                    pos++;
                }
                if (insertPos >= shortenedLen) // Insert at end
                    sw.Write(segBuffer);
            }

            File.Delete(tmpPath);
        }

        private void ApplyStructuralMutations(string sourcePath, string targetPath)
        {
            string tmp1 = sourcePath + ".s1";
            string tmp2 = sourcePath + ".s2";

            ApplyInFrameInsertDelete(sourcePath, tmp1);
            ApplyDeletion(tmp1, tmp2); File.Delete(tmp1);
            ApplyDuplication(tmp2, tmp1); File.Delete(tmp2);
            ApplyInversion(tmp1, tmp2); File.Delete(tmp1);
            ApplyTranslocation(tmp2, targetPath); File.Delete(tmp2);
        }

    }


}