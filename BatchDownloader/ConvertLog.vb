Public Class VideoConvertLogger

    Private WithEvents MergeManager As New VideoMerger()
    Private WithEvents ListBox1 As New LogBox()
    Private LogAutoscroll As Boolean = True
    Private ScrollFreeHeight As Integer = 0

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Private Sub ConvertLog_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        ''replace listbox with logbox
        ListBox1.Location = ListBoxPlaceholder.Location
        ListBox1.Size = ListBoxPlaceholder.Size
        Me.Controls.Remove(ListBoxPlaceholder)
        ListBoxPlaceholder.Dispose()
        Me.Controls.Add(ListBox1)

        ScrollFreeHeight = ListBox1.Height \ ListBox1.ItemHeight
    End Sub

    Private Const CP_NOCLOSE_BUTTON As Integer = &H200
    Protected Overloads Overrides ReadOnly Property CreateParams() As CreateParams ''Hide close button
        Get
            Dim myCp As CreateParams = MyBase.CreateParams
            myCp.ClassStyle = myCp.ClassStyle Or CP_NOCLOSE_BUTTON
            Return myCp
        End Get
    End Property

    Protected Sub ManageAutoScrollFromScroll(sender As System.Object, e As ScrollEventArgs) Handles ListBox1.Scrolled
        'Somewhat wonky
        If e.NewValue >= (ListBox1.Items.Count() - ScrollFreeHeight - 1) - 1 Then
            LogAutoscroll = True
        Else
            LogAutoscroll = False
        End If
    End Sub

    Protected Sub ManageAutoScrollFromMouseDown(sender As System.Object, e As System.EventArgs) Handles ListBox1.MouseDown
        LogAutoscroll = False
    End Sub

    Public Sub StartConvert(FolderName As String, SourceFormat As String, TargetFormat As Format)
        SetAlertText("")
        SetNormText("Checking Downloaded files")
        If (Not System.IO.File.Exists(FolderName & "\Done.Download")) Then
            SetNormText("Error")
            SetAlertText("Download not compleated")
            Return 'Download Not Compleated
        End If

        If (System.IO.File.Exists(FolderName & "\Done.Convert")) Then
            SetNormText("VOD Already Converted")
            Return 'Convert already Done
        End If

        Dim Files As String() = IO.Directory.GetFiles(FolderName)

        LogConverision("Hello World")

        Dim Muted As Boolean = False
        Dim MutedFiles As New List(Of String)
        For Each fileName As String In Files
            If fileName.EndsWith("_Failed") Then
                SetNormText("Error")
                SetAlertText("VOD has missing files")
                Return 'Download Failed
            End If
            If fileName.EndsWith("_Muted") Then
                'Return 'Download has Muted Files
                Muted = True
                SetAlertText("VOD has muted content")
                LogConverision("Warning, Muted File: " + fileName)
                MutedFiles.Add(IO.Path.GetFileName(fileName))
            End If
        Next

        Dim SourceFiles As New List(Of String)
        For Each fileName As String In Files
            If fileName.EndsWith(SourceFormat) Then
                If Not IO.Path.GetFileNameWithoutExtension(fileName) = "Merged" Then
                    SourceFiles.Add(IO.Path.GetFileName(fileName))
                End If
            End If
        Next
        SourceFiles.Sort(New AlphanumComparator())
        Dim FileDirectory As String = IO.Directory.GetCurrentDirectory & "\" & FolderName

        SetNormText("Converting")
        Select Case SourceFormat
            Case Is = ".ts"
                MergeManager.AdjustAnalysisParams(SourceFiles, FileDirectory, MutedFiles)
                Select Case TargetFormat
                    Case Format.AsSource
                        MergeManager.JoinTSToMPEGTS(SourceFiles, FileDirectory)
                    Case Format.MKV
                        MergeManager.JoinTSToMKV(SourceFiles, FileDirectory)
                    Case Format.MP4
                        MergeManager.JoinTSToMP4(SourceFiles, FileDirectory)
                End Select
            Case Is = ".flv"
                If Muted = False Then
                    Select Case TargetFormat
                        Case Format.AsSource
                            MergeManager.JoinFLVToFLV(SourceFiles, FileDirectory)
                        Case Format.MKV
                            MergeManager.JoinFLVToMKV(SourceFiles, FileDirectory)
                        Case Format.MP4
                            MergeManager.JoinFLVToMP4_FFMPEG(SourceFiles, FileDirectory)
                    End Select
                Else
                    SetNormText("Conversion of muted FLV files not supported")
                End If
        End Select
        SetNormText("Done")
        System.IO.File.Create(FolderName & "\Done.Convert").Close()
    End Sub

    Private Sub LogConverision(str As String) Handles MergeManager.ProcOutput
        If ListBox1.InvokeRequired Then
            ListBox1.Invoke(New Action(Of String)(AddressOf LogConverision), str)
        Else
            ListBox1.Items.Add(str)
            Debug.Print(str)
            If LogAutoscroll Then
                ListBox1.SelectedIndex = (ListBox1.Items.Count() - 1)
                ListBox1.SelectedIndex = -1
            End If
        End If
    End Sub

    Private Sub SetNormText(str As String) Handles MergeManager.ManOutput
        If NormLabel.InvokeRequired Then
            NormLabel.Invoke(New Action(Of String)(AddressOf SetNormText), str)
        Else
            NormLabel.Text = str
        End If
    End Sub
    Private Sub SetAlertText(str As String)
        If AlertLabel.InvokeRequired Then
            AlertLabel.Invoke(New Action(Of String)(AddressOf SetAlertText), str)
        Else
            AlertLabel.Text = str
        End If
    End Sub

    Public Sub MtClose()
        If Me.InvokeRequired Then
            Me.Invoke(New Action(AddressOf MtClose))
        Else
            Me.Close()
        End If
    End Sub

End Class