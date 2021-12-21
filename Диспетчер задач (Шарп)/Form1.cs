using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Collections;
using System.Management;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;

namespace TaskManager
{
    public partial class Form1 : Form
    {
        public bool erroccured = false;
        private List<Process> processes = null;
        private ListViewItemComparer comparer = null;
        private static System.Timers.Timer timer;
        List<memList> Lst = new List<memList>();
        public Hashtable presentprocdetails = new Hashtable();
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
            int pid;
            public int Count { get; set; } = 0;
            public memList(string NameProcess, int number, int id)
            {
                prName = NameProcess;
                counter = number;
                pid = id;
            }
            public void setName(string new_name)
            {
                this.prName = new_name;
            }
            public string getName()
            {
                return this.prName;
            }
            public void setCount(int num)
            {
                this.counter = num;
            }
            public int getCount()
            {
                return this.counter;
            }
            public int getPid()
            {
                return this.pid;
            }
        }
        private void LoadAllProcessesOnStartup()
        {
            Process[] processes = null;
            try
            {
                processes = Process.GetProcesses();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            PerformanceCounter pc = new PerformanceCounter();
            double memSize = 0;
            DateTime start = DateTime.Now;
            foreach (Process p in processes)
            {
                try
                {
                    //if (p != null)
                    //{
                        pc.CategoryName = "Process";
                        pc.CounterName = "Working Set - Private";
                        pc.InstanceName = " ";

                        memSize = 0;
                        string name = p.ProcessName;
                        int numCopy = 0;
                        if (presentprocdetails.Contains(name))
                        {
                            memList info = (memList)presentprocdetails[name];
                            int count = info.getCount();
                            count+=1;
                            info.setCount(count);
                            presentprocdetails[name] = info;
                            name += "#" + count;
                            numCopy = count;
                        }
                        memList el = new memList(name, numCopy, p.Id);
                        presentprocdetails.Add(name, el);//первое имя key, второе value;
                        pc.InstanceName = name;
                        
                        /*
                        pc.CategoryName = "Process";
                        pc.CounterName = "Working Set - Private";
                        pc.InstanceName = " ";
                        memSize = 0;
                        string name = p.ProcessName;
                        int numCopy = 0;
                        foreach (memList obj in Lst)
                        {
                            if (obj.getName() == p.ProcessName)
                                numCopy++;
                        }
                        memList elem = new memList(p.ProcessName, numCopy, p.Id);
                        Lst.Add(elem);

                        if (numCopy == 0)
                        {
                            pc.InstanceName = p.ProcessName;
                        }
                        else
                        {
                            name += "#" + numCopy.ToString();
                            pc.InstanceName = name;
                        }
                        int cpu = 0;
                        */
                        memSize = (double)pc.NextValue() / (1024 * 1024);
                        string[] prcdetails = new string[] { p.ProcessName, p.Handle.ToString(), p.Id.ToString(), Math.Round(memSize, 1).ToString(), p.TotalProcessorTime.Duration().Hours.ToString() + ":" + p.TotalProcessorTime.Duration().Minutes.ToString() + ":" + p.TotalProcessorTime.Duration().Seconds.ToString() };
                        ListViewItem proc = new ListViewItem(prcdetails);
                        proc.Tag = pc;
                        listView1.Items.Add(proc);
                    //}
                }
                catch { }
            }
            //6 секунд хэш
            //9 секунд список, но в 1 раз 
            //остальные разы по 6 оба, но хэш чуть быстрее
            //DateTime now = DateTime.Now;
            //TimeSpan time = now - start;
            Text = $"Диспетчер задач     (Запущенно процессов : " + listView1.Items.Count + " )";
        }

        private void RefreshProcessesList()
        {
            DateTime start = DateTime.Now;
            Process[] processes = null;
            try
            {
                processes = Process.GetProcesses();
            }
            catch (Exception) { }

            if (listView1.Items.Count == processes.Length)
            {
                presentprocdetails.Clear();
                foreach (ListViewItem lvi in listView1.Items)
                {
                    try
                    {
                        //PerformanceCounterCategory processCategory = new PerformanceCounterCategory("Process");
                        //var CounterNames = processCategory.GetCounters("msedge");
                        //string[] instanceNames = processCategory.GetInstanceNames();

                        PerformanceCounter mem_mb = (PerformanceCounter)(lvi.Tag);
                        string name = lvi.Text;
                        int numCopy = 0;
                        mem_mb.CategoryName = "Process";
                        mem_mb.CounterName = "Working Set - Private";
                        if (presentprocdetails.Contains(name))
                        {
                            int count = (int)presentprocdetails[name];
                            count++;
                            presentprocdetails[name] = count;
                            name += "#" + count;
                        }
                        presentprocdetails.Add(name, numCopy);
                        mem_mb.InstanceName = name;

                        double mem = (double)mem_mb.NextValue() / (1024 * 1024);
                        // int id_pr = (int)lvi.SubItems[2].Text;
                        Process process = processes.Where((x) => x.Id == int.Parse(lvi.SubItems[2].Text)).ToList()[0];
                        lvi.SubItems[3].Text = Math.Round(mem, 1).ToString();
                        lvi.SubItems[0].Text = process.ProcessName;
                        lvi.SubItems[1].Text = process.Handle.ToString();
                        lvi.SubItems[2].Text = process.Id.ToString();
                        lvi.SubItems[4].Text = process.TotalProcessorTime.Duration().Hours.ToString() + ":" + process.TotalProcessorTime.Duration().Minutes.ToString() + ":"
                            + process.TotalProcessorTime.Duration().Seconds.ToString();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            else
            {
                presentprocdetails.Clear();
                foreach (Process p in processes)
                {
                    try
                    {
                        string name = p.ProcessName;
                        int num = 0;
                        if (presentprocdetails.Contains(name))
                        {
                            memList info = (memList)presentprocdetails[name];
                            int count = info.getCount();
                            count += 1;
                            info.setCount(count);
                            presentprocdetails[name] = info;
                            name += "#" + count;
                            num = count;
                        }
                        memList el = new memList(name, num, p.Id);
                        presentprocdetails.Add(name, el);//первое имя key, второе value;
                    }
                    catch { }
                }
                Hashtable search_id = null;
                search_id = new Hashtable();

                ICollection values = presentprocdetails.Values;

                foreach (memList elem in values)
                {
                    search_id.Add(elem.getName(), elem.getPid());
                }
                DateTime now = DateTime.Now;
                TimeSpan time = now - start;

                foreach (ListViewItem lvi in listView1.Items)
                {
                    try
                    {
                        if (search_id.ContainsValue(int.Parse(lvi.SubItems[2].Text)))
                        {
                            string name = "";
                            int pid = 0;
                            foreach (memList elem in values)
                            {
                                if (elem.getPid() == int.Parse(lvi.SubItems[2].Text))
                                {
                                    name = elem.getName();
                                    pid = elem.getPid();
                                    break;
                                }
                            }
                            search_id.Remove(name);
                            PerformanceCounter mem_mb = (PerformanceCounter)(lvi.Tag);
                            mem_mb.InstanceName = name;
                            double mem = (double)mem_mb.NextValue() / (1024 * 1024);
                            Process process = processes.Where((x) => x.Id == pid).ToList()[0];
                            lvi.SubItems[3].Text = Math.Round(mem, 1).ToString();
                            lvi.SubItems[0].Text = process.ProcessName;
                            lvi.SubItems[1].Text = process.Handle.ToString();
                            lvi.SubItems[2].Text = process.Id.ToString();
                            lvi.SubItems[4].Text = process.TotalProcessorTime.Duration().Hours.ToString() + ":" + process.TotalProcessorTime.Duration().Minutes.ToString() + ":"
                                + process.TotalProcessorTime.Duration().Seconds.ToString();
                        }
                        else
                        {
                            lvi.Remove();
                        }
                    }
                    catch (Exception ex)
                    {
                        var a = ex.Message;
                        continue;
                    }
                }
                if (search_id.Count != 0)
                {
                    try
                    {
                        PerformanceCounter pc = new PerformanceCounter();
                        pc.CategoryName = "Process";
                        pc.CounterName = "Working Set - Private";
                        pc.InstanceName = " ";
                        //Process process = processes.Where((x) => x.Id == pid).ToList()[0];

                        IDictionaryEnumerator val = search_id.GetEnumerator();
                        val.Reset();
                        while (val.MoveNext())
                        {
                            try
                            {
                                double memSize = 0;
                                Process p = processes.Where((x) => x.Id == int.Parse(val.Value.ToString())).ToList()[0];
                                pc.InstanceName = val.Key.ToString();
                                memSize = (double)pc.NextValue() / (1024 * 1024);
                                string[] prcdetails = new string[] { p.ProcessName, p.Handle.ToString(), p.Id.ToString(), Math.Round(memSize, 1).ToString(), p.TotalProcessorTime.Duration().Hours.ToString() + ":" + p.TotalProcessorTime.Duration().Minutes.ToString() + ":" + p.TotalProcessorTime.Duration().Seconds.ToString() };
                                ListViewItem proc = new ListViewItem(prcdetails);
                                proc.Tag = pc;
                                listView1.Items.Add(proc);
                            }
                            catch (Exception e) {
                                var a = e.Message;
                                continue; }
                        }
                    }
                    catch (Exception) { };
                }
            }
            Text = $"Диспетчер задач     (Запущенно процессов : " + listView1.Items.Count + " )";
        }

        //тоже обновление списка но для поиска
        private void RefreshProcessesList(List<Process> processes, string keyword)
        {
            listView1.Items.Clear();
            double memSize; //память
            PerformanceCounter pc = new PerformanceCounter();
            pc.CategoryName = "Process";
            pc.CounterName = "Working Set - Private";
            pc.InstanceName = " ";
            presentprocdetails.Clear();
            foreach (Process p in processes)
            {
                try
                {
                    string name = p.ProcessName;
                    int numCopy = 0;
                    memSize = 0;
                    if (presentprocdetails.Contains(name))
                    {
                        memList info = (memList)presentprocdetails[name];
                        int count = info.getCount();
                        count += 1;
                        info.setCount(count);
                        presentprocdetails[name] = info;
                        name += "#" + count;
                        numCopy = count;
                    }
                    memList el = new memList(name, numCopy, p.Id);
                    presentprocdetails.Add(name, el);//первое имя key, второе value;
                    pc.InstanceName = name;

                    memSize = (double)pc.NextValue() / (1024 * 1024);

                    string[] row = new string[] { p.ProcessName, p.Handle.ToString(), p.Id.ToString(), Math.Round(memSize, 1).ToString(), p.TotalProcessorTime.Duration().Hours.ToString() + ":" + p.TotalProcessorTime.Duration().Minutes.ToString() + ":" + p.TotalProcessorTime.Duration().Seconds.ToString() };
                    ListViewItem proc = new ListViewItem(row);
                    proc.Tag = pc;
                    listView1.Items.Add(proc);
                }
                catch (Exception) { continue; }
            }
            Text = $"Диспетчер задач     (Запущенно процессов '{keyword}': " + listView1.Items.Count.ToString() + ")";
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
                return fullpath;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();
            LoadAllProcessesOnStartup();
            comparer = new ListViewItemComparer();
            comparer.ColumnIndex = 0;
        }

        private void UpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //GetProcesses();
            RefreshProcessesList();
        }

        private void TerminateProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process[] processes = null;                    
                    processes = Process.GetProcesses();                    
                    Process processTokill = processes.Where((x) => x.Id ==
                    int.Parse(listView1.SelectedItems[0].SubItems[2].Text)).ToList()[0];

                    killProcess(processTokill);
                    
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void TerminateProcessTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process[] processes = null;
                    processes = Process.GetProcesses();
                    Process processTokill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    killProcessAndChildren(GetParentProcessId(processTokill));
                    //GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void StartNewProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Interaction.InputBox("Введите имя программы", "Запуск новой задачи");
           // string path = GetFullPathFile(name);
            try
            {
                Process.Start(path);
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message + "\nПопробуйте ввести путь к исполняемому файлу", "Окно Исключений");
            }
            catch (Exception) { }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //this.Close();
            Application.Exit();
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            Process[] processes = null;
            processes = Process.GetProcesses();
            List<Process> sortprocess = processes.Where((x) =>
            x.ProcessName.ToLower().Contains(toolStripTextBox1.Text.ToLower())).ToList<Process>();

            RefreshProcessesList(sortprocess, toolStripTextBox1.Text);
        }

        private bool wasExecuted = false;
        private void ParamSystemToolStripMenuItem_MouseEnter(object sender, EventArgs e)
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
                ParamSystemToolStripMenuItem.DropDownItems.Add(tsl);
                wasExecuted = true;
            }
        }

        private void pathFileToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            pathFileToolStripMenuItem.DropDownItems[1].Text = null;
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            comparer.ColumnIndex = e.Column;
            comparer.SortDirection = comparer.SortDirection == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            listView1.ListViewItemSorter = comparer;
            listView1.Sort();
        }


        private void pathFileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            //исключения думаю что не делать, т.к в функции есть спросить Е.В
            Process[] processes = null;
            processes = Process.GetProcesses();
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
                    pathFileToolStripMenuItem.DropDownItems.Insert(1, tsl);
                }
            }
            catch (Exception) { }
        }
    }
}
