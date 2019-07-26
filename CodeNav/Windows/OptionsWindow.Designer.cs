namespace CodeNav.Windows
{
    partial class OptionsWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsWindow));
            this.label1 = new System.Windows.Forms.Label();
            this.marginSideComboBox = new System.Windows.Forms.ComboBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.filterToolbarCheckBox = new System.Windows.Forms.CheckBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.resetButton = new System.Windows.Forms.Button();
            this.xmlCommentsCheckBox = new System.Windows.Forms.CheckBox();
            this.historyIndicatorCheckBox = new System.Windows.Forms.CheckBox();
            this.disableHighlightCheckBox = new System.Windows.Forms.CheckBox();
            this.autoLoadLineThresholdTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.windowBackgroundButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.highlightBackgroundButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.fontButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(227, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Show CodeNav on this side of the code editor:";
            // 
            // marginSideComboBox
            // 
            this.marginSideComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.marginSideComboBox.FormattingEnabled = true;
            this.marginSideComboBox.Items.AddRange(new object[] {
            "Left",
            "Right",
            "None",
            "Top"});
            this.marginSideComboBox.Location = new System.Drawing.Point(239, 11);
            this.marginSideComboBox.Name = "marginSideComboBox";
            this.marginSideComboBox.Size = new System.Drawing.Size(121, 21);
            this.marginSideComboBox.TabIndex = 1;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(248, 243);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(329, 243);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // fontDialog1
            // 
            this.fontDialog1.ShowEffects = false;
            // 
            // filterToolbarCheckBox
            // 
            this.filterToolbarCheckBox.AutoSize = true;
            this.filterToolbarCheckBox.Location = new System.Drawing.Point(9, 38);
            this.filterToolbarCheckBox.Name = "filterToolbarCheckBox";
            this.filterToolbarCheckBox.Size = new System.Drawing.Size(128, 17);
            this.filterToolbarCheckBox.TabIndex = 6;
            this.filterToolbarCheckBox.Text = "Show the filter toolbar";
            this.filterToolbarCheckBox.UseVisualStyleBackColor = true;
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetButton.Location = new System.Drawing.Point(12, 243);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(75, 23);
            this.resetButton.TabIndex = 9;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // xmlCommentsCheckBox
            // 
            this.xmlCommentsCheckBox.AutoSize = true;
            this.xmlCommentsCheckBox.Location = new System.Drawing.Point(9, 61);
            this.xmlCommentsCheckBox.Name = "xmlCommentsCheckBox";
            this.xmlCommentsCheckBox.Size = new System.Drawing.Size(210, 17);
            this.xmlCommentsCheckBox.TabIndex = 10;
            this.xmlCommentsCheckBox.Text = "Use XML comments for method tooltips";
            this.xmlCommentsCheckBox.UseVisualStyleBackColor = true;
            // 
            // historyIndicatorCheckBox
            // 
            this.historyIndicatorCheckBox.AutoSize = true;
            this.historyIndicatorCheckBox.Location = new System.Drawing.Point(9, 84);
            this.historyIndicatorCheckBox.Name = "historyIndicatorCheckBox";
            this.historyIndicatorCheckBox.Size = new System.Drawing.Size(156, 17);
            this.historyIndicatorCheckBox.TabIndex = 11;
            this.historyIndicatorCheckBox.Text = "Show history/edit indicators";
            this.historyIndicatorCheckBox.UseVisualStyleBackColor = true;
            // 
            // disableHighlightCheckBox
            // 
            this.disableHighlightCheckBox.AutoSize = true;
            this.disableHighlightCheckBox.Location = new System.Drawing.Point(9, 107);
            this.disableHighlightCheckBox.Name = "disableHighlightCheckBox";
            this.disableHighlightCheckBox.Size = new System.Drawing.Size(127, 17);
            this.disableHighlightCheckBox.TabIndex = 12;
            this.disableHighlightCheckBox.Text = "Disable auto-highlight";
            this.disableHighlightCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoLoadLineThresholdTextBox
            // 
            this.autoLoadLineThresholdTextBox.Location = new System.Drawing.Point(9, 130);
            this.autoLoadLineThresholdTextBox.Name = "autoLoadLineThresholdTextBox";
            this.autoLoadLineThresholdTextBox.Size = new System.Drawing.Size(56, 20);
            this.autoLoadLineThresholdTextBox.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(71, 133);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(199, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Do not auto load CodeNav line threshold";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(414, 235);
            this.tabControl1.TabIndex = 15;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.marginSideComboBox);
            this.tabPage1.Controls.Add(this.autoLoadLineThresholdTextBox);
            this.tabPage1.Controls.Add(this.disableHighlightCheckBox);
            this.tabPage1.Controls.Add(this.historyIndicatorCheckBox);
            this.tabPage1.Controls.Add(this.filterToolbarCheckBox);
            this.tabPage1.Controls.Add(this.xmlCommentsCheckBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(406, 209);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.fontButton);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.windowBackgroundButton);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.highlightBackgroundButton);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(406, 209);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Fonts and Colors";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(109, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Window background:";
            // 
            // windowBackgroundButton
            // 
            this.windowBackgroundButton.Location = new System.Drawing.Point(123, 40);
            this.windowBackgroundButton.Name = "windowBackgroundButton";
            this.windowBackgroundButton.Size = new System.Drawing.Size(23, 23);
            this.windowBackgroundButton.TabIndex = 12;
            this.windowBackgroundButton.UseVisualStyleBackColor = true;
            this.windowBackgroundButton.Click += new System.EventHandler(this.WindowBackgroundButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Highlight background:";
            // 
            // highlightBackgroundButton
            // 
            this.highlightBackgroundButton.Location = new System.Drawing.Point(123, 11);
            this.highlightBackgroundButton.Name = "highlightBackgroundButton";
            this.highlightBackgroundButton.Size = new System.Drawing.Size(23, 23);
            this.highlightBackgroundButton.TabIndex = 10;
            this.highlightBackgroundButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Font:";
            // 
            // fontButton
            // 
            this.fontButton.Location = new System.Drawing.Point(123, 69);
            this.fontButton.Name = "fontButton";
            this.fontButton.Size = new System.Drawing.Size(75, 23);
            this.fontButton.TabIndex = 14;
            this.fontButton.Text = "Choose...";
            this.fontButton.UseVisualStyleBackColor = true;
            // 
            // OptionsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 278);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OptionsWindow";
            this.Text = "CodeNav - Options";
            this.Load += new System.EventHandler(this.OptionsToolWindow_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox marginSideComboBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FontDialog fontDialog1;
        private System.Windows.Forms.CheckBox filterToolbarCheckBox;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.CheckBox xmlCommentsCheckBox;
        private System.Windows.Forms.CheckBox historyIndicatorCheckBox;
        private System.Windows.Forms.CheckBox disableHighlightCheckBox;
        private System.Windows.Forms.TextBox autoLoadLineThresholdTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button windowBackgroundButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button highlightBackgroundButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button fontButton;
    }
}