<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.inputName = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ButtonGoStop = New System.Windows.Forms.Button()
        Me.Stats = New System.Windows.Forms.Label()
        Me.ProgressBarTop = New System.Windows.Forms.ProgressBar()
        Me.ButtonVerify = New System.Windows.Forms.Button()
        Me.TotalProgress = New System.Windows.Forms.ProgressBar()
        Me.ProgressBarMid = New System.Windows.Forms.ProgressBar()
        Me.PBTopLabel = New System.Windows.Forms.Label()
        Me.PBMidLabel = New System.Windows.Forms.Label()
        Me.TPLabel = New System.Windows.Forms.Label()
        Me.MergeBox = New System.Windows.Forms.ComboBox()
        Me.CopyToBox = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'inputName
        '
        Me.inputName.Location = New System.Drawing.Point(12, 25)
        Me.inputName.Name = "inputName"
        Me.inputName.Size = New System.Drawing.Size(309, 20)
        Me.inputName.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(135, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Twitch Name or Video Link"
        '
        'ButtonGoStop
        '
        Me.ButtonGoStop.Location = New System.Drawing.Point(12, 51)
        Me.ButtonGoStop.Name = "ButtonGoStop"
        Me.ButtonGoStop.Size = New System.Drawing.Size(309, 30)
        Me.ButtonGoStop.TabIndex = 2
        Me.ButtonGoStop.Text = "Download"
        Me.ButtonGoStop.UseVisualStyleBackColor = True
        '
        'Stats
        '
        Me.Stats.AutoSize = True
        Me.Stats.Location = New System.Drawing.Point(12, 84)
        Me.Stats.Name = "Stats"
        Me.Stats.Size = New System.Drawing.Size(43, 13)
        Me.Stats.TabIndex = 3
        Me.Stats.Text = "Waiting"
        '
        'ProgressBarTop
        '
        Me.ProgressBarTop.Location = New System.Drawing.Point(128, 100)
        Me.ProgressBarTop.Name = "ProgressBarTop"
        Me.ProgressBarTop.Size = New System.Drawing.Size(516, 23)
        Me.ProgressBarTop.TabIndex = 4
        '
        'ButtonVerify
        '
        Me.ButtonVerify.Location = New System.Drawing.Point(335, 51)
        Me.ButtonVerify.Name = "ButtonVerify"
        Me.ButtonVerify.Size = New System.Drawing.Size(309, 30)
        Me.ButtonVerify.TabIndex = 5
        Me.ButtonVerify.Text = "Verify"
        Me.ButtonVerify.UseVisualStyleBackColor = True
        '
        'TotalProgress
        '
        Me.TotalProgress.Location = New System.Drawing.Point(128, 158)
        Me.TotalProgress.Name = "TotalProgress"
        Me.TotalProgress.Size = New System.Drawing.Size(516, 23)
        Me.TotalProgress.TabIndex = 6
        '
        'ProgressBarMid
        '
        Me.ProgressBarMid.Location = New System.Drawing.Point(128, 129)
        Me.ProgressBarMid.Name = "ProgressBarMid"
        Me.ProgressBarMid.Size = New System.Drawing.Size(516, 23)
        Me.ProgressBarMid.TabIndex = 7
        '
        'PBTopLabel
        '
        Me.PBTopLabel.AutoSize = True
        Me.PBTopLabel.Location = New System.Drawing.Point(12, 105)
        Me.PBTopLabel.Name = "PBTopLabel"
        Me.PBTopLabel.Size = New System.Drawing.Size(27, 13)
        Me.PBTopLabel.TabIndex = 8
        Me.PBTopLabel.Text = "N/A"
        '
        'PBMidLabel
        '
        Me.PBMidLabel.AutoSize = True
        Me.PBMidLabel.Location = New System.Drawing.Point(12, 134)
        Me.PBMidLabel.Name = "PBMidLabel"
        Me.PBMidLabel.Size = New System.Drawing.Size(27, 13)
        Me.PBMidLabel.TabIndex = 9
        Me.PBMidLabel.Text = "N/A"
        '
        'TPLabel
        '
        Me.TPLabel.AutoSize = True
        Me.TPLabel.Location = New System.Drawing.Point(12, 163)
        Me.TPLabel.Name = "TPLabel"
        Me.TPLabel.Size = New System.Drawing.Size(31, 13)
        Me.TPLabel.TabIndex = 10
        Me.TPLabel.Text = "Total"
        '
        'MergeBox
        '
        Me.MergeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.MergeBox.FormattingEnabled = True
        Me.MergeBox.Items.AddRange(New Object() {"Yes", "No"})
        Me.MergeBox.Location = New System.Drawing.Point(335, 25)
        Me.MergeBox.Name = "MergeBox"
        Me.MergeBox.Size = New System.Drawing.Size(148, 21)
        Me.MergeBox.TabIndex = 11
        '
        'CopyToBox
        '
        Me.CopyToBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CopyToBox.FormattingEnabled = True
        Me.CopyToBox.Items.AddRange(New Object() {"Just Merge", "Copy to MKV", "Copy to MP4"})
        Me.CopyToBox.Location = New System.Drawing.Point(496, 25)
        Me.CopyToBox.Name = "CopyToBox"
        Me.CopyToBox.Size = New System.Drawing.Size(148, 21)
        Me.CopyToBox.TabIndex = 12
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(332, 9)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(68, 13)
        Me.Label2.TabIndex = 13
        Me.Label2.Text = "Merge (Beta)"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(493, 9)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(47, 13)
        Me.Label3.TabIndex = 14
        Me.Label3.Text = "Copy To"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(656, 197)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.CopyToBox)
        Me.Controls.Add(Me.MergeBox)
        Me.Controls.Add(Me.TPLabel)
        Me.Controls.Add(Me.PBMidLabel)
        Me.Controls.Add(Me.PBTopLabel)
        Me.Controls.Add(Me.ProgressBarMid)
        Me.Controls.Add(Me.TotalProgress)
        Me.Controls.Add(Me.ButtonVerify)
        Me.Controls.Add(Me.ProgressBarTop)
        Me.Controls.Add(Me.Stats)
        Me.Controls.Add(Me.ButtonGoStop)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.inputName)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.Text = "Form2.0 - Twitch VOD Stream Downloader by @Air_Gamer"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents inputName As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ButtonGoStop As System.Windows.Forms.Button
    Friend WithEvents Stats As System.Windows.Forms.Label
    Friend WithEvents ProgressBarTop As System.Windows.Forms.ProgressBar
    Friend WithEvents ButtonVerify As System.Windows.Forms.Button
    Friend WithEvents TotalProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents ProgressBarMid As System.Windows.Forms.ProgressBar
    Friend WithEvents PBTopLabel As System.Windows.Forms.Label
    Friend WithEvents PBMidLabel As System.Windows.Forms.Label
    Friend WithEvents TPLabel As System.Windows.Forms.Label
    Friend WithEvents MergeBox As System.Windows.Forms.ComboBox
    Friend WithEvents CopyToBox As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label

End Class
