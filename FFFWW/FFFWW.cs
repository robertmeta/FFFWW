using System;
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

        KeyboardHook hook = new KeyboardHook();

        public FFFWW()
        {
            InitializeComponent();
            // register the event that is fired after the key press.
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);

            // Just F3
            hook.RegisterHotKey((ModifierKeys)0, Keys.F3);
        }

        private void FFFWW_Load(object sender, EventArgs e)
        {
            windowTree.KeyDown += activeForm_KeyDown;
            searchBox.KeyDown += activeForm_KeyDown;
            searchBox.TextChanged += searchBox_TextChanged;
            doUpdateList();
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            doUpdateList();
        }

        private void activeForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                doSelected();
            }
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.PageDown)
            {
                if (windowTree.SelectedNode != null)
                {
                    windowTree.SelectedNode = windowTree.Nodes[windowTree.SelectedNode.Index];
                }
                else
                {
                    windowTree.SelectedNode = windowTree.GetNodeAt(new Point(0));
                }
            }
        }

        private void doUpdateList()
        {
            windowTree.Nodes.Clear();
            hiddenTree.Nodes.Clear();

            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    if (IsWindowVisible(process.MainWindowHandle))
                    {
                        windowTree.Nodes.Add(process.MainWindowHandle.ToString(), process.ProcessName + " :: " + process.MainWindowTitle + " :: " + process.Id);
                        hiddenTree.Nodes.Add(process.MainWindowHandle.ToString(), process.ProcessName + " :: " + process.MainWindowTitle + " :: " + process.Id);
                    }
                }
            }

            searchBox.Text = "";
            this.Show();
            this.Activate();
            searchBox.Focus();
        }

        private void doSelected()
        {
            if (windowTree.SelectedNode == null)
            {
                windowTree.SelectedNode = windowTree.GetNodeAt(new Point(0));
            }

            if (windowTree.SelectedNode != null) {
                int hWnd = Int32.Parse(windowTree.SelectedNode.Name);
                IntPtr intPtr_hWnd = new IntPtr(hWnd);
                SetForegroundWindow(intPtr_hWnd);
                this.Hide();
            }
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            FindClosestMatch(hiddenTree);

            /*foreach (var n in windowTree.Nodes)
            {
                string nStr = n.ToString().Replace("TreeNode: ", "");
                var dist = EditDistance(searchBox.Text, nStr);
            }*/
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

            Debug.WriteLine(re);
            Match match = Regex.Match(treeNode.Text, re, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                Debug.WriteLine("Adding: " + treeNode.Text);
                windowTree.Nodes.Add(treeNode.Name, treeNode.Text);
            }
            windowTree.Refresh();
            
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



    public sealed class KeyboardHook : IDisposable
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// Represents the window that is used internally to get the messages.
        /// </summary>
        private class Window : NativeWindow, IDisposable
        {
            private static int WM_HOTKEY = 0x0312;

            public Window()
            {
                // create the handle for the window.
                this.CreateHandle(new CreateParams());
            }

            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY)
                {
                    // get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    if (KeyPressed != null)
                        KeyPressed(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            #region IDisposable Members

            public void Dispose()
            {
                this.DestroyHandle();
            }

            #endregion
        }

        private Window _window = new Window();
        private int _currentId;

        public KeyboardHook()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate (object sender, KeyPressedEventArgs args)
            {
                if (KeyPressed != null)
                    KeyPressed(this, args);
            };
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            // increment the counter.
            _currentId = _currentId + 1;

            // register the hot key.
            if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
                throw new InvalidOperationException("Couldn’t register the hot key.");
        }

        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = _currentId; i > 0; i--)
            {
                UnregisterHotKey(_window.Handle, i);
            }

            // dispose the inner native window.
            _window.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierKeys _modifier;
        private Keys _key;

        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public Keys Key
        {
            get { return _key; }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
