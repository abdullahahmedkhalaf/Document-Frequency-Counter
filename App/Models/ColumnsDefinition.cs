namespace Document_Frequency_Counter.App.Models
{
    public enum ColumnAlignment
    {
        Left,
        Center,
        Right
    }

    public class ColumnsDefinition
    {
        public string Text { get; set; } = string.Empty;
        public int Width { get; set; }
        public int XPosition { get; set; }
        public ColumnAlignment ColumnAlignment { get; set; } = ColumnAlignment.Left;
        public bool Sortable { get; set; } = false;

        public Font HeaderFont { get; set; } = new Font("Segoe UI", 10, FontStyle.Bold);
        public Font CellFont { get; set; } = new Font("Segoe UI", 10, FontStyle.Bold);
        public Font CellFontSpecial { get; set; } = new Font("Consolas", 10, FontStyle.Bold);

        public Color HeaderForeColor { get; set; } = Color.White;
        public Color CellForeColor { get; set;} = Color.Black;
        public Color CellForeColorSpecial { get; set;} = Color.FromArgb(37, 99, 235);
    }
}