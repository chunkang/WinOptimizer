namespace WinOptimizer.Controls;

partial class SoftwareDetectionControl
{
    private System.ComponentModel.IContainer components = null;
    private Button btnScan;
    private Button btnSelectAll;
    private Button btnDeselectAll;
    private Button btnUninstall;
    private ListView listView;
    private Label lblCount;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        btnScan = new Button();
        btnSelectAll = new Button();
        btnDeselectAll = new Button();
        btnUninstall = new Button();
        listView = new ListView();
        lblCount = new Label();

        // btnScan
        btnScan.Text = "Scan Now";
        btnScan.Location = new Point(0, 0);
        btnScan.Size = new Size(100, 30);
        btnScan.Click += BtnScan_Click;

        // btnSelectAll
        btnSelectAll.Text = "Select All";
        btnSelectAll.Location = new Point(440, 0);
        btnSelectAll.Size = new Size(90, 30);
        btnSelectAll.Click += BtnSelectAll_Click;

        // btnDeselectAll
        btnDeselectAll.Text = "Deselect All";
        btnDeselectAll.Location = new Point(540, 0);
        btnDeselectAll.Size = new Size(100, 30);
        btnDeselectAll.Click += BtnDeselectAll_Click;

        // listView
        listView.View = View.Details;
        listView.CheckBoxes = true;
        listView.FullRowSelect = true;
        listView.GridLines = true;
        listView.Location = new Point(0, 40);
        listView.Size = new Size(640, 340);
        listView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listView.Columns.Add("Name", 200);
        listView.Columns.Add("Publisher", 150);
        listView.Columns.Add("Version", 80);
        listView.Columns.Add("Location", 200);

        // lblCount
        lblCount.Text = "Click 'Scan Now' to detect banking/security software";
        lblCount.Location = new Point(0, 390);
        lblCount.Size = new Size(400, 25);
        lblCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

        // btnUninstall
        btnUninstall.Text = "Uninstall Selected";
        btnUninstall.Location = new Point(500, 390);
        btnUninstall.Size = new Size(140, 30);
        btnUninstall.Enabled = false;
        btnUninstall.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnUninstall.Click += BtnUninstall_Click;

        // SoftwareDetectionControl
        Controls.Add(btnScan);
        Controls.Add(btnSelectAll);
        Controls.Add(btnDeselectAll);
        Controls.Add(listView);
        Controls.Add(lblCount);
        Controls.Add(btnUninstall);
    }
}
