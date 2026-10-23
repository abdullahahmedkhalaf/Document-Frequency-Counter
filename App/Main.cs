using Document_Frequency_Counter.App.Models;
using Document_Frequency_Counter.src;

namespace Document_Frequency_Counter;

public partial class Main : Form
{
    public Main()
    {
        InitializeComponent();
        SetupComponents();

        DoubleBuffered = true;

        typeof(Panel).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(pnlAnalysisTableContainer, true);

        typeof(Panel).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(pnlAnalysis, true);
    }

    private void ProcessesDroppedFiles(string[] files)
    {
        HashSet<string> supported = [".txt", ".csv", ".log", ".xml", ".json"];
        foreach (string file in files)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (!supported.Contains(ext))
            {
                MessageBox.Show(
                    $"Unsupported file type: {ext}\nFile: {Path.GetFileName(file)}",
                    "Unsupported Format",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                continue;
            }

            if (!_list.Contains(file)) _list.Add(file);

            lblFileCount.Text = $"📄 {_list.Count} file(s) loaded";
            pnlAnalysisButton.Enabled = _list.Count > 0;
            pnlClearButton.Enabled = _list.Count > 0;
        }
    }

    private void OpenFileBrowser()
    {
        using (OpenFileDialog dialog = new())
        {
            dialog.Title = "Select Document Files";
            dialog.Filter = "Text Files (*.txt)|*.txt|CSV Fils (*.csv)|*.csv|Log Files (*.log)|*.log|All Files (*.*)|*.*";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
                ProcessesDroppedFiles(dialog.FileNames);
        }
    }

    private void ShowWordFrequencies(List<WordFrequency> word)
    {
        _wordFrequencies = word;
        RefreshAnalysisTable();
    }

    private void RefreshAnalysisTable()
    {
        if (_analysisTable == null) return;

        var existingRows = pnlAnalysisTableBody.Controls
            .OfType<RoundPanel>()
            .Where(c => c.Tag is WordFrequency)
            .ToList();

        if (existingRows.Count != _wordFrequencies.Count)
        {
            pnlAnalysisTableBody.SuspendLayout();
            _analysisTable.ClearRows();
            foreach (var word in _wordFrequencies)
                _analysisTable.AddWordFrequencyRow(word);
            pnlAnalysisTableBody.ResumeLayout(true);
            return;
        }

        pnlAnalysisTableBody.SuspendLayout();

        for (int i = 0; i < _wordFrequencies.Count; i++)
        {
            var word = _wordFrequencies[i];
            var row = existingRows[i];
            row.Tag = word;
            row.Location = new Point(10, i * 45);

            // Find controls by tag
            var rankLabel = row.Controls.OfType<Label>().FirstOrDefault(c => c.Tag?.ToString() == "Rank");
            var wordLabel = row.Controls.OfType<Label>().FirstOrDefault(c => c.Tag?.ToString() == "Word");
            var percentageLabel = row.Controls.OfType<Label>().FirstOrDefault(c => c.Tag?.ToString() == "Percentage");
            var distributionLabel = row.Controls.OfType<Label>().FirstOrDefault(c => c.Tag?.ToString() == "Distribution");
            var categoryBadge = row.Controls.OfType<RoundPanel>().FirstOrDefault(c => c.Tag?.ToString() == "CategoryBadge");

            rankLabel?.Text = word.DisplayRank();
            wordLabel?.Text = word.Word;
            percentageLabel?.Text = word.PercentageBar();
            distributionLabel?.Text = word.GetBar();

            if (categoryBadge != null)
            {
                categoryBadge.FillColor = word.CategoryColor();
                var badgeLabel = categoryBadge.Controls.OfType<Label>().FirstOrDefault();
                badgeLabel?.Text = word.GetCategory();
            }
        }

        pnlAnalysisTableBody.ResumeLayout(true);
    }

