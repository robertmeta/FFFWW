using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FFFWW
{
    public partial class FFFWW : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public static string GetWindowTitle(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    windows.Add(wnd);
                }

                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return GetWindowTitle(wnd).Contains(titleText);
            });
        }

        public FFFWW()
        {
            InitializeComponent();
        }

        private void FFFWW_Load(object sender, EventArgs e)
        {
            searchBox.KeyDown += activeForm_KeyDown;
            windowTree.KeyDown += activeForm_KeyDown;
            windowTree.TreeViewNodeSorter = new NodeSorter();
            searchBox.TextChanged += searchBox_TextChanged;

            doUpdateList();
        }

        public class NodeSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                TreeNode tx = x as TreeNode;
                TreeNode ty = y as TreeNode;
                return tx.Text.Length - ty.Text.Length;
            }
        }

        private void activeForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                doSelected();
            }
        }

        public static uint GetWindowPID(IntPtr hWnd)
        {
            uint lpdwProcessId;
            GetWindowThreadProcessId(hWnd, out lpdwProcessId);
            return lpdwProcessId;
        }

        private string GetWindowModuleFileName(IntPtr hWnd)
        {
            uint processId = 0;
            const int nChars = 1024;
            StringBuilder filename = new StringBuilder(nChars);
            GetWindowThreadProcessId(hWnd, out processId);
            IntPtr hProcess = OpenProcess(1040, false, processId);
            GetModuleFileNameEx(hProcess, IntPtr.Zero, filename, nChars);
            CloseHandle(hProcess);
            string[] parts = filename.ToString().Split('\\');
            return parts[parts.Length-1].Replace(".exe", "");
        }

        private void doUpdateList()
        {
            Process[] processlist = Process.GetProcesses();
            var windows = FindWindowsWithText("");
            foreach (IntPtr w in windows)
            {
                if (IsWindowVisible(w))
                {
                    string processName = GetWindowModuleFileName(w);
                    string title = GetWindowTitle(w);
                    string pid = GetWindowPID(w).ToString();
                    if (processName != "" && title != "" && pid != "")
                    {
                            TreeNode tn = new TreeNode();
                            tn.Name = w.ToString();
                            tn.Text = processName + " :: " + title + " :: " + pid;
                            TreeNode tn2 = (TreeNode)tn.Clone();
                            windowTree.Nodes.Add(tn);
                            hiddenTree.Nodes.Add(tn2);
                    }
                }
            }

            searchBox.Text = "";
            searchBox.Focus();
            this.Height = windowTree.VisibleCount * 20;
        }

        private void doSelected()
        {
            if (windowTree.SelectedNode == null)
            {
                windowTree.SelectedNode = windowTree.GetNodeAt(new Point(0));
            }

            if (windowTree.SelectedNode != null)
            {
                int hWnd = Int32.Parse(windowTree.SelectedNode.Name);
                IntPtr intPtr_hWnd = new IntPtr(hWnd);
                SetForegroundWindow(intPtr_hWnd);
                this.Close();
            }
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            FindClosestMatch(hiddenTree);
        }

        private void FindClosestMatch(TreeView treeView)
        {
            windowTree.Nodes.Clear();

            TreeNodeCollection nodes = treeView.Nodes;
            foreach (TreeNode n in nodes)
            {
                FindRecursive(n);
            }
        }

        private void FindRecursive(TreeNode treeNode)
        {
            string re = ".*?";
            foreach (char c in searchBox.Text)
            {
                re += c + ".*?";
            }
            re += "";

            Match match = Regex.Match(treeNode.Text, re, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                windowTree.Nodes.Add(treeNode);
            }

            foreach (TreeNode tn in treeNode.Nodes)
            {
                FindRecursive(tn);
            }
        }
    }
}