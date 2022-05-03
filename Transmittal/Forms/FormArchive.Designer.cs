namespace Transmittal.Forms;

partial class FormArchive
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStripButtonMerge = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonTransmittalReport = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSummary = new System.Windows.Forms.ToolStripButton();
            this.sfDataGridTransmittals = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.sfDataGridTransmittalItems = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            this.sfDataGridTransmittalDistribution = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sfDataGridTransmittals)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sfDataGridTransmittalItems)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sfDataGridTransmittalDistribution)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 560);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(984, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonTransmittalReport,
            this.toolStripButtonSummary,
            this.toolStripButtonMerge});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(984, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.sfDataGridTransmittals);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(984, 535);
            this.splitContainer1.SplitterDistance = 328;
            this.splitContainer1.TabIndex = 2;
            // 
            // toolStripButtonMerge
            // 
            this.toolStripButtonMerge.Enabled = false;
            this.toolStripButtonMerge.Image = global::Transmittal.Properties.Resources.DataMerge;
            this.toolStripButtonMerge.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonMerge.Name = "toolStripButtonMerge";
            this.toolStripButtonMerge.Size = new System.Drawing.Size(127, 22);
            this.toolStripButtonMerge.Text = "Merge Transmittals";
            // 
            // toolStripButtonTransmittalReport
            // 
            this.toolStripButtonTransmittalReport.Enabled = false;
            this.toolStripButtonTransmittalReport.Image = global::Transmittal.Properties.Resources.Report;
            this.toolStripButtonTransmittalReport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonTransmittalReport.Name = "toolStripButtonTransmittalReport";
            this.toolStripButtonTransmittalReport.Size = new System.Drawing.Size(123, 22);
            this.toolStripButtonTransmittalReport.Text = "Transmittal Report";
            // 
            // toolStripButtonSummary
            // 
            this.toolStripButtonSummary.Image = global::Transmittal.Properties.Resources.Report;
            this.toolStripButtonSummary.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSummary.Name = "toolStripButtonSummary";
            this.toolStripButtonSummary.Size = new System.Drawing.Size(116, 22);
            this.toolStripButtonSummary.Text = "Summary Report";
            // 
            // sfDataGridTransmittals
            // 
            this.sfDataGridTransmittals.AccessibleName = "Table";
            this.sfDataGridTransmittals.AddNewRowPosition = Syncfusion.WinForms.DataGrid.Enums.RowPosition.Bottom;
            this.sfDataGridTransmittals.AllowDeleting = true;
            this.sfDataGridTransmittals.AllowFiltering = true;
            this.sfDataGridTransmittals.AutoGenerateColumns = false;
            this.sfDataGridTransmittals.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.LastColumnFill;
            this.sfDataGridTransmittals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sfDataGridTransmittals.Location = new System.Drawing.Point(0, 0);
            this.sfDataGridTransmittals.Name = "sfDataGridTransmittals";
            this.sfDataGridTransmittals.RowHeight = global::Transmittal.Properties.Settings.Default.GridRowHeight;
            this.sfDataGridTransmittals.SelectionMode = Syncfusion.WinForms.DataGrid.Enums.GridSelectionMode.Extended;
            this.sfDataGridTransmittals.Size = new System.Drawing.Size(328, 535);
            this.sfDataGridTransmittals.TabIndex = 0;
            this.sfDataGridTransmittals.Text = "sfDataGrid1";
            this.sfDataGridTransmittals.ValidationMode = Syncfusion.WinForms.DataGrid.Enums.GridValidationMode.InEdit;
            // 
            // tabControl1
            // 
            this.tabControl1.BackColor = global::Transmittal.Properties.Settings.Default.CommonFormBackColor;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = global::Transmittal.Properties.Settings.Default.CommonFormFont;
            this.tabControl1.ForeColor = global::Transmittal.Properties.Settings.Default.CommonFormForeColor;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(652, 535);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = global::Transmittal.Properties.Settings.Default.CommonFormBackColor;
            this.tabPage1.Controls.Add(this.sfDataGridTransmittalItems);
            this.tabPage1.Font = global::Transmittal.Properties.Settings.Default.CommonFormFont;
            this.tabPage1.ForeColor = global::Transmittal.Properties.Settings.Default.CommonFormForeColor;
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(644, 507);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Items";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.sfDataGridTransmittalDistribution);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(644, 507);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Distribution";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // sfDataGridTransmittalItems
            // 
            this.sfDataGridTransmittalItems.AccessibleName = "Table";
            this.sfDataGridTransmittalItems.AddNewRowPosition = Syncfusion.WinForms.DataGrid.Enums.RowPosition.Bottom;
            this.sfDataGridTransmittalItems.AllowDeleting = true;
            this.sfDataGridTransmittalItems.AllowFiltering = true;
            this.sfDataGridTransmittalItems.AutoGenerateColumns = false;
            this.sfDataGridTransmittalItems.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.LastColumnFill;
            this.sfDataGridTransmittalItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sfDataGridTransmittalItems.Location = new System.Drawing.Point(3, 3);
            this.sfDataGridTransmittalItems.Name = "sfDataGridTransmittalItems";
            this.sfDataGridTransmittalItems.RowHeight = global::Transmittal.Properties.Settings.Default.GridRowHeight;
            this.sfDataGridTransmittalItems.Size = new System.Drawing.Size(638, 501);
            this.sfDataGridTransmittalItems.TabIndex = 1;
            this.sfDataGridTransmittalItems.Text = "sfDataGrid1";
            this.sfDataGridTransmittalItems.ValidationMode = Syncfusion.WinForms.DataGrid.Enums.GridValidationMode.InEdit;
            // 
            // sfDataGridTransmittalDistribution
            // 
            this.sfDataGridTransmittalDistribution.AccessibleName = "Table";
            this.sfDataGridTransmittalDistribution.AddNewRowPosition = Syncfusion.WinForms.DataGrid.Enums.RowPosition.Bottom;
            this.sfDataGridTransmittalDistribution.AllowDeleting = true;
            this.sfDataGridTransmittalDistribution.AllowFiltering = true;
            this.sfDataGridTransmittalDistribution.AutoGenerateColumns = false;
            this.sfDataGridTransmittalDistribution.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.AllCellsWithLastColumnFill;
            this.sfDataGridTransmittalDistribution.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sfDataGridTransmittalDistribution.Location = new System.Drawing.Point(3, 3);
            this.sfDataGridTransmittalDistribution.Name = "sfDataGridTransmittalDistribution";
            this.sfDataGridTransmittalDistribution.RowHeight = global::Transmittal.Properties.Settings.Default.GridRowHeight;
            this.sfDataGridTransmittalDistribution.Size = new System.Drawing.Size(638, 501);
            this.sfDataGridTransmittalDistribution.TabIndex = 2;
            this.sfDataGridTransmittalDistribution.Text = "sfDataGrid1";
            this.sfDataGridTransmittalDistribution.ValidationMode = Syncfusion.WinForms.DataGrid.Enums.GridValidationMode.InEdit;
            // 
            // FormArchive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = global::Transmittal.Properties.Settings.Default.CommonFormBackColor;
            this.ClientSize = new System.Drawing.Size(984, 582);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Font = global::Transmittal.Properties.Settings.Default.CommonFormFont;
            this.ForeColor = global::Transmittal.Properties.Settings.Default.CommonFormForeColor;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormArchive";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Archive";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sfDataGridTransmittals)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sfDataGridTransmittalItems)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sfDataGridTransmittalDistribution)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private StatusStrip statusStrip1;
    private ToolStrip toolStrip1;
    private ToolStripButton toolStripButtonTransmittalReport;
    private ToolStripButton toolStripButtonSummary;
    private ToolStripButton toolStripButtonMerge;
    private SplitContainer splitContainer1;
    private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGridTransmittals;
    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGridTransmittalItems;
    private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGridTransmittalDistribution;
}