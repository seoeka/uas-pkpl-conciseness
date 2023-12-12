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

        private string selectedFilePath;

        private void bt_upload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    textBox1.Text = selectedFilePath;
                }
            }
        }

        private void bt_calculate_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                string[] lines = File.ReadAllLines(selectedFilePath);
                int totalFeatures = CountFeatures(lines);
                int totalLines = CountNonEmptyNonCommentLines(lines);
                int totalLOC = CountCommentLines(lines);

                if (totalLines > 0)
                {
                    double conciseness1 = (double)totalLines / totalFeatures;
                    double conciseness2 = (double)totalLOC / totalFeatures;
                    label2.Text = $"Total Number of Function : {totalFeatures}\nTotal Number Line of Code : {totalLOC}\nTotal Number of Executable Line of Code : {totalLines}\n\nConciseness (#Line of Code / Function) = {conciseness1:F2}\nConciseness  (#Executable Line of Code / Function) = {conciseness2:F2}";

                    string methodNames = ExtractMethodNames(lines);
                    label3.Text = $"{methodNames}";
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

        private int CountFeatures(string[] lines)
        {
            int featureCount = 0;
            bool inMultilineComment = false;

            string pattern = @"^\s*(private|public|protected|internal|static)?\s\w+\s\w+\s*\(.*\)";

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

                    if (!trimmedLine.StartsWith("//") && Regex.IsMatch(trimmedLine, pattern))
                    {
                        featureCount++;
                    }
                }
            }

            return featureCount;
        }
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

        private int CountCommentLines(string[] lines)
        {
            int allLines = 0;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                allLines++;
            }
            return allLines;
        }
        private string ExtractMethodNames(string[] lines)
        {
            List<string> methodNames = new List<string>();
            bool inMultilineComment = false;
            int sequenceNumber = 1; // Sequential number counter

            string pattern = @"^\s*(private|public|protected|internal|static)?\s\w+\s\w+\s*\(.*\)";

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

                    if (!trimmedLine.StartsWith("//") && Regex.IsMatch(trimmedLine, pattern))
                    {
                        // Extract and add method names with sequential numbers
                        string[] parts = trimmedLine.Split('(');
                        string methodDeclaration = parts[0].Trim();
                        string methodLine = $"{sequenceNumber}. {methodDeclaration}";
                        methodNames.Add(methodLine);
                        sequenceNumber++; // Increment the sequential number
                    }
                }
            }

            // Join the method names into a string
            return string.Join("\n", methodNames);
        }

        private void bt_export_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                string[] lines = File.ReadAllLines(selectedFilePath);
                int totalFeatures = CountFeatures(lines);
                int totalLines = CountNonEmptyNonCommentLines(lines);
                int totalLOC = CountCommentLines(lines);

                if (totalLines > 0)
                {
                    double conciseness1 = (double)totalLines / totalFeatures;
                    double conciseness2 = (double)totalLOC / totalFeatures;

                    // Create a StringBuilder to build the CSV content
                    StringBuilder csvContent = new StringBuilder();
                    csvContent.AppendLine("Name;Output;");
                    csvContent.AppendLine($"Total Number of Function;{totalFeatures};");
                    csvContent.AppendLine($"Total Number Line of Code;{totalLines};");
                    csvContent.AppendLine($"Total Number of Executable Line of Code;{totalLOC};");
                    csvContent.AppendLine($"Conciseness (#Line of Code / Function);{conciseness1:F2};");
                    csvContent.AppendLine($"Conciseness (#Executable Line of Code / Function);{conciseness2:F2};");
                    csvContent.AppendLine(";;");
                    csvContent.AppendLine("Function Name;");

                    string methodNames = ExtractMethodNames(lines);
                    string[] methods = methodNames.Split('\n');
                    int sequenceNumber = 1;

                    foreach (string method in methods)
                    {
                        if (!string.IsNullOrWhiteSpace(method))
                        {
                            string methodName = method.Trim();
                            // Menghapus nomor urut jika sudah ada
                            methodName = System.Text.RegularExpressions.Regex.Replace(methodName, @"^\d+\.", "");
                            csvContent.AppendLine($"{sequenceNumber}. {methodName.Replace(". ", ";")};");
                            sequenceNumber++;
                        }
                    }

                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                        saveFileDialog.FilterIndex = 1;
                        saveFileDialog.RestoreDirectory = true;

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Save the CSV content to the selected file
                            string csvFilePath = saveFileDialog.FileName;
                            File.WriteAllText(csvFilePath, csvContent.ToString());

                            MessageBox.Show($"CSV file exported successfully: {csvFilePath}");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No code found in the file.");
                }
            }
            else
            {
                MessageBox.Show("Please select a C# file first.");
            }
        }

        private void bt_help_Click(object sender, EventArgs e)
        {
            // Buka link menggunakan browser default
            System.Diagnostics.Process.Start("https://github.com/seoeka/uas-pkpl-conciseness/blob/master/README.md");
        }

    }
}
