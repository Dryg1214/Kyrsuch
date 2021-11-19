using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Windows.Forms;

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
            processes.Clear();
            processes = Process.GetProcesses().ToList<Process>();
        }

        //класс для подсчета порядкового номера процессов с одинаковым именем
        class memList  
        {
            string prName;
            int counter;
            public memList(string NameProcess, int number)
            {
                prName = NameProcess;
                counter = number;
            }
            public string getName()
            {
                return this.prName;
            }
            public int getCounter()
            {
                return this.counter;
            }
        }
        
        //private void GetListCounter()
        //{
        //    List<memList> Lst = new List<memList>();
        //    foreach (Process p in processes)// перебор всех процессов
        //    {
        //        if (p != null)
        //        {
        //            string name = p.ProcessName;
        //            int numCopy = 0;
        //            foreach (memList obj in Lst)
        //            {
        //                if (obj.getName() == p.ProcessName)
        //                    numCopy++;
        //            }
        //            if (numCopy != 0)
        //                name += "#" + numCopy.ToString();
        //            memList elem = new memList(name, numCopy);
        //            Lst.Add(elem);
        //        }
        //    }
        //}

            //обновить список процессов (тут происходит добавление всех данных)
        private void RefreshProcessesList()
        {
            try
            {
                listView1.Items.Clear();
                double memSize; //память
                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = " ";

                //PerformanceCounterCategory processCategory = new PerformanceCounterCategory("Process");
                //var CounterNames = processCategory.GetCounters("msedge");
                //string[] instanceNames = processCategory.GetInstanceNames();

                List<memList> Lst = new List<memList>();

                foreach (Process p in processes)
                {
                    if (p != null)
                    {
                        memSize = 0;
                        string name = p.ProcessName;
                        int numCopy = 0;
                        foreach (memList obj in Lst)
                        {
                            if (obj.getName() == p.ProcessName)
                                numCopy++;
                        }
                        memList elem = new memList(p.ProcessName, numCopy);
                        Lst.Add(elem);

                        var cpu = new PerformanceCounter("Process", "% Processor Time", name, true);
                        if (numCopy == 0)
                        {
                            pc.InstanceName = p.ProcessName;
                            cpu.InstanceName = p.ProcessName;
                        }
                        else
                        {
                            name += "#" + numCopy.ToString();
                            pc.InstanceName = name;
                            cpu.InstanceName = name;
                        }
                      
                        memSize = (double)pc.NextValue() / (1024 * 1024);
                        
                        cpu.NextValue();//для инициализации потому что с 1 раза счетчик не считает и без этой строки всегда все будут 0
                        double cpup = Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2);

                        string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString(), p.Id.ToString(), cpup.ToString() };
                        listView1.Items.Add(new ListViewItem(row));

                        pc.Close();
                        pc.Dispose();
                    }
                }
                Text = $"Диспетчер задач     (Запущенно процессов : " + processes.Count.ToString() + " )";
            }
            catch (Exception) { }//Console.WriteLine(e.Message);} 
        }
        
        //тоже обновление списка но для поиска
        private void RefreshProcessesList(List<Process> processes, string keyword)
        {
            try
            {
                listView1.Items.Clear();
                double memSize; //память
                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = " ";

                PerformanceCounter cpu = new PerformanceCounter();
                cpu.CategoryName = "Process";
                cpu.CounterName = "% Processor Time";
                cpu.InstanceName = " ";

                List<memList> Lst = new List<memList>();

                foreach (Process p in processes)
                {
                    if (p != null)
                    {
                        memSize = 0;
                        string name = p.ProcessName;
                        int numCopy = 0;
                        foreach (memList obj in Lst)
                        {
                            if (obj.getName() == p.ProcessName)
                                numCopy++;
                        }
                        memList elem = new memList(p.ProcessName, numCopy);
                        Lst.Add(elem);

                        if (numCopy == 0)
                        {
                            pc.InstanceName = p.ProcessName;
                            cpu.InstanceName = p.ProcessName;
                        }
                        else
                        {
                            name += "#" + numCopy.ToString();
                            pc.InstanceName = name;
                            cpu.InstanceName = name;
                        }
                        cpu.NextValue();
                        memSize = (double)pc.NextValue() / (1024 * 1024);
                        double cpup = Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2);

                        string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString(), p.Id.ToString(), cpup.ToString() };
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
            if (pid == 0)
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
            catch (ArgumentException) { }
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
            try
            {
                string fullpath = p.Modules[0].FileName;
                //string fullpath = Path.GetFullPath(p.ProcessName);
                //Application.Ex
                return fullpath;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        //private string GetFullPathFile(string path)
        //{
        //    string fullpath = Path.GetFullPath(path);
        //    return fullpath;
        //}

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

        private void путьКФайлуToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            путьКФайлуToolStripMenuItem.DropDownItems[1].Text = null;
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            comparer.ColumnIndex = e.Column;
            comparer.SortDirection = comparer.SortDirection == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            listView1.ListViewItemSorter = comparer;
            listView1.Sort();
        }


        private void путьКФайлуToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process process = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    string text = GetFullPathFile(process);
                    ToolStripLabel tsl = new ToolStripLabel(text);
                    //tsl.AutoSize = true;
                    tsl.Height = 50;
                    tsl.Width = 500;
                    путьКФайлуToolStripMenuItem.DropDownItems.Insert(1, tsl);
                }
            }
            catch (Exception) { }
        }
    }
}
