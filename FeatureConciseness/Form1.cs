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
            int sequenceNumber = 1; 

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
                        string[] parts = trimmedLine.Split('(');
                        string methodDeclaration = parts[0].Trim();
                        string methodLine = $"{sequenceNumber}. {methodDeclaration}";
                        methodNames.Add(methodLine);
                        sequenceNumber++;
                    }
                }
            }
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

                    StringBuilder csvContent = new StringBuilder();
                    csvContent.AppendLine("Name;Output;");
                    csvContent.AppendLine($"Total Number of Function :;{totalFeatures};");
                    csvContent.AppendLine($"Total Number Line of Code :;{totalLines};");
                    csvContent.AppendLine($"Total Number of Executable Line of Code :;{totalLOC};");
                    csvContent.AppendLine($"Conciseness (#Line of Code / Function) :;{conciseness1:F2};");
                    csvContent.AppendLine($"Conciseness (#Executable Line of Code / Function) :;{conciseness2:F2};");
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
            contextMenuStrip1.Show(bt_help, new Point(0, bt_help.Height));
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string message = "How to Use the Application:\n\n";
            message += "1. Upload your C# file\n";
            message += "2. Click the 'Calculate Conciseness' button to calculate the Conciseness score\n";
            message += "3. The result of the calculation will be displayed on the screen\n";
            message += "4. If you want to export the result to a CSV file, please click the 'Export to CSV' button";

            MessageBox.Show(message, "How to Use the Application", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void onlineNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to open the Online Notes?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("https://github.com/seoeka/uas-pkpl-conciseness/blob/master/README.md");
            }
        }

    }
}
