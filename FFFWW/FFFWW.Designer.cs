namespace FFFWW
{
    partial class FFFWW
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.searchBox = new System.Windows.Forms.TextBox();
            this.windowTree = new System.Windows.Forms.TreeView();
            this.hiddenTree = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.Location = new System.Drawing.Point(0, 0);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(876, 20);
            this.searchBox.TabIndex = 0;
            // 
            // windowTree
            // 
            this.windowTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.windowTree.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.windowTree.Location = new System.Drawing.Point(0, 26);
            this.windowTree.Name = "windowTree";
            this.windowTree.Size = new System.Drawing.Size(876, 548);
            this.windowTree.TabIndex = 1;
            // 
            // hiddenTree
            // 
            this.hiddenTree.Location = new System.Drawing.Point(244, 248);
            this.hiddenTree.Name = "hiddenTree";
            this.hiddenTree.Size = new System.Drawing.Size(89, 96);
            this.hiddenTree.TabIndex = 2;
            this.hiddenTree.Visible = false;
            // 
            // FFFWW
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(876, 574);
            this.Controls.Add(this.hiddenTree);
            this.Controls.Add(this.windowTree);
            this.Controls.Add(this.searchBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FFFWW";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Fuzzy Finder For Windows Windows";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FFFWW_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.TreeView windowTree;
        private System.Windows.Forms.TreeView hiddenTree;
    }
}

