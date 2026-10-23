namespace Document_Frequency_Counter.App.Models
{
    public class NGramsData
    {
        public int Rank { get; set; }
        public string Phrase { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public int TotalNGrams { get; set; }
        public double Percentage
        {
            get => TotalNGrams == 0 ? 0 : (double)Frequency / TotalNGrams * 100;
        }

        public string DisplayRank() => (Rank <= 3 && Rank >= 1) ? $"👑 {Rank}" : $"# {Rank}";

        public Color RankColor() => Rank switch
        {
            _ when Rank >= 1 && Rank <= 3 => Color.FromArgb(243, 181, 75),
            _ => Color.Black
        };

        public string PercentageBar() => $"{Percentage:F2}%";

        public string GetBar()
        {
            var nearestNumber = Math.Round(Percentage / 5.0) * 5.0;
            int blockCount = Math.Clamp((int)nearestNumber / 5, 0, 20);
            return new string('█', blockCount);
        }

        public override string ToString() => $"{Rank},{Phrase},{Frequency},{PercentageBar()}%,{GetBar()}";
    }
}