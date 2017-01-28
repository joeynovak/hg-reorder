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
using mercurial_reorder.Properties;
using Stn.Svn;

namespace mercurial_reorder
{
   public partial class MainForm : Form
   {
      public MainForm()
      {
         InitializeComponent();
      }

      private void button1_Click(object sender, EventArgs e)
      {
         SaveSettings();
         if (!Directory.Exists(textBox1.Text))
            MessageBox.Show("Directory: " + textBox1.Text + " doesn't exist.");

         if(!Directory.Exists(textBox1.Text.TrimEnd(new char[] {'\\'}) + "\\.hg"))
            MessageBox.Show("Directory: " + textBox1.Text + " not a mercurial repo.");

         if (!Directory.Exists(textBox2.Text))
            MessageBox.Show("Directory: " + textBox2.Text + " doesn't exist.");

         if (!Directory.Exists(textBox2.Text.TrimEnd(new char[] { '\\' }) + "\\.hg"))
            MessageBox.Show("Directory: " + textBox2.Text + " not a mercurial repo.");

         string tempFileName = Path.GetTempFileName();

         string hgData =
            CommandPrompt.RunCommand("cd " + textBox1.Text + " && " +
                                     "hg log --template \"START||||{rev}||||{branch}||||{node}||||{p1node}||||{date}||||{desc}\\n\" > " + tempFileName);

         Tuple<List<HgCommit>, List<HgCommit>> commits = ParseOutput(File.ReadAllText(tempFileName));

         commits.Item1.Sort();

         foreach (var commit in commits.Item1)
         {
            CommandPrompt.RunCommand("cd " + textBox2.Text + " && hg pull " + textBox1.Text + " -r " + commit.Revision);
            foreach (var commit2 in commits.Item2)
            {
               if (commit2.ParentChangesetId == commit.ChangesetId)
               {
                  CommandPrompt.RunCommand("cd " + textBox2.Text + " && hg pull " + textBox1.Text + " -r " + commit2.Revision);
                  break;
               }
            }
         }
      }

      private Tuple<List<HgCommit>, List<HgCommit>> ParseOutput(string hgData)
      {
         List<HgCommit> byRevisionCommits = new List<HgCommit>();
         List<HgCommit> byChangeSetCommits = new List<HgCommit>();
         string[] commandOutput = hgData.Split(new string[] {"START||||"}, StringSplitOptions.RemoveEmptyEntries);
         bool firstOne = true;
         bool secondOne = true;

         foreach (var commitText in commandOutput)
         {
            if (firstOne)
            {
               firstOne = false;
               continue;
            }
            if (secondOne)
            {
               secondOne = false;
               continue;
            }

            HgCommit commit = new HgCommit(commitText);
            if (commitMatches(commit))            
               byChangeSetCommits.Add(commit);               
            else
               byRevisionCommits.Add(commit);
         }
         
         return new Tuple<List<HgCommit>, List<HgCommit>>(byRevisionCommits, byChangeSetCommits);
      }

      private bool commitMatches(HgCommit commit)
      {
         foreach (var pattern in textBox3.Text.Split('\n'))
         {            
            if (Regex.IsMatch(commit.Message, pattern.Trim()))
               return true;
         }

         return false;
      }

      private void Form1_Load(object sender, EventArgs e)
      {
         LoadSettings();

      }

      private void LoadSettings()
      {
         textBox1.Text = Settings.Default.SourcePath;
         textBox2.Text = Settings.Default.DestinationPath;
         textBox3.Text = Settings.Default.Patterns;
      }

      public void SaveSettings()
      {
         Settings.Default.SourcePath = textBox1.Text;
         Settings.Default.DestinationPath = textBox2.Text;
         Settings.Default.Patterns = textBox3.Text;
         Settings.Default.Save();
      }
   }
}
