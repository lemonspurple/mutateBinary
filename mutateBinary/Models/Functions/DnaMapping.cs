namespace mutateBinary.Models.Functions
{
    public class DnaMapping
    {
        public char Map00 { get; set; } = 'A';
        public char Map01 { get; set; } = 'C';
        public char Map10 { get; set; } = 'G';
        public char Map11 { get; set; } = 'T';

        public bool IsDefault =>
            Map00 == 'A' && Map01 == 'C' && Map10 == 'G' && Map11 == 'T';

        // Only add suffix, if the mapping ins't standard: "_mpACGT"
        public string ToSuffix() =>
            IsDefault ? "" : $"_mp{Map00}{Map01}{Map10}{Map11}";
    }
}