namespace Document_Frequency_Counter.App.Models
{
    public class TableBuilder
    {
        private readonly RoundPanel _container;
        private readonly ColumnsDefinition[] _columns;
        private readonly Color _headerColor = Color.FromArgb(52, 87, 213);
        private readonly Color _rowColor1 = Color.White;
        private readonly Color _rowColor2 = Color.FromArgb(249, 250, 252);

        private int _currentY = 65;
        private bool _skipHeader;

        private List<RoundPanel> _dataRows = [];
        private RoundPanel? _headerBar;

        public EventHandler<int>? ColumnHeaderClicked;
        public EventHandler<object>? RowClicked;

        public TableBuilder(Panel container, ColumnsDefinition[] columns, bool skipHeader = false)
        {
            _container = container as RoundPanel ?? throw new ArgumentException("Container must be RoundPanel");
            _columns = columns;
            _skipHeader = skipHeader;
            _currentY = _skipHeader ? 0 : 65;

            if (!skipHeader) BuildHeader();
        }

        public void BuildHeader()
        {
            _currentY = 65;

            _headerBar = new()
            {
                FillColor = _headerColor,
                BorderColor = Color.Transparent,
                BorderWidth = 1,
                BorderRadius = 25,
                Size = new Size(_container.Width - 1, 50),
                Location = new Point(1, 1)
            };

            for (int i = 0; i < _columns.Length; i++)
            {
                Label headerLabel = new()
                {
                    Text = _columns[i].Text,
                    Location = new Point(_columns[i].XPosition, 12),
                    Size = new Size(_columns[i].Width, 26),
                    Font = _columns[i].HeaderFont,
                    ForeColor = _columns[i].HeaderForeColor,
                    TextAlign = GetContentAlignment(_columns[i].ColumnAlignment),
                    BackColor = Color.Transparent,
                    Cursor = _columns[i].Sortable ? Cursors.Hand : Cursors.Default,
                    Tag = i
                };

                int columnIndex = i;
                headerLabel.MouseEnter += (s, e) => ((Label)s!).ForeColor = Color.FromArgb(255, 220, 100);
                headerLabel.MouseLeave += (s, e) => ((Label)s!).ForeColor = _columns[columnIndex].HeaderForeColor;
                headerLabel.Click += (s, e) => ColumnHeaderClicked?.Invoke(this, (int)((Label)s!).Tag!);

                _headerBar.Controls.Add(headerLabel);
            }
            _container.Controls.Add(_headerBar);
        }

        public RoundPanel AddWordFrequencyRow(WordFrequency word)
        {
            int rowIndex = _dataRows.Count;
            Color rowColor = (rowIndex % 2 == 0) ? _rowColor1 : _rowColor2;

            RoundPanel rowPanel = new()
            {
                FillColor = rowColor,
                BorderColor = Color.FromArgb(230, 231, 235),
                BorderWidth = 1,
                BorderRadius = 5,
                Size = new Size(_container.Width - 40, 40),
                Location = new Point(10, _currentY),
                Cursor = Cursors.Hand,
                Tag = word
            };

            Color originalColor = rowColor;
            rowPanel.Click += (s, e) => RowClicked!.Invoke(this, ((RoundPanel)s!).Tag!);

            //Rank
            AddCell(rowPanel,
                    word.DisplayRank(),
                    _columns[0],
                    "Rank",
                    word.RankColor(),
                    new Font("Segoe UI", 9, FontStyle.Bold)
            );

            //Word
            AddCell(rowPanel,
                    word.Word,
                    _columns[1],
                    "Word",
                    _columns[1].CellForeColorSpecial,
                    _columns[1].CellFontSpecial
            );

            //Frequency
            AddCell(rowPanel,
                    word.Frequency.ToString(),
                    _columns[2],
                    "Frequency"
            );

            //Percentage
            AddCell(rowPanel,
                    word.PercentageBar(),
                    _columns[3],
                    "Percentage"
            );

            //Distribution
            AddCell(rowPanel,
                    word.GetBar(),
                    _columns[4],
                    "Distribution",
                    Color.Black,
                    new Font("MonoSpace", 9, FontStyle.Bold),
                    ColumnAlignment.Left
            );

            //Category
            AddCategoryBadge(rowPanel, word);

            foreach (Control control in rowPanel.Controls)
            {
                control.Click += (s, e) =>
                {
                    // Walk up to find the parent RoundPanel that has the Tag
                    var parent = ((Control)s!).Parent as RoundPanel;
                    RowClicked?.Invoke(this, parent!.Tag!);
                };
            }

            _container.Controls.Add(rowPanel);
            _dataRows.Add(rowPanel);
            _currentY += 45;

            return rowPanel;
        }

        private static ContentAlignment GetContentAlignment(ColumnAlignment columnAlignment) => columnAlignment switch
        {
            ColumnAlignment.Left => ContentAlignment.MiddleLeft,
            ColumnAlignment.Center => ContentAlignment.MiddleCenter,
            ColumnAlignment.Right => ContentAlignment.MiddleRight,
            _ => ContentAlignment.MiddleLeft,
        };

        private static void AddCell(RoundPanel row, string text, ColumnsDefinition col, string tag, Color? foreColor = null, Font? font = null, ColumnAlignment? columnAlignment = null)
        {
            Label cell = new()
            {
                Text = text,
                Location = new Point(col.XPosition - 10, 10),
                Size = new Size(col.Width, 20),
                TextAlign = GetContentAlignment(columnAlignment ?? col.ColumnAlignment),
                Font = font ?? col.CellFont,
                ForeColor = foreColor ?? col.CellForeColor,
                BackColor = Color.Transparent,
                Tag = tag
            };

            row.Controls.Add(cell);
        }

        private void AddCategoryBadge(RoundPanel row, WordFrequency word)
        {
            RoundPanel badge = new()
            {
                FillColor = word.CategoryColor(),
                BorderColor = Color.Transparent,
                BorderRadius = 12,
                Size = new Size(140, 26),
                Location = new Point(_columns[5].XPosition - 10, 7),
                Tag = "CategoryBadge"
            };

            Label badgeText = new()
            {
                Text = word.GetCategory(),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            badge.Controls.Add(badgeText);
            row.Controls.Add(badge);
        }

        public void ClearRows()
        {
            foreach (var row in _dataRows)
            {
                _container.Controls.Remove(row);
                row.Dispose();
            }

            _dataRows.Clear();
            _currentY = _skipHeader ? 0 : 65;
        }
    }
}