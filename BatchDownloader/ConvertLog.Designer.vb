<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class VideoConvertLogger
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
        Me.NormLabel = New System.Windows.Forms.Label()
        Me.ListBoxPlaceholder = New System.Windows.Forms.ListBox()
        Me.AlertLabel = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'NormLabel
        '
        Me.NormLabel.AutoSize = True
        Me.NormLabel.Location = New System.Drawing.Point(12, 14)
        Me.NormLabel.Name = "NormLabel"
        Me.NormLabel.Size = New System.Drawing.Size(52, 13)
        Me.NormLabel.TabIndex = 0
        Me.NormLabel.Text = "Waiting..."
        '
        'ListBoxPlaceholder
        '
        Me.ListBoxPlaceholder.FormattingEnabled = True
        Me.ListBoxPlaceholder.Location = New System.Drawing.Point(13, 30)
        Me.ListBoxPlaceholder.Name = "ListBoxPlaceholder"
        Me.ListBoxPlaceholder.Size = New System.Drawing.Size(631, 212)
        Me.ListBoxPlaceholder.TabIndex = 1
        '
        'AlertLabel
        '
        Me.AlertLabel.AutoSize = True
        Me.AlertLabel.ForeColor = System.Drawing.Color.Red
        Me.AlertLabel.Location = New System.Drawing.Point(326, 14)
        Me.AlertLabel.Name = "AlertLabel"
        Me.AlertLabel.Size = New System.Drawing.Size(55, 13)
        Me.AlertLabel.TabIndex = 3
        Me.AlertLabel.Text = " Waiting..."
        '
        'VideoConvertLogger
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(656, 258)
        Me.Controls.Add(Me.AlertLabel)
        Me.Controls.Add(Me.ListBoxPlaceholder)
        Me.Controls.Add(Me.NormLabel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "VideoConvertLogger"
        Me.Text = "ConvertLog"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents NormLabel As System.Windows.Forms.Label
    Friend WithEvents ListBoxPlaceholder As System.Windows.Forms.ListBox
    Friend WithEvents AlertLabel As System.Windows.Forms.Label
End Class
