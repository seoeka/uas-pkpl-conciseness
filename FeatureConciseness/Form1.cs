using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FeatureConciseness
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string selectedFilePath; // Deklarasikan di sini

        private void bt_upload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    textBox1.Text = selectedFilePath; // Set the TextBox value to the selected file path.
                }
            }
        }

        private void bt_calculate_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                string[] lines = File.ReadAllLines(selectedFilePath);
                int totalFunctions = CountFunctions(lines);
                int totalLines = CountNonEmptyNonCommentLines(lines);

                if (totalLines > 0)
                {
                    double conciseness = (double)totalFunctions / totalLines;
                    label2.Text = $"Features Total: {totalFunctions}\nLine of Code: {totalLines}\nConciseness (Features/Line of Code): {conciseness:F2}";
                }
                else
                {
                    label2.Text = "No code found in the file.";
                }
            }
            else
            {
                MessageBox.Show("Please select a C# file first.");
            }
        }

        private int CountFunctions(string[] lines)
        {
            int featureCount = 0;
            bool inMultilineComment = false;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (inMultilineComment)
                {
                    if (trimmedLine.Contains("*/"))
                    {
                        inMultilineComment = false;
                    }
                }
                else
                {
                    // Memeriksa komentar multiline (/* ... */)
                    if (trimmedLine.StartsWith("/*"))
                    {
                        inMultilineComment = true;
                        continue; // Langsung ke baris berikutnya
                    }

                    // Memeriksa komentar satu baris (// ...)
                    if (trimmedLine.StartsWith("//"))
                    {
                        continue; // Langsung ke baris berikutnya
                    }

                    // Gunakan ekspresi reguler untuk mengidentifikasi deklarasi fungsi.
                    string pattern = @"(private|public|protected|internal|static|void|int|bool|double|float|decimal|char|string)?\s+(\w+\s+){0,3}\(";
                    if (Regex.IsMatch(trimmedLine, pattern))
                    {
                        featureCount++;
                    }
                }
            }

            return featureCount;
        }
        // Define a function to count non-empty non-comment lines in the code.
        private int CountNonEmptyNonCommentLines(string[] lines)
        {
            int nonEmptyLines = 0;
            bool inMultilineComment = false;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (inMultilineComment)
                {
                    if (trimmedLine.Contains("*/"))
                    {
                        inMultilineComment = false;
                    }
                }
                else
                {
                    if (trimmedLine.StartsWith("/*"))
                    {
                        inMultilineComment = true;
                        continue;
                    }

                    if (!trimmedLine.StartsWith("//") && !string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        nonEmptyLines++;
                    }
                }
            }
            return nonEmptyLines;
        }
    }
}
