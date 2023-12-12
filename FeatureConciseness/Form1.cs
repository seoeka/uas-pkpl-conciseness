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
                    double conciseness1 = (double)totalLOC / totalFeatures;
                    double conciseness2 = (double)totalLines / totalFeatures;
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
    }
}
