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
            ButtonVerify.Enabled = False
        Else
            StopThread()
            ButtonGoStop.Text = "Stopping"
            ButtonGoStop.Enabled = False
        End If
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles ButtonVerify.Click
        'Get a list of folders and delete Download.Done in all of them
        Dim DirList As IEnumerable(Of String) = IO.Directory.EnumerateDirectories(IO.Directory.GetCurrentDirectory)
        For Each Dir As String In DirList
            If System.IO.File.Exists(Dir & "\Done.Download") Then
                System.IO.File.Delete(Dir & "\Done.Download")
            End If
        Next
        If IO.File.Exists("Log.txt") Then 'reset log
            IO.File.Delete("Log.txt")
        End If
        Button_Click(Nothing, Nothing)
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
            ButtonVerify.Enabled = True
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
                        SetProgress2(0)
                        If (System.IO.File.Exists(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")) Then
                            System.IO.File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")
                        End If
                        If (System.IO.File.Exists(FolderName + "\Part " & (PartNo + 1).ToString() & "_Muted")) Then
                            System.IO.File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & "_Muted")
                        End If


                        Dim LineParts As String() = Regex.Split(FilePartList(PartNo), """")
                        Dim URL As String = LineParts(1)
                        Dim URLcomponents As String() = URL.Split("/")
                        Dim URL2 As String = "http://media-cdn.twitch.tv/" & URLcomponents(2).Substring(0, URLcomponents(2).Length - 10) & "/" & URLcomponents(3) & "/" & URLcomponents(4) & "/" & URLcomponents(5)

                        'check if stopping
                        SyncLock accessLock
                            If Downloading = False Then
                                Compleated = True
                                Exit Do
                            End If
                        End SyncLock

                        If (Not System.IO.File.Exists(FolderName & "\Part " & (PartNo + 1).ToString() & ".flv")) Then
                            SetStatsText("Downloading " & Title & " Part: " & (PartNo + 1))

                            StartDownloadWrapper(URL, URL2, Title, FolderName, PartNo)
                        Else
                            SetStatsText("Verifying " & Title & " Part: " & (PartNo + 1))
                            Dim ExpectedSize As Long = GetFileSize(URL)
                            If ExpectedSize = -1 Then
                                ExpectedSize = GetFileSize(URL2)
                            End If
                            If ExpectedSize > 0 Then
                                If Not (ExpectedSize <= My.Computer.FileSystem.GetFileInfo(FolderName + "\Part " & (PartNo + 1).ToString() & ".flv").Length) Then
                                    '<= works around muted VOD files if they are muted after download (has this happend yet?)
                                    SetStatsText("Redownloading " & Title & " Part: " & (PartNo + 1))
                                    IO.File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & ".flv")
                                    StartDownloadWrapper(URL, URL2, Title, FolderName, PartNo)
                                Else
                                    SetStatsText("File is expected size")
                                    If Not (ExpectedSize = My.Computer.FileSystem.GetFileInfo(FolderName + "\Part " & (PartNo + 1).ToString() & ".flv").Length) Then
                                        LogToFile(FolderName, PartNo, "Downloaded file larger then online version")
                                    End If
                                    Threading.Thread.Sleep(500)
                                    CheckMuted(FolderName, PartNo)
                                End If
                            Else
                                SetStatsText("Could not connect")
                                LogToFile(FolderName, PartNo, "Couldn't Verify File")
                                Threading.Thread.Sleep(500)
                            End If
                        End If
                    Next
                    System.IO.File.Create(FolderName & "\Done.Download").Close()
                End If

                If FileList(Index).Contains("</div>") Then
                    CompleatedPage = True
                End If
            Loop
        Loop
        SetStatsText("Done")
        EnableStuff("t")
    End Sub

    Public Shared Function GetFileSize(url As String) As Long
        Dim Tries As Integer = 1
        Do
            Try
                Dim req As System.Net.WebRequest = System.Net.HttpWebRequest.Create(url)
                req.Method = "HEAD"
                Using resp As System.Net.WebResponse = req.GetResponse()
                    Dim ContentLength As Long
                    If (Long.TryParse(resp.Headers.Get("Content-Length"), ContentLength)) Then
                        Tries = 3
                        Return ContentLength
                    End If
                End Using
            Catch
                Tries += 1
            End Try
        Loop Until Tries = 3
        Return -1
    End Function

    Public Sub CheckMuted(FolderName As String, PartNo As Integer)

        Dim MI As MediaInfoLib.MediaInfo = New MediaInfoLib.MediaInfo
        MI.Open(FolderName + "\Part " & (PartNo + 1).ToString() & ".flv")
        Dim AudioTrack As String = MI.Get_(MediaInfoLib.StreamKind.Audio, 0, "Format")
        If AudioTrack = "" Then
            System.IO.File.Create(FolderName + "\Part " & (PartNo + 1).ToString() & "_Muted").Close()
            LogToFile(FolderName, PartNo, "File is Muted")
            LogToFile(FolderName, PartNo, "File Muted By Twitch")
            Threading.Thread.Sleep(500)
        End If
    End Sub

    Public Sub StartDownloadWrapper(url As String, url2 As String, title As String, FolderName As String, PartNo As Integer)
        If StartDownload(url, FolderName, PartNo) = False Then
            If StartDownload(url2, FolderName, PartNo) = False Then
                SetStatsText("Failed to Download " & title & " Part: " & (PartNo + 1))
                Threading.Thread.Sleep(500)
                System.IO.File.Create(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed").Close()
                LogToFile(FolderName, PartNo, "Couldn't Download File")
            Else
                SetStatsText("Completed " & title & " Part: " & (PartNo + 1))
                CheckMuted(FolderName, PartNo)
                Threading.Thread.Sleep(500)
            End If
        End If
    End Sub

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

    Sub LogToFile(FolderName As String, PartNo As String, Log As String)
        SyncLock accessLock
            If Not (IO.File.Exists("Log.txt")) Then
                IO.File.Create("Log.txt").Close()
            End If

            Dim FileStream As System.IO.StreamWriter = IO.File.AppendText("Log.txt")
            FileStream.WriteLine(FolderName & " Part: " & (PartNo + 1) & ": " & Log)
            FileStream.Close()
        End SyncLock
    End Sub
End Class
