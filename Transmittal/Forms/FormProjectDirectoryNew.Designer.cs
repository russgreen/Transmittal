
namespace Transmittal.Forms
{
    partial class FormProjectDirectoryNew
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.OK_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxCompanies = new System.Windows.Forms.ComboBox();
            this.buttonAddCompany = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxContacts = new System.Windows.Forms.ComboBox();
            this.buttonAddContact = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.Cancel_button, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.OK_button, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(206, 108);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(146, 29);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // Cancel_button
            // 
            this.Cancel_button.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Cancel_button.Location = new System.Drawing.Point(76, 3);
            this.Cancel_button.Margin = new System.Windows.Forms.Padding(2);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(67, 23);
            this.Cancel_button.TabIndex = 1;
            this.Cancel_button.Text = "Cancel";
            this.Cancel_button.UseVisualStyleBackColor = true;
            this.Cancel_button.Click += new System.EventHandler(this.Cancel_button_Click);
            // 
            // OK_button
            // 
            this.OK_button.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.OK_button.Enabled = false;
            this.OK_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OK_button.Location = new System.Drawing.Point(3, 3);
            this.OK_button.Margin = new System.Windows.Forms.Padding(2);
            this.OK_button.Name = "OK_button";
            this.OK_button.Size = new System.Drawing.Size(67, 23);
            this.OK_button.TabIndex = 2;
            this.OK_button.Text = "OK";
            this.OK_button.UseVisualStyleBackColor = true;
            this.OK_button.Click += new System.EventHandler(this.OK_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Company";
            // 
            // comboBoxCompanies
            // 
            this.comboBoxCompanies.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxCompanies.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxCompanies.FormattingEnabled = true;
            this.comboBoxCompanies.Location = new System.Drawing.Point(16, 29);
            this.comboBoxCompanies.Name = "comboBoxCompanies";
            this.comboBoxCompanies.Size = new System.Drawing.Size(302, 21);
            this.comboBoxCompanies.TabIndex = 2;
            this.comboBoxCompanies.SelectedIndexChanged += new System.EventHandler(this.ComboBoxCompanies_SelectedIndexChanged);
            // 
            // buttonAddCompany
            // 
            this.buttonAddCompany.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAddCompany.Image = global::Transmittal.Properties.Resources.Add;
            this.buttonAddCompany.Location = new System.Drawing.Point(324, 26);
            this.buttonAddCompany.Name = "buttonAddCompany";
            this.buttonAddCompany.Size = new System.Drawing.Size(25, 25);
            this.buttonAddCompany.TabIndex = 3;
            this.buttonAddCompany.UseVisualStyleBackColor = true;
            this.buttonAddCompany.Click += new System.EventHandler(this.ButtonAddCompany_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Contact Person";
            // 
            // comboBoxContacts
            // 
            this.comboBoxContacts.FormattingEnabled = true;
            this.comboBoxContacts.Location = new System.Drawing.Point(16, 73);
            this.comboBoxContacts.Name = "comboBoxContacts";
            this.comboBoxContacts.Size = new System.Drawing.Size(302, 21);
            this.comboBoxContacts.TabIndex = 5;
            this.comboBoxContacts.SelectedIndexChanged += new System.EventHandler(this.ComboBoxContacts_SelectedIndexChanged);
            // 
            // buttonAddContact
            // 
            this.buttonAddContact.Enabled = false;
            this.buttonAddContact.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAddContact.Image = global::Transmittal.Properties.Resources.Add;
            this.buttonAddContact.Location = new System.Drawing.Point(324, 70);
            this.buttonAddContact.Name = "buttonAddContact";
            this.buttonAddContact.Size = new System.Drawing.Size(25, 25);
            this.buttonAddContact.TabIndex = 6;
            this.buttonAddContact.UseVisualStyleBackColor = true;
            this.buttonAddContact.Click += new System.EventHandler(this.ButtonAddContact_Click);
            // 
            // FormProjectDirectoryNew
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = global::Transmittal.Properties.Settings.Default.CommonFormBackColor;
            this.ClientSize = new System.Drawing.Size(363, 148);
            this.Controls.Add(this.comboBoxContacts);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonAddContact);
            this.Controls.Add(this.buttonAddCompany);
            this.Controls.Add(this.comboBoxCompanies);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = global::Transmittal.Properties.Settings.Default.CommonFormFont;
            this.ForeColor = global::Transmittal.Properties.Settings.Default.CommonFormForeColor;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormProjectDirectoryNew";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Project Directory Entry";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button Cancel_button;
        private System.Windows.Forms.Button OK_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxCompanies;
        private System.Windows.Forms.Button buttonAddCompany;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxContacts;
        private System.Windows.Forms.Button buttonAddContact;
    }
}