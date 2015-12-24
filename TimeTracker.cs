using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TimeTracker
{
    public partial class Form1 : Form
    {
        const string filePath = @"C:\Users\Desktop\TimeTracker\";
        const string archiveFilePath = @"C:\Users\Desktop\TimeTracker\Archive";
        string path = filePath + DateTime.Now.Date.ToString("yyyyMMdd") + " Daily Tracker.txt";

        public void makeOutputFile(DateTime date)
        {
            string dateToParse = date.Date.ToString("yyyyMMdd");
            string outputPath = filePath + dateToParse + " Daily Output.txt";
            string inputPath = filePath + dateToParse + " Daily Tracker.txt";

            if (!File.Exists(inputPath))
            {
                lblErrorLabel.Text = "Cannot sum as Tracker File does not exist for date";
            }
            else
            {
                Dictionary<string, Task> daysTasks = new Dictionary<string, Task>();

                using (StreamReader sr = new StreamReader(inputPath))
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        string[] lineSplit = line.Split('|');
                        string taskName = lineSplit[0];
                        string hours = lineSplit[1];
                        string duration = lineSplit[2] + " " + lineSplit[3];
                        string comments = lineSplit[4];

                        if (daysTasks.Keys.Contains(taskName))
                        {
                            daysTasks[taskName].AddTaskTime(comments, hours, duration);
                        }
                        else
                        {
                            daysTasks.Add(taskName, new Task(taskName, comments, hours, duration));
                        }
                    }
                    sr.Close();
                }

                StreamWriter sw = new StreamWriter(outputPath);
                foreach (KeyValuePair<string, Task> taskEntry in daysTasks)
                {
                    Task summedTask = taskEntry.Value;
                    double roundedSum = summedTask.Hours % 0.25 >= 0.15 ? 0.25 - (summedTask.Hours % 0.25) : -(summedTask.Hours % 0.25);
                    double summedHours = summedTask.Hours + roundedSum;
                    sw.WriteLine(summedTask.Name + "|" + summedHours + "|" + String.Join("\n", summedTask.Comments));
                }
                sw.Close();
            }
        }

        public Form1()
        {
            InitializeComponent();

            dtpSumAllTasks.Value = DateTime.Today;
            if (!File.Exists(path))
            {
                FileStream f = File.Create(path);
                f.Close();
            }

            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
            {
                for(int i = 2; i < 9; i++)
                {
                    DateTime dayX = DateTime.Now.Date.AddDays(-i);
                    string dayXFilePath = filePath +  dayX.ToString("yyyyMMdd") + " Daily Tracker.txt";
                    string dayXArchivePath = archiveFilePath +  dayX.ToString("yyyyMMdd") + " Daily Tracker.txt";

                    string dayXSummaryFilePath = filePath + dayX.ToString("yyyyMMdd") + " Daily Output.txt";
                    string dayXSummaryArchivePath = archiveFilePath + dayX.ToString("yyyyMMdd") + " Daily Output.txt";

                    if (File.Exists(dayXFilePath))
                    {
                        File.Move(dayXFilePath, archiveFilePath);

                        if (!File.Exists(dayXSummaryFilePath))
                        {
                            makeOutputFile(dayX);
                        }
                        File.Move(dayXSummaryFilePath, dayXSummaryArchivePath);
                    }
                }
               
            }
        }

        private void btnTrackTask_Click(object sender, EventArgs e)
        {
            lblErrorLabel.Text = "";

            string currentTime = DateTime.Now.ToString("HH:mm");
            string taskName = txtTaskName.Text;
            string lastTaskName = lblLastTask.Text;
            if (lastTaskName == "" && taskName == "")
            {
                lblErrorLabel.Text = "Cannot track if previous task and current task are empty";
            }
            else
            {
                if (lastTaskName != "")
                {
                    string previousTime = lblLastTaskStart.Text;
                    string previousTaskLength = DateTime.Parse(currentTime).Subtract(DateTime.Parse(previousTime)).TotalHours.ToString();
                    string previousComments = lblLastComments.Text;
                    if (File.Exists(path))
                    {
                        using (StreamWriter w = File.AppendText(path))
                        {
                            w.WriteLine(lastTaskName.ToUpper() + "|" + previousTaskLength + "|" + previousTime + "|" + currentTime + "|" + previousComments);
                            w.Close();
                        }
                    }
                }
                lblLastTask.Text = taskName;
                lblLastTaskStart.Text = taskName != "" ? currentTime : "";
                lblLastComments.Text = txtComments.Text;
                txtTaskName.Text = "";
                txtComments.Text = "";
            }
        }

        private void btnParseAll_Click(object sender, EventArgs e)
        {
            DateTime date = dtpSumAllTasks.Value;
            makeOutputFile(date);
        }
    }


    public class Task
    {
        public string Name { get; set; }
        public List<string> Comments = new List<string>();
        public double Hours { get; set; }
        public List<string> TaskDurations = new List<string>();

        public Task(string name, string comments, string hours, string taskDuration)
        {
            Name = name;
            Comments.Add(comments);
            Hours = Double.Parse(hours);
            TaskDurations.Add(taskDuration);
        }

        public void AddTaskTime(string comments, string hours, string taskDuration)
        {
            var test = taskDuration;
            if (!this.TaskDurations.Contains(taskDuration))
            {
                this.Hours += Double.Parse(hours);
                this.TaskDurations.Add(taskDuration);
                if(!this.Comments.Contains(comments))
                {
                    this.Comments.Add(comments);
                }
            }
        }
    }
}
