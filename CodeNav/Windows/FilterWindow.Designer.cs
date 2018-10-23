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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.okButton = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.cancelButton = new System.Windows.Forms.Button();
            this.filterRulesDataGridView = new System.Windows.Forms.DataGridView();
            this.filterRuleBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.kindDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.accessDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.visibleDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.opacityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.filterRulesDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.filterRulesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.filterRulesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.kindDataGridViewTextBoxColumn,
            this.accessDataGridViewTextBoxColumn,
            this.visibleDataGridViewCheckBoxColumn,
            this.opacityDataGridViewTextBoxColumn});
            this.filterRulesDataGridView.DataSource = this.filterRuleBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.filterRulesDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.filterRulesDataGridView.Location = new System.Drawing.Point(11, 12);
            this.filterRulesDataGridView.Name = "filterRulesDataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.filterRulesDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.filterRulesDataGridView.Size = new System.Drawing.Size(320, 179);
            this.filterRulesDataGridView.TabIndex = 6;
            // 
            // filterRuleBindingSource
            // 
            this.filterRuleBindingSource.DataSource = typeof(CodeNav.Models.FilterRule);
            // 
            // kindDataGridViewTextBoxColumn
            // 
            this.kindDataGridViewTextBoxColumn.DataPropertyName = "Kind";
            this.kindDataGridViewTextBoxColumn.HeaderText = "Kind";
            this.kindDataGridViewTextBoxColumn.Name = "kindDataGridViewTextBoxColumn";
            this.kindDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.kindDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.kindDataGridViewTextBoxColumn.ToolTipText = "Opacity value between 0 and 1";
            this.kindDataGridViewTextBoxColumn.Width = 53;
            // 
            // accessDataGridViewTextBoxColumn
            // 
            this.accessDataGridViewTextBoxColumn.DataPropertyName = "Access";
            this.accessDataGridViewTextBoxColumn.HeaderText = "Access";
            this.accessDataGridViewTextBoxColumn.Name = "accessDataGridViewTextBoxColumn";
            this.accessDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.accessDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.accessDataGridViewTextBoxColumn.Width = 67;
            // 
            // visibleDataGridViewCheckBoxColumn
            // 
            this.visibleDataGridViewCheckBoxColumn.DataPropertyName = "Visible";
            this.visibleDataGridViewCheckBoxColumn.HeaderText = "Visible";
            this.visibleDataGridViewCheckBoxColumn.Name = "visibleDataGridViewCheckBoxColumn";
            this.visibleDataGridViewCheckBoxColumn.Width = 43;
            // 
            // opacityDataGridViewTextBoxColumn
            // 
            this.opacityDataGridViewTextBoxColumn.DataPropertyName = "Opacity";
            this.opacityDataGridViewTextBoxColumn.HeaderText = "Opacity";
            this.opacityDataGridViewTextBoxColumn.Name = "opacityDataGridViewTextBoxColumn";
            this.opacityDataGridViewTextBoxColumn.Width = 68;
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
        private System.Windows.Forms.DataGridViewComboBoxColumn kindDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn accessDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn visibleDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn opacityDataGridViewTextBoxColumn;
    }
}