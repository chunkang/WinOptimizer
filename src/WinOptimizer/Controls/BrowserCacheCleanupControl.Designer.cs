namespace WinOptimizer.Controls;

partial class BrowserCacheCleanupControl
{
    private System.ComponentModel.IContainer components = null;
    private CheckedListBox checkedListBox;
    private Button btnScan;
    private Button btnClean;
    private Label lblDescription;
    private Label lblNote;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        checkedListBox = new CheckedListBox();
        btnScan = new Button();
        btnClean = new Button();
        lblDescription = new Label();
        lblNote = new Label();

        // lblDescription
        lblDescription.Text = "Select browsers to clean cache. Items marked [RUNNING] must be closed first.";
        lblDescription.Location = new Point(0, 0);
        lblDescription.Size = new Size(640, 25);

        // checkedListBox
        checkedListBox.Location = new Point(0, 30);
        checkedListBox.Size = new Size(640, 310);
        checkedListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        checkedListBox.CheckOnClick = true;

        // lblNote
        lblNote.Text = "Tip: Close all browsers before cleaning for best results. Bookmarks and passwords are not affected.";
        lblNote.ForeColor = System.Drawing.Color.DarkBlue;
        lblNote.Location = new Point(0, 350);
        lblNote.Size = new Size(640, 25);
        lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

        // btnScan
        btnScan.Text = "Rescan";
        btnScan.Location = new Point(430, 390);
        btnScan.Size = new Size(90, 30);
        btnScan.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnScan.Click += BtnScan_Click;

        // btnClean
        btnClean.Text = "Clean Selected";
        btnClean.Location = new Point(530, 390);
        btnClean.Size = new Size(110, 30);
        btnClean.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClean.Click += BtnClean_Click;

        // BrowserCacheCleanupControl
        Controls.Add(lblDescription);
        Controls.Add(checkedListBox);
        Controls.Add(lblNote);
        Controls.Add(btnScan);
        Controls.Add(btnClean);
    }
}
