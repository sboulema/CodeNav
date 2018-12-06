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
            this.label2 = new System.Windows.Forms.Label();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.fontButton = new System.Windows.Forms.Button();
            this.filterToolbarCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.highlightBackgroundButton = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.resetButton = new System.Windows.Forms.Button();
            this.xmlCommentsCheckBox = new System.Windows.Forms.CheckBox();
            this.historyIndicatorCheckBox = new System.Windows.Forms.CheckBox();
            this.disableHighlightCheckBox = new System.Windows.Forms.CheckBox();
            this.autoLoadLineThresholdTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
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
            "None"});
            this.marginSideComboBox.Location = new System.Drawing.Point(242, 12);
            this.marginSideComboBox.Name = "marginSideComboBox";
            this.marginSideComboBox.Size = new System.Drawing.Size(121, 21);
            this.marginSideComboBox.TabIndex = 1;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(210, 237);
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
            this.cancelButton.Location = new System.Drawing.Point(291, 237);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Font:";
            // 
            // fontDialog1
            // 
            this.fontDialog1.ShowEffects = false;
            // 
            // fontButton
            // 
            this.fontButton.Location = new System.Drawing.Point(46, 62);
            this.fontButton.Name = "fontButton";
            this.fontButton.Size = new System.Drawing.Size(75, 23);
            this.fontButton.TabIndex = 5;
            this.fontButton.Text = "Choose...";
            this.fontButton.UseVisualStyleBackColor = true;
            this.fontButton.Click += new System.EventHandler(this.fontButton_Click);
            // 
            // filterToolbarCheckBox
            // 
            this.filterToolbarCheckBox.AutoSize = true;
            this.filterToolbarCheckBox.Location = new System.Drawing.Point(12, 39);
            this.filterToolbarCheckBox.Name = "filterToolbarCheckBox";
            this.filterToolbarCheckBox.Size = new System.Drawing.Size(128, 17);
            this.filterToolbarCheckBox.TabIndex = 6;
            this.filterToolbarCheckBox.Text = "Show the filter toolbar";
            this.filterToolbarCheckBox.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Highlight background:";
            // 
            // highlightBackgroundButton
            // 
            this.highlightBackgroundButton.Location = new System.Drawing.Point(126, 92);
            this.highlightBackgroundButton.Name = "highlightBackgroundButton";
            this.highlightBackgroundButton.Size = new System.Drawing.Size(23, 23);
            this.highlightBackgroundButton.TabIndex = 8;
            this.highlightBackgroundButton.UseVisualStyleBackColor = true;
            this.highlightBackgroundButton.Click += new System.EventHandler(this.highlightBackgroundButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetButton.Location = new System.Drawing.Point(12, 237);
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
            this.xmlCommentsCheckBox.Location = new System.Drawing.Point(12, 124);
            this.xmlCommentsCheckBox.Name = "xmlCommentsCheckBox";
            this.xmlCommentsCheckBox.Size = new System.Drawing.Size(210, 17);
            this.xmlCommentsCheckBox.TabIndex = 10;
            this.xmlCommentsCheckBox.Text = "Use XML comments for method tooltips";
            this.xmlCommentsCheckBox.UseVisualStyleBackColor = true;
            // 
            // historyIndicatorCheckBox
            // 
            this.historyIndicatorCheckBox.AutoSize = true;
            this.historyIndicatorCheckBox.Location = new System.Drawing.Point(12, 147);
            this.historyIndicatorCheckBox.Name = "historyIndicatorCheckBox";
            this.historyIndicatorCheckBox.Size = new System.Drawing.Size(156, 17);
            this.historyIndicatorCheckBox.TabIndex = 11;
            this.historyIndicatorCheckBox.Text = "Show history/edit indicators";
            this.historyIndicatorCheckBox.UseVisualStyleBackColor = true;
            // 
            // disableHighlightCheckBox
            // 
            this.disableHighlightCheckBox.AutoSize = true;
            this.disableHighlightCheckBox.Location = new System.Drawing.Point(12, 170);
            this.disableHighlightCheckBox.Name = "disableHighlightCheckBox";
            this.disableHighlightCheckBox.Size = new System.Drawing.Size(127, 17);
            this.disableHighlightCheckBox.TabIndex = 12;
            this.disableHighlightCheckBox.Text = "Disable auto-highlight";
            this.disableHighlightCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoLoadLineThresholdTextBox
            // 
            this.autoLoadLineThresholdTextBox.Location = new System.Drawing.Point(12, 193);
            this.autoLoadLineThresholdTextBox.Name = "autoLoadLineThresholdTextBox";
            this.autoLoadLineThresholdTextBox.Size = new System.Drawing.Size(56, 20);
            this.autoLoadLineThresholdTextBox.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(74, 196);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(199, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Do not auto load CodeNav line threshold";
            // 
            // OptionsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 272);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.autoLoadLineThresholdTextBox);
            this.Controls.Add(this.disableHighlightCheckBox);
            this.Controls.Add(this.historyIndicatorCheckBox);
            this.Controls.Add(this.xmlCommentsCheckBox);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.highlightBackgroundButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.filterToolbarCheckBox);
            this.Controls.Add(this.fontButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.marginSideComboBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OptionsWindow";
            this.Text = "CodeNav - Options";
            this.Load += new System.EventHandler(this.OptionsToolWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox marginSideComboBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FontDialog fontDialog1;
        private System.Windows.Forms.Button fontButton;
        private System.Windows.Forms.CheckBox filterToolbarCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button highlightBackgroundButton;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.CheckBox xmlCommentsCheckBox;
        private System.Windows.Forms.CheckBox historyIndicatorCheckBox;
        private System.Windows.Forms.CheckBox disableHighlightCheckBox;
        private System.Windows.Forms.TextBox autoLoadLineThresholdTextBox;
        private System.Windows.Forms.Label label4;
    }
}