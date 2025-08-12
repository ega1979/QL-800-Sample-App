using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Linq;
using System.IO;

namespace QL800Sample
{
    public partial class Form1 : Form
    {
        private PrintDocument printDoc;
        public Form1()
        {
            InitializeComponent();

            // Add Button to the form
            var btnPrint = new Button
            {
                Text = "印刷する",
                Location = new Point(10, 10),
                AutoSize = true
            };
            btnPrint.Click += BtnPrint_Click;
            Controls.Add(btnPrint);

            // Setting up the PrintDocument
            printDoc = new PrintDocument();

            // Check if the printer is available
            var targetPrinter = "Brother QL-800";
            if (!PrinterSettings.InstalledPrinters.Cast<String>().Any(p => string.Equals(p, targetPrinter, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"プリンターが見つかりません: {targetPrinter}\n先にWindowsの「プリンターとスキャナー」で追加してください。", "プリンター未検出", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // The Size of Paper
            var sizes = printDoc.PrinterSettings.PaperSizes.Cast<PaperSize>().ToList();
            var desired = sizes.FirstOrDefault(s =>
                s.PaperName.Contains("62", StringComparison.OrdinalIgnoreCase) &&
                (s.PaperName.Contains("100", StringComparison.OrdinalIgnoreCase) || s.PaperName.Contains("x100", StringComparison.OrdinalIgnoreCase))
            );
            if (desired != null)
            {
                printDoc.DefaultPageSettings.PaperSize = desired;
            }
            else
            {
                MessageBox.Show("QL-800の用紙サイズが見つかりませんでした。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // no spacing
            printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            printDoc.OriginAtMargins = false;

            printDoc.PrintPage += PrintDoc_PrintPage;

            var list = string.Join("\n", printDoc.PrinterSettings.PaperSizes.Cast<PaperSize>().Select(ps => $"{ps.PaperName} ({ps.Width}x{ps.Height})"));
            MessageBox.Show(list, "用紙サイズ一覧");
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
    {
        try
        {
            printDoc.Print();
            MessageBox.Show("印刷しました");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"印刷エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
    }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (e?.Graphics == null)
            {
                MessageBox.Show("印刷用のグラフィックス オブジェクトが取得できませんでした。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Load the backgroud image template
            string templatePath = Path.Combine(AppContext.BaseDirectory, "Resources", "template.png");
            if (File.Exists(templatePath))
            {
                using (Image template = Image.FromFile(templatePath))
                {
                    // Draw the background image fitting the page bounds
                    e.Graphics.DrawImage(template, e.PageBounds);
                }
            }
            else
            {
                MessageBox.Show("背景テンプレートが見つかりません: " + templatePath, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Print Contents above the background
            string text = "QL-800 テスト印刷\n" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            using (Font font = new Font("MS Gothic", 14))
            {
                e.Graphics.DrawString(text, font, Brushes.Black, 20, 50);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
