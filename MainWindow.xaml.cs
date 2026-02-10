using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32; // Dosya seçme diyaloğu için
using System.IO;       // Dosya okuma için
using System.Collections.Generic;
using System.Linq;


namespace LogAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Orijinal veriyi saklamak için
        private List<LogEntry> _allLogs = new List<LogEntry>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateStatistics()
        {
            if (_allLogs == null) return;

            // LINQ kullanarak sayıları alıyoruz
            int errors = _allLogs.Count(x => x.Severity == "Error");
            int warnings = _allLogs.Count(x => x.Severity == "Warning");
            int debugs = _allLogs.Count(x => x.Severity == "Debug");
            int total = _allLogs.Count;

            // Arayüzdeki TextBlock'lara yazdırıyoruz
            TxtErrorCount.Text = errors.ToString();
            TxtWarningCount.Text = warnings.ToString();
            TxtDebugCount.Text = debugs.ToString();
            TxtTotalCount.Text = total.ToString();
        }

        private void BtnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _allLogs.Clear();
                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                for (int i = 0; i < lines.Length; i++)
                {
                    string severity = "Info";
                    if (lines[i].Contains("ERROR", StringComparison.OrdinalIgnoreCase)) severity = "Error";
                    else if (lines[i].Contains("WARN", StringComparison.OrdinalIgnoreCase)) severity = "Warning";
                    else if (lines[i].Contains("DEBUG", StringComparison.OrdinalIgnoreCase)) severity = "Debug";

                    _allLogs.Add(new LogEntry { LineNumber = i + 1, Severity = severity, Message = lines[i] });
                }
                LogDataGrid.ItemsSource = null;
                LogDataGrid.ItemsSource = _allLogs;
                UpdateStatistics();
            }
        }

        private void CmbSeverity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            // 1. GÜVENLİK: Eğer ana listemiz henüz oluşmadıysa (dosya seçilmediyse) fonksiyondan çık.
            if (_allLogs == null || _allLogs.Count == 0)
                return;

            // 2. GÜVENLİK: UI elemanları yüklenmeden bu metod çalışırsa hata vermemesi için kontrol.
            if (CmbSeverity == null || TxtSearch == null || LogDataGrid == null)
                return;

            // Seçili öğeyi güvenli bir şekilde alalım
            var selectedItem = CmbSeverity.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            string selectedCategory = selectedItem.Content.ToString();
            string searchKeyword = TxtSearch.Text.ToLower();

            // Filtreleme işlemi
            var filtered = _allLogs.Where(log =>
                (selectedCategory == "All" || log.Severity == selectedCategory) &&
                (string.IsNullOrEmpty(searchKeyword) || log.Message.ToLower().Contains(searchKeyword))
            ).ToList();

            LogDataGrid.ItemsSource = filtered;
        }

    }

    public class LogEntry
    {
        public int LineNumber { get; set; }
        public string Severity { get; set; } // Info, Warning, Error, Debug
        public string Message { get; set; }

        // UI tarafında renk göstermek için yardımcı özellik
        public string SeverityColor => Severity switch
        {
            "Error" => "Red",
            "Warning" => "Orange",
            "Info" => "Blue",
            "Debug" => "Gray",
            _ => "Black"
        };
    }   
}