namespace Transmittal.Forms;

partial class FormDirectory
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonAdd = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonExportVcard = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonReport = new System.Windows.Forms.ToolStripButton();
            this.sfDataGrid1 = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sfDataGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAdd,
            this.toolStripButtonRemove,
            this.toolStripButtonExportVcard,
            this.toolStripButtonReport});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(895, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonAdd
            // 
            this.toolStripButtonAdd.Image = global::Transmittal.Properties.Resources.Add;
            this.toolStripButtonAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAdd.Name = "toolStripButtonAdd";
            this.toolStripButtonAdd.Size = new System.Drawing.Size(94, 22);
            this.toolStripButtonAdd.Text = "Add Contact";
            // 
            // toolStripButtonRemove
            // 
            this.toolStripButtonRemove.Enabled = false;
            this.toolStripButtonRemove.Image = global::Transmittal.Properties.Resources.Delete;
            this.toolStripButtonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRemove.Name = "toolStripButtonRemove";
            this.toolStripButtonRemove.Size = new System.Drawing.Size(115, 22);
            this.toolStripButtonRemove.Text = "Remove Contact";
            // 
            // toolStripButtonExportVcard
            // 
            this.toolStripButtonExportVcard.Enabled = false;
            this.toolStripButtonExportVcard.Image = global::Transmittal.Properties.Resources.ContactDetails;
            this.toolStripButtonExportVcard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonExportVcard.Name = "toolStripButtonExportVcard";
            this.toolStripButtonExportVcard.Size = new System.Drawing.Size(95, 22);
            this.toolStripButtonExportVcard.Text = "Export vCard";
            // 
            // toolStripButtonReport
            // 
            this.toolStripButtonReport.Image = global::Transmittal.Properties.Resources.Report;
            this.toolStripButtonReport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonReport.Name = "toolStripButtonReport";
            this.toolStripButtonReport.Size = new System.Drawing.Size(113, 22);
            this.toolStripButtonReport.Text = "Directory Report";
            // 
            // sfDataGrid1
            // 
            this.sfDataGrid1.AccessibleName = "Table";
            this.sfDataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sfDataGrid1.Location = new System.Drawing.Point(0, 25);
            this.sfDataGrid1.Name = "sfDataGrid1";
            this.sfDataGrid1.Size = new System.Drawing.Size(895, 525);
            this.sfDataGrid1.TabIndex = 3;
            this.sfDataGrid1.Text = "sfDataGrid1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 550);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(895, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // FormDirectory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = global::Transmittal.Properties.Settings.Default.CommonFormBackColor;
            this.ClientSize = new System.Drawing.Size(895, 572);
            this.Controls.Add(this.sfDataGrid1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Font = global::Transmittal.Properties.Settings.Default.CommonFormFont;
            this.ForeColor = global::Transmittal.Properties.Settings.Default.CommonFormForeColor;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormDirectory";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Directory";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sfDataGrid1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion
    private ToolStrip toolStrip1;
    private ToolStripButton toolStripButtonAdd;
    private ToolStripButton toolStripButtonRemove;
    private ToolStripButton toolStripButtonExportVcard;
    private ToolStripButton toolStripButtonReport;
    private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGrid1;
    private StatusStrip statusStrip1;
}