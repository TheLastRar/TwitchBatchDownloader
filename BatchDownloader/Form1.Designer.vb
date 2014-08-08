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
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.SuspendLayout()
        '
        'inputName
        '
        Me.inputName.Location = New System.Drawing.Point(12, 25)
        Me.inputName.Name = "inputName"
        Me.inputName.Size = New System.Drawing.Size(632, 20)
        Me.inputName.TabIndex = 0
        Me.inputName.Text = "monotonetim"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(70, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Twitch Name"
        '
        'ButtonGoStop
        '
        Me.ButtonGoStop.Location = New System.Drawing.Point(12, 51)
        Me.ButtonGoStop.Name = "ButtonGoStop"
        Me.ButtonGoStop.Size = New System.Drawing.Size(632, 30)
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
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(12, 100)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(632, 23)
        Me.ProgressBar1.TabIndex = 4
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(656, 136)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.Stats)
        Me.Controls.Add(Me.ButtonGoStop)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.inputName)
        Me.Name = "Form1"
        Me.Text = "Form1.2 - Batch Twitch VOD Stream Downloader by @Air_Gamer"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents inputName As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ButtonGoStop As System.Windows.Forms.Button
    Friend WithEvents Stats As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar

End Class
