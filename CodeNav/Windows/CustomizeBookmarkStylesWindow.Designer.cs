namespace CodeNav.Windows
{
    partial class CustomizeBookmarkStylesWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomizeBookmarkStylesWindow));
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.bookmarkStylesFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.backgroundButton = new System.Windows.Forms.Button();
            this.foregroundButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bookmarkStylesFlowLayoutPanel
            // 
            this.bookmarkStylesFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bookmarkStylesFlowLayoutPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.bookmarkStylesFlowLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.bookmarkStylesFlowLayoutPanel.Name = "bookmarkStylesFlowLayoutPanel";
            this.bookmarkStylesFlowLayoutPanel.Size = new System.Drawing.Size(376, 107);
            this.bookmarkStylesFlowLayoutPanel.TabIndex = 2;
            // 
            // backgroundButton
            // 
            this.backgroundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.backgroundButton.Location = new System.Drawing.Point(12, 125);
            this.backgroundButton.Name = "backgroundButton";
            this.backgroundButton.Size = new System.Drawing.Size(75, 23);
            this.backgroundButton.TabIndex = 4;
            this.backgroundButton.Text = "Background";
            this.backgroundButton.UseVisualStyleBackColor = true;
            this.backgroundButton.Click += new System.EventHandler(this.backgroundButton_Click);
            // 
            // foregroundButton
            // 
            this.foregroundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.foregroundButton.Location = new System.Drawing.Point(93, 125);
            this.foregroundButton.Name = "foregroundButton";
            this.foregroundButton.Size = new System.Drawing.Size(75, 23);
            this.foregroundButton.TabIndex = 5;
            this.foregroundButton.Text = "Foregound";
            this.foregroundButton.UseVisualStyleBackColor = true;
            this.foregroundButton.Click += new System.EventHandler(this.foregroundButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(313, 173);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(232, 173);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // CustomizeBookmarkStylesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 214);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.foregroundButton);
            this.Controls.Add(this.backgroundButton);
            this.Controls.Add(this.bookmarkStylesFlowLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CustomizeBookmarkStylesWindow";
            this.Text = "Customize Bookmark Styles";
            this.Load += new System.EventHandler(this.CustomizeBookmarkStylesWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.FlowLayoutPanel bookmarkStylesFlowLayoutPanel;
        private System.Windows.Forms.Button backgroundButton;
        private System.Windows.Forms.Button foregroundButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
    }
}