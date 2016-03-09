using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace FFFWW
{
    public partial class FFFWW : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary> Get the text for the window pointed to by hWnd </summary>
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

        /// <summary> Find all windows that match the given filter </summary>
        /// <param name="filter"> A delegate that returns true for windows
        ///    that should be returned and false for windows that should
        ///    not be returned </param>
        public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary> Find all windows that contain the given title text </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
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

        // Call the procedure using the TreeView.
        private void FindClosestMatch(TreeView treeView)
        {
            windowTree.Nodes.Clear();

            // Print each node recursively.
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

            // Print each node recursively.
            foreach (TreeNode tn in treeNode.Nodes)
            {
                FindRecursive(tn);
            }
        }

        private void windowTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }
    }
}