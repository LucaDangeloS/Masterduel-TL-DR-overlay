namespace Masterduel_TLDR_overlay;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.consoleLog = new System.Windows.Forms.TextBox();
        this.startButton = new System.Windows.Forms.Button();
        this.stopButton = new System.Windows.Forms.Button();
        this.logConsoleCheckBox = new System.Windows.Forms.CheckBox();
        this.SuspendLayout();
        // 
        // consoleLog
        // 
        this.consoleLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.consoleLog.Location = new System.Drawing.Point(12, 106);
        this.consoleLog.Margin = new System.Windows.Forms.Padding(10);
        this.consoleLog.Multiline = true;
        this.consoleLog.Name = "consoleLog";
        this.consoleLog.ReadOnly = true;
        this.consoleLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.consoleLog.Size = new System.Drawing.Size(360, 0);
        this.consoleLog.TabIndex = 5;
        this.consoleLog.Visible = false;
        this.consoleLog.TextChanged += new System.EventHandler(this.consoleLog_TextChanged);
        // 
        // startButton
        // 
        this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.startButton.Location = new System.Drawing.Point(301, 12);
        this.startButton.Margin = new System.Windows.Forms.Padding(10);
        this.startButton.Name = "startButton";
        this.startButton.Size = new System.Drawing.Size(75, 23);
        this.startButton.TabIndex = 6;
        this.startButton.Text = "Start";
        this.startButton.UseVisualStyleBackColor = true;
        this.startButton.Click += new System.EventHandler(this.StartLoop);
        // 
        // stopButton
        // 
        this.stopButton.Enabled = false;
        this.stopButton.Location = new System.Drawing.Point(12, 12);
        this.stopButton.Name = "stopButton";
        this.stopButton.Size = new System.Drawing.Size(75, 23);
        this.stopButton.TabIndex = 8;
        this.stopButton.Text = "Stop";
        this.stopButton.UseVisualStyleBackColor = true;
        this.stopButton.Click += new System.EventHandler(this.StopLoop);
        // 
        // logConsoleCheckBox
        // 
        this.logConsoleCheckBox.AutoSize = true;
        this.logConsoleCheckBox.Location = new System.Drawing.Point(12, 74);
        this.logConsoleCheckBox.Name = "logConsoleCheckBox";
        this.logConsoleCheckBox.Size = new System.Drawing.Size(92, 19);
        this.logConsoleCheckBox.TabIndex = 9;
        this.logConsoleCheckBox.Text = "Log Console";
        this.logConsoleCheckBox.UseVisualStyleBackColor = true;
        this.logConsoleCheckBox.CheckedChanged += new System.EventHandler(this.LogConsoleCheckBox_changed);
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(384, 111);
        this.Controls.Add(this.logConsoleCheckBox);
        this.Controls.Add(this.stopButton);
        this.Controls.Add(this.startButton);
        this.Controls.Add(this.consoleLog);
        this.MinimumSize = new System.Drawing.Size(400, 150);
        this.Name = "MainForm";
        this.Text = "TLDR Overlay";
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private TextBox consoleLog;
    private Button startButton;
    private Button stopButton;
    private CheckBox logConsoleCheckBox;
}
