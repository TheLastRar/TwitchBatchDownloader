Imports System.Text.RegularExpressions

Public Class Form1

    Dim accessLock As New Object
    Dim Downloading As Boolean = False

    Private Sub Button_Click(sender As System.Object, e As System.EventArgs) Handles ButtonGoStop.Click
        If Downloading = False Then
            Dim DownloadThread As New Threading.Thread(AddressOf DownloadFile)
            Downloading = True
            DownloadThread.IsBackground = True
            DownloadThread.Start(inputName.Text)
            inputName.Enabled = False
            ButtonGoStop.Text = "Stop"
        Else
            StopThread()
            ButtonGoStop.Text = "Stopping"
            ButtonGoStop.Enabled = False
        End If
    End Sub

    Private Sub SetStatsText(ByVal text As String)
        If Stats.InvokeRequired Then
            Stats.Invoke(New Action(Of String)(AddressOf SetStatsText), text)
        Else
            Stats.Text = text
        End If
    End Sub

    Private Sub SetProgress(ByVal sender As Object, ByVal e As FileDownloadProgressChangedEventArgs)
        SetProgress2(e.ProgressPercentage)
    End Sub

    Private Sub SetProgress2(ByVal int As Integer)
        If Stats.InvokeRequired Then
            Stats.Invoke(New Action(Of Integer)(AddressOf SetProgress2), int)
        Else
            ProgressBar1.Value = int
        End If
    End Sub

    Private Sub EnableStuff(dummy As String)
        If Stats.InvokeRequired Then
            Stats.Invoke(New Action(Of String)(AddressOf EnableStuff), dummy)
        Else
            inputName.Enabled = True
            ButtonGoStop.Text = "Download"
            ButtonGoStop.Enabled = True
            Downloading = False
        End If
    End Sub

    Public Sub StopThread()
        SyncLock accessLock
            Downloading = False
        End SyncLock
    End Sub

    Private Sub DownloadFile(ChannelName As String)
        SetStatsText("Loading")
        If (System.IO.Directory.Exists("Temp")) Then
            System.IO.Directory.Delete("Temp", True)
        End If
        System.IO.Directory.CreateDirectory("Temp")

        Dim PageNumber As Integer = 1
        Dim Compleated As Boolean = False
        Do While Compleated = False
            SetStatsText("Downloading File List")
            Dim Index As Integer = 63

            Dim FileListDownloaded As Boolean = False
            Dim FileList As String() = Nothing
            Do While FileListDownloaded = False
                My.Computer.Network.DownloadFile("http://download.twitchapps.com/?stream=" & ChannelName & "&limit=100&page=" & PageNumber.ToString() & "&vodType=a", "Temp\Page.html")
                FileList = System.IO.File.ReadAllLines("Temp\Page.html")
                System.IO.File.Delete("Temp\Page.html")

                'If FileList(58) = "<hr/><div class=""alert alert-error"">Unable to access API.</div>" Then
                '    Threading.Thread.Sleep(5000)
                'Else
                FileListDownloaded = True
                'End If
            Loop

            If FileList(59).Contains("next") Then
                PageNumber += 1
            Else
                Compleated = True
            End If
            If FileList(Index) = "</div>" Then
                Compleated = True
                Exit Do
            End If

            Dim CompleatedPage As Boolean = False

            Do While CompleatedPage = False
                SetStatsText("Reading File List")
                Dim Title As String
                If FileList(Index).Contains("</td>") Then
                    Title = FileList(Index).Substring(4, FileList(Index).Length - (5 + 4)) 'need to detect multiline titles
                Else
                    Title = FileList(Index).Substring(4, FileList(Index).Length - 4)
                    Index += 1
                    Do Until FileList(Index).Contains("</td>")
                        Title += " " + FileList(Index)
                        Index += 1
                    Loop
                    Title += " " + FileList(Index).Substring(0, FileList(Index).Length - 5)
                End If
                Index += 1
                Dim Id As String = FileList(Index).Substring(4, FileList(Index).Length - (5 + 4))
                Index += 3
                Dim StreamDate As String = FileList(Index).Substring(4, FileList(Index).Length - (5 + 4))
                Index += 5

                Dim FolderName As String = StreamDate & "_" & Title

                FolderName = FolderName.Replace("\\", "_").Replace("/", "_").Replace("""", "_").Replace("*", "_").Replace(":", "_").Replace("?", "_").Replace("<", "_").Replace(">", "_").Replace("|", "_").Trim()
                Dim URLID As String = Id.Substring(1, Id.Length - 1)


                If (Not System.IO.File.Exists(FolderName & "\Done.Download")) Then
                    If (Not System.IO.Directory.Exists(FolderName)) Then
                        System.IO.Directory.CreateDirectory(FolderName)
                    End If
                    IO.File.WriteAllText(FolderName & "\StreamURL.txt", "http://www.twitch.tv/" & ChannelName & "/b/" & URLID)

                    SetStatsText("Downloading Part List")

                    Dim PartListDownloadDone As Boolean = False
                    Dim FilePartList As String() = Nothing
                    Do While PartListDownloadDone = False
                        My.Computer.Network.DownloadFile("http://download.twitchapps.com/listparts.php?id=" & Id, "Temp\StreamList.html")
                        FilePartList = System.IO.File.ReadAllLines("Temp\StreamList.html")
                        System.IO.File.Delete("Temp\StreamList.html")
                        If FilePartList(0) = "Error: Unable to access API" Then
                            Threading.Thread.Sleep(5000)
                        Else
                            PartListDownloadDone = True
                        End If
                    Loop

                    For PartNo As Integer = 0 To FilePartList.Count - 1
                        If (System.IO.File.Exists(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")) Then
                            System.IO.File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")
                        End If

                        SetStatsText("Downloading " & Title & " Part: " & (PartNo + 1))
                        Dim LineParts As String() = Regex.Split(FilePartList(PartNo), """")
                        Dim URL As String = LineParts(1)
                        If (Not System.IO.File.Exists(FolderName & "\Part " & (PartNo + 1).ToString() & ".flv")) Then
                            Threading.Thread.Sleep(500)

                            'check if stopping
                            SyncLock accessLock
                                If Downloading = False Then
                                    Compleated = True
                                    Exit Do
                                End If
                            End SyncLock

                            If StartDownload(URL, FolderName, PartNo) = False Then
                                Dim URLcomponents As String() = URL.Split("/")
                                Dim URL2 As String = "http://media-cdn.twitch.tv/" & URLcomponents(2).Substring(0, URLcomponents(2).Length - 10) & "/" & URLcomponents(3) & "/" & URLcomponents(4) & "/" & URLcomponents(5)
                                If StartDownload(URL2, FolderName, PartNo) = False Then
                                    System.IO.File.Create(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")
                                End If
                            End If
                        End If
                    Next
                    System.IO.File.Create(FolderName & "\Done.Download").Close()
                End If

                If FileList(Index) = "</div>" Then
                    CompleatedPage = True
                End If
            Loop
        Loop
        SetStatsText("Done")
        EnableStuff("t")
    End Sub

    Public Shared Function GetFileSize(url As String) As Long
        Using obj As New Net.WebClient()
            Using s As IO.Stream = obj.OpenRead(url)
                Return Long.Parse(obj.ResponseHeaders("Content-Length").ToString())
            End Using
        End Using
    End Function

    Public Function StartDownload(url As String, FolderName As String, PartNo As Integer) As Boolean
        Dim Downloader As New DownloadFileAsyncExtended()
        AddHandler Downloader.DownloadProgressChanged, AddressOf SetProgress
        Dim Tries As Integer = 0

        Do
            Tries += 1
            Downloader.DowloadFileAsync(url, "Temp\Part " & (PartNo + 1).ToString() & ".flv", Nothing)
            Do
                Threading.Thread.Sleep(500)
            Loop Until (Not IsNothing(Downloader.ResponseHeaders)) Or Downloader.IsBusy = False
        Loop Until (Not IsNothing(Downloader.ResponseHeaders)) Or Tries = 3

        If IsNothing(Downloader.ResponseHeaders) Then
            'System.IO.File.Create(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")
            Return False
        Else
            Dim Headers As System.Net.WebHeaderCollection = Downloader.ResponseHeaders
            Dim TargetSize As Long = Headers.GetValues("Content-Length")(0)

            Dim PartDownloaded As Boolean = False
            Do
                Do
                    Threading.Thread.Sleep(500)
                Loop Until Downloader.IsBusy = False

                If Not (TargetSize = My.Computer.FileSystem.GetFileInfo("Temp\Part " & (PartNo + 1).ToString() & ".flv").Length) Then
                    Downloader.ResumeAsync()
                Else
                    PartDownloaded = True
                End If
            Loop Until PartDownloaded = True

            System.IO.File.Move("Temp\Part " & (PartNo + 1).ToString() & ".flv", FolderName + "\Part " & (PartNo + 1).ToString() & ".flv")
            Return True
        End If
    End Function
End Class
