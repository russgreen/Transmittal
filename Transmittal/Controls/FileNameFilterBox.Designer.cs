using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Transmittal.Controls
{
    public partial class FileNameFilterBox : System.Windows.Forms.UserControl
    {

        // UserControl overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is object)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this._TextBox1 = new System.Windows.Forms.TextBox();
            this._ComboBox1 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // _TextBox1
            // 
            this._TextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._TextBox1.Location = new System.Drawing.Point(0, 0);
            this._TextBox1.Name = "_TextBox1";
            this._TextBox1.Size = new System.Drawing.Size(380, 20);
            this._TextBox1.TabIndex = 0;
            this._TextBox1.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            // 
            // _ComboBox1
            // 
            this._ComboBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this._ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._ComboBox1.DropDownWidth = 100;
            this._ComboBox1.FormattingEnabled = true;
            this._ComboBox1.Items.AddRange(new object[] {
            "<ProjNo>",
            "<ProjId>",
            "<ProjName>",
            "<SheetNo>",
            "<SheetName>",
            "<Rev>",
            "<DateYY>",
            "<DateYYYY>",
            "<DateMM>",
            "<DateDD>",
            "<Originator>",
            "<Volume>",
            "<Level>",
            "<Type>",
            "<Role>",
            "<Status>",
            "<StatusDescription>"});
            this._ComboBox1.Location = new System.Drawing.Point(380, 0);
            this._ComboBox1.Name = "_ComboBox1";
            this._ComboBox1.Size = new System.Drawing.Size(20, 21);
            this._ComboBox1.TabIndex = 1;
            this._ComboBox1.SelectionChangeCommitted += new System.EventHandler(this.ComboBox1_SelectionChangeCommitted);
            // 
            // FileNameFilterBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._ComboBox1);
            this.Controls.Add(this._TextBox1);
            this.Name = "FileNameFilterBox";
            this.Size = new System.Drawing.Size(400, 21);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TextBox _TextBox1;

        internal System.Windows.Forms.TextBox TextBox1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TextBox1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TextBox1 != null)
                {
                    _TextBox1.TextChanged -= TextBox1_TextChanged;
                }

                _TextBox1 = value;
                if (_TextBox1 != null)
                {
                    _TextBox1.TextChanged += TextBox1_TextChanged;
                }
            }
        }

        private System.Windows.Forms.ComboBox _ComboBox1;

        internal System.Windows.Forms.ComboBox ComboBox1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ComboBox1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ComboBox1 != null)
                {
                    _ComboBox1.SelectionChangeCommitted -= ComboBox1_SelectionChangeCommitted;
                }

                _ComboBox1 = value;
                if (_ComboBox1 != null)
                {
                    _ComboBox1.SelectionChangeCommitted += ComboBox1_SelectionChangeCommitted;
                }
            }
        }
    }
}