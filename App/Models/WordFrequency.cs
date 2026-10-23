namespace Document_Frequency_Counter.App.Models
{
    public class WordFrequency
    {
        public int Rank { get; set; }
        public string Word { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public string? Category { get; set; }
        public int TotalWords { get; set; }
        public double Percentage
        {
            get => TotalWords == 0 ? 0 : (double)Frequency / TotalWords * 100;
        }

        public string DisplayRank() => (Rank <= 3 && Rank >= 1) ? $"👑 {Rank}" : $"# {Rank}";

        public Color RankColor() => Rank switch
        {
            _ when Rank >= 1 && Rank <= 3 => Color.FromArgb(243, 181, 75),
            _ => Color.Black
        };

        public string GetCategory() => Category switch
        {
            _ when Frequency > 500 => "🔥 Very Common",
            _ when Frequency <= 500 && Frequency > 300 => "📈 Common",
            _ when Frequency <= 300 && Frequency > 0 => "📊 Moderate",
            _ => ""
        };

        public Color CategoryColor() => GetCategory() switch
        {
            "🔥 Very Common" => Color.FromArgb(220, 38, 38),
            "📈 Common" => Color.FromArgb(245, 158, 11),
            "📊 Moderate" => Color.FromArgb(59, 130, 246),
            _ => Color.FromArgb(156, 163, 175)
        };

        public string PercentageBar() => $"{Percentage:F2}%";

        public string GetBar()
        {
            var nearestNumber = Math.Round(Percentage / 5.0) * 5.0;
            int blockCount = Math.Clamp((int)nearestNumber / 5, 0, 20);
            return new string('█', blockCount);
        }

        public override string ToString() => $"{Rank},{Word},{Frequency},{PercentageBar()},{GetBar()},{GetCategory()}";
    }
}