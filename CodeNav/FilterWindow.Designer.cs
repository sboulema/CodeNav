namespace CodeNav
{
    partial class FilterWindow
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterWindow));
            this.okButton = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.cancelButton = new System.Windows.Forms.Button();
            this.filterRulesDataGridView = new System.Windows.Forms.DataGridView();
            this.filterRuleBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.accessDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.kindDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Visible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.filterRulesDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterRuleBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(175, 201);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Constant_centered_16x.png");
            this.imageList1.Images.SetKeyName(1, "MethodAdded_16x.png");
            this.imageList1.Images.SetKeyName(2, "EnumItem_16x.png");
            this.imageList1.Images.SetKeyName(3, "Event_16x.png");
            this.imageList1.Images.SetKeyName(4, "Method_purple_16x.png");
            this.imageList1.Images.SetKeyName(5, "Property_16x.png");
            this.imageList1.Images.SetKeyName(6, "Field_blue_16x.png");
            this.imageList1.Images.SetKeyName(7, "Delegate_purple_16x.png");
            this.imageList1.Images.SetKeyName(8, "Enumerator_orange_16x.png");
            this.imageList1.Images.SetKeyName(9, "Structure_16x.png");
            this.imageList1.Images.SetKeyName(10, "AzureTeam_16x.png");
            this.imageList1.Images.SetKeyName(11, "Mail_16x.png");
            this.imageList1.Images.SetKeyName(12, "Lock_16x.png");
            this.imageList1.Images.SetKeyName(13, "Key_16x.png");
            this.imageList1.Images.SetKeyName(14, "StatusNo_grey_16x.png");
            this.imageList1.Images.SetKeyName(15, "Flow_16x.png");
            this.imageList1.Images.SetKeyName(16, "FlowDecision_16x.png");
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(256, 201);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // filterRulesDataGridView
            // 
            this.filterRulesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterRulesDataGridView.AutoGenerateColumns = false;
            this.filterRulesDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.filterRulesDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.filterRulesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.filterRulesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.accessDataGridViewTextBoxColumn,
            this.kindDataGridViewTextBoxColumn,
            this.Visible});
            this.filterRulesDataGridView.DataSource = this.filterRuleBindingSource;
            this.filterRulesDataGridView.Location = new System.Drawing.Point(11, 12);
            this.filterRulesDataGridView.Name = "filterRulesDataGridView";
            this.filterRulesDataGridView.Size = new System.Drawing.Size(320, 179);
            this.filterRulesDataGridView.TabIndex = 6;
            // 
            // filterRuleBindingSource
            // 
            this.filterRuleBindingSource.DataSource = typeof(CodeNav.Models.FilterRule);
            // 
            // accessDataGridViewTextBoxColumn
            // 
            this.accessDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.accessDataGridViewTextBoxColumn.DataPropertyName = "Access";
            this.accessDataGridViewTextBoxColumn.HeaderText = "Access";
            this.accessDataGridViewTextBoxColumn.Name = "accessDataGridViewTextBoxColumn";
            this.accessDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.accessDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // kindDataGridViewTextBoxColumn
            // 
            this.kindDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.kindDataGridViewTextBoxColumn.DataPropertyName = "Kind";
            this.kindDataGridViewTextBoxColumn.HeaderText = "Kind";
            this.kindDataGridViewTextBoxColumn.Name = "kindDataGridViewTextBoxColumn";
            this.kindDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.kindDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // Visible
            // 
            this.Visible.DataPropertyName = "Visible";
            this.Visible.HeaderText = "Visible";
            this.Visible.Name = "Visible";
            this.Visible.Width = 43;
            // 
            // FilterWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 236);
            this.Controls.Add(this.filterRulesDataGridView);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FilterWindow";
            this.Text = "Filter items";
            this.Load += new System.EventHandler(this.FilterToolWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.filterRulesDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterRuleBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.DataGridView filterRulesDataGridView;
        private System.Windows.Forms.BindingSource filterRuleBindingSource;
        private System.Windows.Forms.DataGridViewComboBoxColumn accessDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn kindDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Visible;
    }
}