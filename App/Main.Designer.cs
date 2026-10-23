using System.Drawing.Drawing2D;
using Document_Frequency_Counter.App.Models;

namespace Document_Frequency_Counter;

partial class Main
{
    private System.ComponentModel.IContainer components = null;

    private RoundPanel pnlImport, pnlAnalysisTableContainer, pnlAnalysisButton, pnlClearButton, pnlCancelButton, pnlExportButton, pnlExportNGramsButton, pnlAnalysisTableBody, pnlFileCount;
    private Panel pnlAnalysis;
    private Label lblImportTitle, lblAnalysisButton, lblClearButton, lblCancelButton, lblExportButton, lblExportNGramsButton, lblFileCount;

    private TableBuilder _analysisTable;

    private List<string> _list = [];
    private string _sortColumn = "Frequency";
    private bool _sortAsc = false;
    private string[][] tokensArray = [];

    private List<WordFrequency> _wordFrequencies = new();
    private List<WordFrequency> _wordFrequenciesFull = new();
    private List<NGramsData> _nGramDataFull = new();

    private const int ANIMATION_STEPS = 20;

    private CancellationTokenSource _cts;

    private readonly Color DROP_ZONE_COLOR = Color.FromArgb(229, 235, 251);
    private readonly Color DROP_ZONE_ACTIVE_COLOR = Color.FromArgb(52, 87, 213);
    private readonly Color DROP_ZONE_TEXT_COLOR = Color.FromArgb(52, 87, 213);

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        ClientSize = new Size(1280, 720);
        Text = "Document Frequency Counter";
        DoubleBuffered = true;
    }

    private void SetupComponents()
    {
        //-----------Designs-----------

        //Analysis Pane
        pnlAnalysis = new()
        {
            BackColor = this.BackColor,
            BorderStyle = BorderStyle.None,
            Size = this.ClientSize,
            Location = this.Location
        };
        Controls.Add(pnlAnalysis);

        //Drop Panel
        pnlImport = new RoundPanel();
        pnlImport.FillColor = DROP_ZONE_COLOR;
        pnlImport.BorderColor = Color.Transparent;
        pnlImport.BorderRadius = 25;
        pnlImport.Size = new Size(pnlAnalysis.ClientSize.Width - 50, 100);
        pnlImport.Location = new Point((pnlAnalysis.ClientSize.Width - pnlImport.Width) / 2, 10);
        pnlImport.AllowDrop = true;
        pnlImport.Cursor = Cursors.Hand;
        pnlAnalysis.Controls.Add(pnlImport);

        lblImportTitle = new Label();
        lblImportTitle.Text = "Drop your files here or click to browse";
        lblImportTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        lblImportTitle.TextAlign = ContentAlignment.MiddleCenter;
        lblImportTitle.Dock = DockStyle.Fill;
        lblImportTitle.BackColor = Color.Transparent;
        lblImportTitle.ForeColor = DROP_ZONE_TEXT_COLOR;
        pnlImport.Controls.Add(lblImportTitle);

        //Buttons
        CreateButton("Analysis", -300, ref pnlAnalysisButton, ref lblAnalysisButton);
        CreateButton("Clear", -150, ref pnlClearButton, ref lblClearButton);
        CreateButton("Cancel", 0, ref pnlCancelButton, ref lblCancelButton);
        CreateButton("Export", 150, ref pnlExportButton, ref lblExportButton);
        CreateButton("Export NGrams", 300, ref pnlExportNGramsButton, ref lblExportNGramsButton);

        //Analysis Table
        CreateAnalysisTable();

        //File Count
        pnlFileCount = new RoundPanel();
        pnlFileCount.FillColor = Color.FromArgb(249, 250, 252);
        pnlFileCount.BorderColor = Color.Transparent;
        pnlFileCount.BorderRadius = 25;
        pnlFileCount.Size = new Size(200, 25);
        pnlFileCount.Location = new Point((pnlAnalysis.ClientSize.Width - pnlFileCount.Width) / 2, 180);
        pnlAnalysis.Controls.Add(pnlFileCount);

        lblFileCount = new Label
        {
            Text = "📄 No files loaded",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ForeColor = Color.Gray
        };
        pnlFileCount.Controls.Add(lblFileCount);

        //-----------Events-----------

        //Drop Panel Events
        pnlImport.DragEnter += (s, e) =>
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;

                //Visual Effects
                pnlImport.FillColor = DROP_ZONE_ACTIVE_COLOR;
                lblImportTitle.Text = "Release to add files!";
                lblImportTitle.ForeColor = Color.White;
            }
        };
        pnlImport.DragLeave += (s, e) => ResetDropPanel();
        pnlImport.DragDrop += (s, e) =>
        {
            ResetDropPanel();
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ProcessesDroppedFiles(files);
            }
        };

        lblImportTitle.MouseEnter += (s, e) =>
        {
            pnlImport.FillColor = DROP_ZONE_ACTIVE_COLOR;
            lblImportTitle.ForeColor = Color.White;

        };
        lblImportTitle.MouseLeave += (s, e) => ResetDropPanel();
        lblImportTitle.Click += (s, e) => OpenFileBrowser();

        //Buttons Events
        ButtonStyleEffects(pnlAnalysisButton, lblAnalysisButton);
        ButtonStyleEffects(pnlClearButton, lblClearButton);
        ButtonStyleEffects(pnlCancelButton, lblCancelButton);
        ButtonStyleEffects(pnlExportButton, lblExportButton);
        ButtonStyleEffects(pnlExportNGramsButton, lblExportNGramsButton);

        lblAnalysisButton.Click += async (s, e) => await Analysis(s, e);
        lblClearButton.Click += (s, e) =>
        {
            pnlAnalysisTableContainer.SuspendLayout();
            _analysisTable.ClearRows();
            pnlAnalysisTableContainer.ResumeLayout();

            _list.Clear();
            _wordFrequencies.Clear();
            _wordFrequenciesFull.Clear();
            lblFileCount.Text = "📄 No files loaded";

            pnlAnalysisButton.Enabled = false;
            pnlClearButton.Enabled = false;
            pnlExportButton.Enabled = false;
            pnlExportNGramsButton.Enabled = false;
        };
        lblCancelButton.Click += (s, e) =>
        {
            _cts?.Cancel();
            lblCancelButton.Text = "Cancelling...";
        };
        lblExportButton.Click += async (s, e) => await Export(s, e);
        lblExportNGramsButton.Click += async (s, e) => await ExportNGrams(s, e);
    }

    private void ResetDropPanel()
    {
        pnlImport.FillColor = DROP_ZONE_COLOR;
        lblImportTitle.Text = "Drop your files here or click to browse";
        lblImportTitle.ForeColor = DROP_ZONE_TEXT_COLOR;
    }

    private void CreateButton(string title, int offset, ref RoundPanel roundPanel, ref Label label, bool enabled = false)
    {
        //Button Panel
        roundPanel = new RoundPanel();
        roundPanel.FillColor = Color.FromArgb(249, 250, 252);
        roundPanel.BorderColor = Color.FromArgb(230, 231, 235);
        roundPanel.BorderWidth = 1;
        roundPanel.BorderRadius = 50;
        roundPanel.Size = new Size(100, 50);
        roundPanel.Location = new Point((pnlAnalysis.ClientSize.Width - roundPanel.Width) / 2 + offset, pnlImport.Height + 20);
        roundPanel.Enabled = enabled;
        roundPanel.Cursor = Cursors.Hand;
        pnlAnalysis.Controls.Add(roundPanel);

        //Button Label
        label = new Label();
        label.Text = title;
        label.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.BackColor = Color.Transparent;
        label.ForeColor = Color.FromArgb(17, 17, 17);
        label.Dock = DockStyle.Fill;
        roundPanel.Controls.Add(label);
    }

    private static void ButtonStyleEffects(RoundPanel roundPanel, Label label)
    {
        label.MouseEnter += (s, e) =>
        {
            roundPanel.FillColor = Color.FromArgb(87, 116, 220);
            label.ForeColor = Color.White;
        };
        label.MouseLeave += (s, e) =>
        {
            roundPanel.FillColor = Color.FromArgb(249, 250, 252);
            label.ForeColor = Color.FromArgb(17, 17, 17);
        };
        label.MouseDown += (s, e) =>
        {
            roundPanel.FillColor = Color.FromArgb(58, 86, 185);
            label.ForeColor = Color.FromArgb(197, 197, 197);
        };
        label.MouseUp += (s, e) =>
        {
            roundPanel.FillColor = Color.FromArgb(87, 116, 220);
            label.ForeColor = Color.White;
        };
    }

    private void CreateAnalysisTable()
    {
        //Container
        pnlAnalysisTableContainer = new RoundPanel();
        pnlAnalysisTableContainer.FillColor = Color.FromArgb(249, 250, 252); //Main
        // pnlAnalysisTableContainer.FillColor = Color.FromArgb(209, 210, 215); //Debug -> More Clear
        pnlAnalysisTableContainer.BorderColor = Color.Transparent;
        pnlAnalysisTableContainer.BorderRadius = 25;
        pnlAnalysisTableContainer.Size = new Size((pnlAnalysis.ClientSize.Width - 50), 500);
        pnlAnalysisTableContainer.Location = new Point((pnlAnalysis.ClientSize.Width - pnlAnalysisTableContainer.Width) / 2, (pnlAnalysis.ClientSize.Height - pnlAnalysisTableContainer.Height) / 2 + 100);
        pnlAnalysisTableContainer.Padding = new Padding(5);
        pnlAnalysis.Controls.Add(pnlAnalysisTableContainer);

        RoundPanel headerPanel = new()
        {
            FillColor = Color.FromArgb(52, 87, 213),
            BorderColor = Color.Transparent,
            BorderRadius = 20,
            Height = 50,
            Width = pnlAnalysisTableContainer.Width,
            Dock = DockStyle.Top,
        };
        pnlAnalysisTableContainer.Controls.Add(headerPanel);

        ColumnsDefinition[] columns = [
            new ColumnsDefinition { Text = "Rank", XPosition = 30, Width = 60, ColumnAlignment = ColumnAlignment.Center}, //The same as the frequency
            new ColumnsDefinition { Text = "Word", XPosition = 90, Width = 350, ColumnAlignment = ColumnAlignment.Center, Sortable = true },
            new ColumnsDefinition { Text = "Frequency", XPosition = 440, Width = 150, ColumnAlignment = ColumnAlignment.Center, Sortable = true },
            new ColumnsDefinition { Text = "Percentage", XPosition = 590, Width = 100, ColumnAlignment = ColumnAlignment.Center}, //The same as the frequency
            new ColumnsDefinition { Text = "Distribution", XPosition = 690, Width = 200, ColumnAlignment = ColumnAlignment.Center },
            new ColumnsDefinition { Text = "Category", XPosition = 890, Width = 150, ColumnAlignment = ColumnAlignment.Center },
        ];

        // var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Purple }; // --> Debugging

        for (int i = 0; i < columns.Length; i++)
        {
            Label headerLabel = new Label
            {
                Text = columns[i].Text,
                Location = new Point(columns[i].XPosition, 12),
                Size = new Size(columns[i].Width, 26),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                // BackColor = colors[i % colors.Length], // --> Debugging
                Cursor = columns[i].Sortable ? Cursors.Hand : Cursors.Default,
                Tag = i
            };

            int columnIndex = i;
            headerLabel.MouseEnter += (s, e) => ((Label)s!).ForeColor = Color.FromArgb(255, 220, 100);
            headerLabel.MouseLeave += (s, e) => ((Label)s!).ForeColor = columns[columnIndex].HeaderForeColor;

            headerLabel.Click += (s, e) =>
            {
                if (columns[columnIndex].Sortable)
                {
                    string cleanColumn = headerLabel.Text.Replace("▲", "").Replace("▼", "").Trim();

                    if (_sortColumn == cleanColumn)
                        _sortAsc = !_sortAsc;  // Toggle direction
                    else
                        _sortAsc = true;  // Default to ascending for new column

                    _sortColumn = cleanColumn;
                    SortByColumn(_sortColumn);

                    for (int j = 0; j < headerPanel.Controls.Count; j++)
                    {
                        if (j != i && columns[j].Sortable)  // Skip the clicked column, only sortable ones
                        {
                            Label otherLabel = (Label)headerPanel.Controls[j];
                            otherLabel.Text = columns[j].Text
                                .Replace("▲ ", "").Replace("▼ ", "").Trim();  // Remove any arrow
                        }
                    }

                    headerLabel.Text = _sortAsc ? $"▲{_sortColumn}" : $"▼{_sortColumn}";
                }
            };
            headerPanel.Controls.Add(headerLabel);
        }

        pnlAnalysisTableBody = new RoundPanel
        {
            FillColor = Color.Transparent,
            BorderRadius = 1,
            BorderColor = Color.Transparent,
            AutoScroll = true,
            Padding = new Padding(5, 0, 20, 0),
            Location = new Point(5, 60),
            Size = new Size(pnlAnalysisTableContainer.ClientSize.Width - 20, pnlAnalysisTableContainer.ClientSize.Height - 70)
        };

        _analysisTable = new TableBuilder(pnlAnalysisTableBody, columns, true);
        pnlAnalysisTableContainer.Controls.Add(pnlAnalysisTableBody);

        _analysisTable.RowClicked += (s, rowData) =>
        {
            var wordFrequency = (WordFrequency)rowData;
            MessageBox.Show(
                $"Word: {wordFrequency.Word}\nFrequency: {wordFrequency.Frequency}\nPercentage: {wordFrequency.Percentage:F2}%\nCategory: {wordFrequency.GetCategory()}", "Word Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        };

        pnlAnalysis.Controls.Add(pnlAnalysisTableContainer);
    }

    #endregion
}
