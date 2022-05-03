using System.ComponentModel;

namespace Transmittal.Controls;

public partial class FileNameFilterBox : INotifyPropertyChanged
{
    public FileNameFilterBox()
    {
        InitializeComponent();
        _TextBox1.Name = "TextBox1";
        _ComboBox1.Name = "ComboBox1";
    }

    private string _fileNameFilter;
    private string _defaultFileNameFilter = "<ProjId>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>";
    private bool _isValid = true;

    public string FileNameFilter
    {
        get
        {
            return _fileNameFilter;
        }

        set
        {
            _fileNameFilter = value;
            // Call OnPropertyChanged whenever the property is updated
            OnPropertyChanged("FileNameFilter");
        }
    }

    public string DefaultFileNameFilter
    {
        get
        {
            return _defaultFileNameFilter;
        }

        set
        {
            _defaultFileNameFilter = value;
        }
    }

    public bool IsValid
    {
        get
        {
            return _isValid;
        }
    }

    public event IsValidChangedEventHandler IsValidChanged;

    public delegate void IsValidChangedEventHandler();

    public event PropertyChangedEventHandler PropertyChanged;

    // Create the OnPropertyChanged method to raise the event
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        switch (name ?? "")
        {
            case "FileNameFilter":
                {
                    TextBox1.Text = _fileNameFilter;
                    break;
                }
        }
    }

    private void ComboBox1_SelectionChangeCommitted(object sender, EventArgs e)
    {
        TextBox1.Text = TextBox1.Text + ComboBox1.SelectedItem.ToString();
        _fileNameFilter = TextBox1.Text;
        TextBox1.Focus();
        TextBox1.Select(TextBox1.Text.Length, TextBox1.Text.Length);
    }

    private void TextBox1_TextChanged(object sender, EventArgs e)
    {
        if ((TextBox1.Text ?? "") == (string.Empty ?? ""))
        {
            _isValid = false;
            TextBox1.BackColor = System.Drawing.Color.Red;
            // Me.TextBox1.Text = m_DefaultFileNameFilter
            IsValidChanged?.Invoke();
        }
        else
        {
            _isValid = true;
            TextBox1.BackColor = System.Drawing.Color.White;
            IsValidChanged?.Invoke();
        }

        _fileNameFilter = TextBox1.Text;
    }
}
