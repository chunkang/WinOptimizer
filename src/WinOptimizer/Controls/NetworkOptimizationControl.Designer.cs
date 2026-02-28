namespace WinOptimizer.Controls;

partial class NetworkOptimizationControl
{
    private System.ComponentModel.IContainer components = null;
    private CheckedListBox checkedListBox;
    private Button btnApply;
    private Button btnRevert;
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
        btnApply = new Button();
        btnRevert = new Button();
        lblDescription = new Label();
        lblNote = new Label();

        // lblDescription
        lblDescription.Text = "Select network optimizations to apply or revert. Items marked '(Applied)' are currently active.";
        lblDescription.Location = new Point(0, 0);
        lblDescription.Size = new Size(640, 25);

        // checkedListBox
        checkedListBox.Location = new Point(0, 30);
        checkedListBox.Size = new Size(640, 310);
        checkedListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        checkedListBox.CheckOnClick = true;

        // lblNote
        lblNote.Text = "Note: A system reboot may be required for network changes to take effect.";
        lblNote.ForeColor = System.Drawing.Color.DarkRed;
        lblNote.Location = new Point(0, 350);
        lblNote.Size = new Size(640, 25);
        lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

        // btnApply
        btnApply.Text = "Apply Selected";
        btnApply.Location = new Point(380, 390);
        btnApply.Size = new Size(120, 30);
        btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnApply.Click += BtnApply_Click;

        // btnRevert
        btnRevert.Text = "Revert Selected";
        btnRevert.Location = new Point(510, 390);
        btnRevert.Size = new Size(130, 30);
        btnRevert.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnRevert.Click += BtnRevert_Click;

        // NetworkOptimizationControl
        Controls.Add(lblDescription);
        Controls.Add(checkedListBox);
        Controls.Add(lblNote);
        Controls.Add(btnApply);
        Controls.Add(btnRevert);
    }
}
