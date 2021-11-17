using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;
using System.IO;
using System.Dynamic;
using System.Threading;

namespace TaskManager
{
    public partial class Form1 : Form
    {
        private List<Process> processes = null;
        private ListViewItemComparer comparer = null;

        public Form1()
        {
            InitializeComponent();
        }
        private void GetProcesses()//Заполнение и обновление списка процессов
        {
            processes.Clear();//очистка списка
            processes = Process.GetProcesses().ToList<Process>();
        }

        private void RefreshProcessesList()
        {
            listView1.Items.Clear();
            double memSize = 0; //память 
            foreach (Process p in processes)// перебор всех процессов
            {
                if (p != null)
                {
                    memSize = 0;
                    PerformanceCounter pc = new PerformanceCounter();
                    pc.CategoryName = "Process";
                    pc.CounterName = "Working Set - Private";
                    pc.InstanceName = p.ProcessName;

                    memSize = (double)pc.NextValue() / (1024 * 1024);
                    
                    //var cpu = new PerformanceCounter("Process", "% Processor Time", p.ProcessName, true);
                    //cpu.NextValue();
                    //double cpup = Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2);
                    string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString(), p.Id.ToString() ,};

                    listView1.Items.Add(new ListViewItem(row));
                    
                    pc.Close();
                    pc.Dispose();

                }
            }
            Text = $"Диспетчер задач     (Запущенно процессов : " + processes.Count.ToString() + " )";
        }


        private void RefreshProcessesList(List<Process> processes, string keyword)
        {
            try
            {
                listView1.Items.Clear();
                double memSize = 0;
                foreach (Process p in processes)
                {
                    if (p != null)
                    {
                        memSize = 0;
                        PerformanceCounter pc = new PerformanceCounter("Process", "Working Set - Private", p.ProcessName);
                        //PerformanceCounter pc = new PerformanceCounter();
                        //pc.CategoryName = "Process";
                        //pc.CounterName = "Working Set - Private";
                        //pc.InstanceName = p.ProcessName;
                        memSize = (double)pc.NextValue() / (1024 * 1024);//физ память working set;


                        string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString(), p.Id.ToString() };
                        
                        listView1.Items.Add(new ListViewItem(row));

                        pc.Close();
                        pc.Dispose();
                    }
                }
                Text = $"Диспетчер задач     (Запущенно процессов '{keyword}': " + processes.Count.ToString() + ")";
            }
            catch (Exception) { }
        }

        private void killProcess(Process process)
        {
            process.Kill();
            process.WaitForExit();
        }
        
        private void killProcessAndChildren(int pid)
        {
            if(pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection objectCollection = searcher.Get();

            foreach (ManagementObject obj in objectCollection)
            {
                killProcessAndChildren(Convert.ToInt32(obj["ProcessID"]));
            }

            try
            {
                Process p = Process.GetProcessById(pid);
                p.Kill();
                p.WaitForExit();
            }
            catch(ArgumentException) { }
        }

        private int GetParentProcessId(Process p)
        {
            int parentID = 0;
            try
            {
                ManagementObject managementObject = new ManagementObject("win32_process.handle='" + p.Id + "'");
                managementObject.Get();
                parentID = Convert.ToInt32(managementObject["ParentProcessID"]);
            }
            catch (Exception) { }
            return parentID;
        }

        private string GetFullPathFile(Process p)
        {
            string fullpath = p.Modules[0].FileName;
            //string fullpath = Path.GetFullPath(p.ProcessName);
            Application.Ex
            return fullpath;
        }

        //private string GetFullPathFile(string path)
        //{
        //    string fullpath = Path.GetFullPath(path);
        //    return fullpath;
        //}

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();
            GetProcesses();
            RefreshProcessesList();
            comparer = new ListViewItemComparer();
            comparer.ColumnIndex = 0;
        }

        private void обновитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetProcesses();
            RefreshProcessesList();
        }

        private void завершитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processTokill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    killProcess(processTokill);
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void завершитьДеревоПроцессовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processTokill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    killProcessAndChildren(GetParentProcessId(processTokill));
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void запуститьНовуюЗадачуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Interaction.InputBox("Введите имя программы", "Запуск новой задачи");
            try
            {
                Process.Start(path);
            }
            catch (Exception) { }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //this.Close();
            Application.Exit();
        }
        
        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            GetProcesses();
            List<Process> sortprocess = processes.Where((x) =>
            x.ProcessName.ToLower().Contains(toolStripTextBox1.Text.ToLower())).ToList<Process>();

            RefreshProcessesList(sortprocess, toolStripTextBox1.Text);
        }
        private bool wasExecuted = false;
        private void параметрыСистемыToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            if (!wasExecuted)
            {
                string bit_depth = " ";
                if (Environment.Is64BitOperatingSystem)
                    bit_depth += "Разрядность : 64Bit\n";
                else
                    bit_depth += "Разрядность : 32Bit\n";
                string param = Environment.OSVersion.ToString() + "\n"
                                + bit_depth + "Имя компьютера : " + Environment.MachineName.ToString() + "\n"
                                + "Число процессоров : " + Environment.ProcessorCount.ToString() + "\n"
                                + "Системная папка : " + Environment.SystemDirectory.ToString() + "\n"
                                + "Логические диски : " + String.Join(", ", Environment.GetLogicalDrives()).Replace(":\\", String.Empty);
                ToolStripLabel tsl = new ToolStripLabel(param);
                tsl.Size = new Size(250, 250);
                параметрыСистемыToolStripMenuItem.DropDownItems.Add(tsl);
                wasExecuted = true;
            }
        }

        private void путьКФайлуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process process = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    string text = GetFullPathFile(process);
                    ToolStripLabel tsl = new ToolStripLabel(text);
                    tsl.Width = 300;
                    путьКФайлуToolStripMenuItem.DropDownItems.Add(tsl);
                }
            }
            catch (Exception) { }
        }

        private void путьКФайлуToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process process = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    string text = GetFullPathFile(process);
                    ToolStripLabel tsl = new ToolStripLabel(text);
                    tsl.Width = 300;
                    путьКФайлуToolStripMenuItem.DropDownItems.Add(tsl);
                }
            }
            catch (Exception) { }
        }

        private void путьКФайлуToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {

        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            comparer.ColumnIndex = e.Column;
            comparer.SortDirection = comparer.SortDirection == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            listView1.ListViewItemSorter = comparer;
            listView1.Sort();
        }
    }
}