    private async Task Analysis(object sender, EventArgs e)
    {
        bool analyzed = false;
        try
        {
            _cts = new CancellationTokenSource();

            pnlCancelButton.Enabled = true;
            pnlAnalysisButton.Enabled = false;
            pnlClearButton.Enabled = false;
            pnlExportButton.Enabled = false;
            pnlExportNGramsButton.Enabled = false;
            lblAnalysisButton.Text = "⏳...";

            var words = await Reader.ReadFilesAsync(_cts.Token, [.. _list]);
            var tokens = await Cleaner.CleanTexts(_cts.Token, words);
            tokensArray = tokens;
            var (map, tokensCount) = await Handler.GetFrequenciesAsync(_cts.Token, tokens);

            var newWordList = await Task.Run(async () =>
            {
                _cts.Token.ThrowIfCancellationRequested();

                var allWords = new List<WordFrequency>();
                int rank = 1;

                foreach (var kvp in map)
                {
                    allWords.Add(new()
                    {
                        Rank = rank++,
                        Word = kvp.Key,
                        Frequency = kvp.Value,
                        TotalWords = tokensCount
                    });
                }

                return allWords;
            });

            _wordFrequenciesFull = newWordList;
            _wordFrequencies = [.. _wordFrequenciesFull.Take(250)];

            if (_wordFrequencies.Count > 0)
            {
                pnlAnalysisTableContainer.SuspendLayout();
                ShowWordFrequencies(_wordFrequencies);
                pnlAnalysisTableContainer.ResumeLayout(true);

                pnlAnalysisTableBody.AutoScrollPosition = new Point(0, 0);
            }

            analyzed = true;
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show("❌ Analysis cancelled", "Cancel Analysis", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            pnlCancelButton.FillColor = Color.FromArgb(249, 250, 252);
            lblCancelButton.ForeColor = Color.FromArgb(17, 17, 17);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Analysis failed: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            pnlAnalysisButton.Enabled = true;
            lblAnalysisButton.Text = "Analysis";
            pnlCancelButton.Enabled = false;
            lblCancelButton.Text = "Cancel";
            pnlClearButton.Enabled = analyzed;
            pnlExportButton.Enabled = analyzed;
            pnlExportNGramsButton.Enabled = analyzed;
        }
    }

    private async Task Export(object sender, EventArgs e)
    {
        if (_list.Count == 0)
        {
            MessageBox.Show("No files loaded. Load files and run Analysis first.",
                "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using (SaveFileDialog dialog = new())
        {
            dialog.Title = "Export Word Frequencies As CSV";
            dialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
            dialog.FileName = "export.csv";
            if (dialog.ShowDialog() != DialogResult.OK) return;

            lblExportButton.Text = "⏳...";
            pnlExportButton.Enabled = false;

            await Task.Run(() =>
            {
                try
                {
                    File.WriteAllText(dialog.FileName, string.Empty); // Clear existing content
                    using (StreamWriter sw = new(dialog.FileName, append: true))
                    {
                        sw.WriteLine("Rank,Word,Frequency,Percentage,Distribution,Category");

                        foreach (var word in _wordFrequenciesFull) sw.WriteLine(word);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exporting failed: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            lblExportButton.Text = "Export";
            lblExportButton.ForeColor = Color.FromArgb(17, 17, 17);
            pnlExportButton.FillColor = Color.FromArgb(249, 250, 252);
            pnlExportButton.Enabled = true;

            MessageBox.Show(
                $"{_wordFrequenciesFull.Count} Rows has been exported",
                "Export Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }

    private async Task ExportNGrams(object sender, EventArgs e)
    {
        if (_list.Count == 0)
        {
            MessageBox.Show("No files loaded. Load files and run Analysis first.",
                "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using (SaveFileDialog dialog = new())
        {
            dialog.Title = "Export NGrams As CSV";
            dialog.Filter = "CSV Files (*.csv)|*.csv";
            dialog.FileName = "nGrams.csv";
            if (dialog.ShowDialog() != DialogResult.OK) return;

            lblExportNGramsButton.Text = "⏳...";
            pnlExportNGramsButton.Enabled = false;

            try
            {
                var nGramData = await Task.Run(async () =>
                {
                    var (nGrams, nGramsCount) = await Handler.GetNGramsAsync(CancellationToken.None, tokensArray);

                    var list = new List<NGramsData>();
                    int rank = 1;
                    foreach (var kvp in nGrams)
                    {
                        list.Add(new NGramsData
                        {
                            Rank = rank++,
                            Phrase = kvp.Key,
                            Frequency = kvp.Value,
                            TotalNGrams = nGramsCount
                        });
                    }
                    return list;
                });

                await Task.Run(() =>
                {
                    using (StreamWriter sw = new(dialog.FileName))
                    {
                        sw.WriteLine("Rank,Phrase,Frequency,Percentage,Distribution");
                        foreach (var nGram in nGramData)
                            sw.WriteLine(nGram.ToString());
                    }
                });

                MessageBox.Show($"{nGramData.Count} NGrams exported successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"NGrams export failed: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lblExportNGramsButton.Text = "NGrams";
                lblExportNGramsButton.ForeColor = Color.FromArgb(17, 17, 17);
                pnlExportNGramsButton.FillColor = Color.FromArgb(249, 250, 252);
                pnlExportNGramsButton.Enabled = true;
            }
        }
    }

    private void SortByColumn(string sortColumn)
    {
        switch (sortColumn)
        {
            case "Word":
                _wordFrequencies = _sortAsc
                    ? [.. _wordFrequencies.OrderBy(w => w.Word)]
                    : [.. _wordFrequencies.OrderByDescending(w => w.Word)];
                break;
            case "Frequency":
                _wordFrequencies = _sortAsc
                    ? [.. _wordFrequencies.OrderBy(w => w.Frequency)]
                    : [.. _wordFrequencies.OrderByDescending(w => w.Frequency)];
                break;
        }
        RefreshAnalysisTable();
    }
}
