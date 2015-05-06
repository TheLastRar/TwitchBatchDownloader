Public Enum Format
    TS
    FLV
    AsSource
    MKV
    MP4
End Enum


Public Class VideoMerger

    Public Event ProcOutput(outLine As String)
    Public Event ManOutput(outLine As String)

    Private Sub OutputHandler(sendingProcess As Object, outLine As DataReceivedEventArgs)
        ' Collect the sort command output.
        If Not String.IsNullOrEmpty(outLine.Data) Then
            ' Add the text to the collected output.
            If outLine.Data.StartsWith("[") Then
                'Console.ForegroundColor = ConsoleColor.Yellow
                RaiseEvent ProcOutput("!!!" + outLine.Data)
            Else
                RaiseEvent ProcOutput(outLine.Data)
            End If
            'LogConverision(outLine.Data)
            'Console.ForegroundColor = ConsoleColor.Gray
        End If
    End Sub

#Region "FromTS"
    Public Function TrimUnconvertableFiles(SourceFiles As List(Of String), FileDirectory As String) As List(Of String)
        ''EditZP's stream produces an error on the 1st part that prevents container copy of video
        Dim OK As Boolean = False
        Dim RemoveList As New List(Of String)
        Dim index As Integer = 0
        Do
            Dim ret As String = StartFFprobe(FileDirectory, "-i """ & SourceFiles(index) & """")
            OK = Not ret.Contains("start time for stream 1 is not set in estimate_timings_from_pts")
            If OK = False Then
                RemoveList.Add(SourceFiles(index))
                index += 1
            End If
        Loop Until OK = True
        RaiseEvent ManOutput("Finished Check")
        If RemoveList.Count > OK Then
            RaiseEvent ProcOutput("Warning, Unable to convert start of stream: start time for stream 1 is not set in estimate_timings_from_pts")
            For Each Str As String In RemoveList
                SourceFiles.Remove(Str)
            Next
        End If
        Return SourceFiles
    End Function

    Private Shared Function GenerateParamsTSProtocol(SourceFiles As List(Of String)) As String
        'Only seems to work when going to MKV
        Dim argsProtocol As String = "-f mpegts -i ""concat:"
        For x As Integer = 0 To SourceFiles.Count - 1
            Dim FileName As String = SourceFiles(x)
            argsProtocol = argsProtocol & FileName & "|"
        Next
        'convert the audio
        argsProtocol = argsProtocol.TrimEnd("|") & """ -copyinkf -c:v copy -copyts -c:a copy -y " '"" & FileDirectory & "\Merged.mkv"""
        Return argsProtocol
    End Function

    Public Sub JoinTSToMPEGTS(SourceFiles As List(Of String), FileDirectory As String, Optional Extension As String = ".ts")
        RaiseEvent ManOutput("Merging TS")
        Dim watch As Stopwatch = Stopwatch.StartNew()
        If IO.File.Exists(FileDirectory & "\Merged" & Extension) Then IO.File.Delete(FileDirectory & "\Merged" & Extension)

        'Buch Reads and writes into 10 iterations each (test)

        Dim FileS As New IO.FileStream(FileDirectory & "\Merged" & Extension, IO.FileMode.Append, IO.FileAccess.Write)

        Dim FileHandles As New List(Of IO.FileStream)

        For x As Integer = 0 To SourceFiles.Count - 1
            FileHandles.Add(New IO.FileStream(FileDirectory & "\" & SourceFiles(x), IO.FileMode.Open, IO.FileAccess.Read))
        Next
        For x As Integer = 0 To SourceFiles.Count - 1
            RaiseEvent ProcOutput("Appending Part " & x)
            FileHandles(x).CopyTo(FileS)
        Next
        Debug.Print("Save Compleate")
        For x As Integer = 0 To SourceFiles.Count - 1
            FileHandles(x).Close()
        Next
        FileS.Flush()
        FileS.Close()
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput("")
        RaiseEvent ProcOutput("Merging TS took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        RaiseEvent ProcOutput("")
        'Console.ForegroundColor = ConsoleColor.Gray

    End Sub

    Public Sub JoinTSToMKV(SourceFiles As List(Of String), FileDirectory As String, Optional Extension As String = ".mkv")
        RaiseEvent ManOutput("Converting TS to MKV")
        Dim argsProtocol As String = GenerateParamsTSProtocol(SourceFiles) & "-f matroska ""Merged" & Extension & """"
        Const ProtocolLimit As Integer = 32768

        Dim watch As Stopwatch = Stopwatch.StartNew()

        If argsProtocol.Length + 1 >= ProtocolLimit Then
            RaiseEvent ProcOutput("File To Large to use FFMPEG concat, doing it ourselves")
            JoinTSToMPEGTS(SourceFiles, FileDirectory, ".tmp")
            argsProtocol = "-f mpegts -i ""Merged.tmp"" -copyinkf -c:v copy -copyts -c:a copy -y -f matroska ""Merged" & Extension & """"
        End If

        StartFFMPEG(FileDirectory, argsProtocol)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput("")
        RaiseEvent ProcOutput("Converting TS to MKV took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        RaiseEvent ProcOutput("")
        'Console.ForegroundColor = ConsoleColor.Gray


        If argsProtocol.Length + 1 >= ProtocolLimit Then
            IO.File.Delete(FileDirectory & "\Merged.tmp")
        End If
    End Sub
    Public Sub JoinTSToMP4(SourceFiles As List(Of String), FileDirectory As String)
        RaiseEvent ManOutput("Converting TS to MP4")
        RaiseEvent ManOutput("Managing Audio Sync")
        JoinTSToMKV(SourceFiles, FileDirectory, ".tmp2")
        RaiseEvent ManOutput("Done Managing Audio Sync")
        RaiseEvent ManOutput("Starting Final Convert")

        'Then convert to MP4 (A direct convert will cause desync)
        'Note: in the TS-MKV-MP4 convert, AAC-LC gets converted to AAC-LTP
        'That may be the key to a direct convert without midderling convert
        'At this point, I don't care enough to investigate
        Dim args = "-f matroska -i """ & FileDirectory & "\Merged.tmp2"" -copyinkf -c:v copy -copyts -c:a copy -bsf:a aac_adtstoasc -y """ & FileDirectory & "\Merged.mp4"""

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, args)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput("")
        RaiseEvent ProcOutput("Converting MKV to MP4 took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        RaiseEvent ProcOutput("")
        'Console.ForegroundColor = ConsoleColor.Gray

        IO.File.Delete(FileDirectory & "\Merged.tmp2")
    End Sub
#End Region

#Region "FromFLV"
    Private Shared Function GenerateParamsAnyDemux(SourceFiles As List(Of String), FileDirectory As String) As String
        'Sometimes gives stuttery audio (re-check)
        Dim InputArray(SourceFiles.Count - 1) As String
        For x As Integer = 0 To SourceFiles.Count - 1
            Dim FileName As String = SourceFiles(x)
            InputArray(x) = "file '" & FileDirectory & "\" & FileName & "'"
        Next
        IO.File.WriteAllLines(FileDirectory & "\list.txt", InputArray) 'just leave the file there

        Dim argsDemux As String = "-f concat -i """ & FileDirectory & "\list.txt" & """ -copyinkf -c:v copy -copyts -c:a copy -y " '""" & FileDirectory & "\Merged.mp4"""
        '-copyinkf has been removed
        Return argsDemux
    End Function

    Private Function PartsFLVToMKV(SourceFiles As List(Of String), FileDirectory As String) As List(Of String)
        ''Convert each part to MKV
        Dim watch As Stopwatch = Stopwatch.StartNew()
        If Not IO.Directory.Exists(FileDirectory & "\Temp") Then
            IO.Directory.CreateDirectory(FileDirectory & "\Temp")
        End If

        Dim TempFiles As New List(Of String)
        For x As Integer = 0 To SourceFiles.Count - 1
            Dim args As String = "-i """ & FileDirectory & "\" & SourceFiles(x) & """ -copyinkf -c:v copy -copyts -c:a copy -y " & """" & FileDirectory & "\Temp\" & SourceFiles(x) & ".mkv"""
            StartFFMPEG(FileDirectory, args)
            TempFiles.Add(SourceFiles(x) & ".mkv")
        Next
        watch.Stop()

        Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput("")
        RaiseEvent ProcOutput("Converting parts took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        RaiseEvent ProcOutput("")
        Console.ForegroundColor = ConsoleColor.Gray

        Return TempFiles
    End Function

    Public Sub JoinFLVToFLV(SourceFiles As List(Of String), FileDirectory As String)
        RaiseEvent ManOutput("Merging FLV")
        'Errors produced
        'Unkown stream (id 2)
        'Stream found after parsing head
        'Weird Desync issue, fast catching up effect

        Dim argsDemux As String = GenerateParamsAnyDemux(SourceFiles, FileDirectory) & """" & FileDirectory & "\Merged.flv"""

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, argsDemux)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput("")
        RaiseEvent ProcOutput("Merging FLV took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        RaiseEvent ProcOutput("")
        'Console.ForegroundColor = ConsoleColor.Gray
    End Sub

    Public Sub JoinFLVToMKV(SourceFiles As List(Of String), FileDirectory As String)
        RaiseEvent ManOutput("Converting FLV to MKV")
        'Errors produced
        'Unkown stream (id 2)
        'Stream found after parsing head
        'Sometimes Desyncs

        Dim TempFiles As List(Of String) = PartsFLVToMKV(SourceFiles, FileDirectory)

        'Dim argsDemux As String = GenerateParamsAnyDemux(SourceFiles, FileDirectory) & """" & FileDirectory & "\Merged.mkv"""
        Dim argsDemux As String = GenerateParamsAnyDemux(TempFiles, FileDirectory & "\Temp") & """" & FileDirectory & "\Merged.mkv""" 'should be mkv

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, argsDemux)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput("")
        RaiseEvent ProcOutput("Converting Parts to MKV took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        RaiseEvent ProcOutput("")
        'Console.ForegroundColor = ConsoleColor.Gray

        IO.Directory.Delete(FileDirectory & "\Temp", True)
    End Sub

    Public Sub JoinFLVToMP4_FFMPEG(SourceFiles As List(Of String), FileDirectory As String)
        RaiseEvent ManOutput("Converting FLV to MP4")
        'Errors produced
        'Unkown stream (2)
        'Stream found after parsing head
        'Non-monotonous DTS in output stream 0:0 (video);
        '               Previous: <value>;
        '               current: <value>;
        '               changing to <value+1>. 
        '               This may result in incorrect timestamps in the output file
        'Sometimes get Audio/Visual Corruption (Fixed?)

        Dim TempFiles As List(Of String) = PartsFLVToMKV(SourceFiles, FileDirectory)

        Dim argsDemux As String = GenerateParamsAnyDemux(TempFiles, FileDirectory & "\Temp") & """" & FileDirectory & "\Merged.mp4"""

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, argsDemux)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput("")
        RaiseEvent ProcOutput("Converting Parts to MP4 took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        RaiseEvent ProcOutput("")
        'Console.ForegroundColor = ConsoleColor.Gray

        IO.Directory.Delete(FileDirectory & "\Temp", True)
    End Sub
#End Region

    Private Sub StartFFMPEG(WorkingDirectory As String, Args As String)
        RaiseEvent ProcOutput("Starting FFMPEG")
        Dim ffSI As New ProcessStartInfo(My.Application.Info.DirectoryPath & "\utils\ffmpeg.exe", Args)
        ffSI.WorkingDirectory = WorkingDirectory
        ffSI.RedirectStandardError = True
        ffSI.RedirectStandardOutput = True
        ffSI.RedirectStandardInput = True
        ffSI.UseShellExecute = False
        ffSI.CreateNoWindow = True

        Dim ff As New Process()
        ff.StartInfo = ffSI
        AddHandler ff.OutputDataReceived, AddressOf OutputHandler
        AddHandler ff.ErrorDataReceived, AddressOf OutputHandler

        ff.Start()
        ff.BeginOutputReadLine()
        ff.BeginErrorReadLine()
        ff.WaitForExit()
        RaiseEvent ProcOutput("FFMPEG Done")
    End Sub

    Private Function StartFFprobe(WorkingDirectory As String, Args As String) As String
        RaiseEvent ProcOutput("Starting Probe")
        Dim ffSI As New ProcessStartInfo(My.Application.Info.DirectoryPath & "\utils\ffprobe.exe", Args)
        ffSI.WorkingDirectory = WorkingDirectory
        ffSI.RedirectStandardError = True
        ffSI.RedirectStandardOutput = False
        ''ffSI.RedirectStandardInput = True
        ffSI.UseShellExecute = False
        ffSI.CreateNoWindow = True

        Dim ff As New Process()
        ff.StartInfo = ffSI
        ff.Start()

        Dim sOutput As String = ""
        'Using oStreamReader As System.IO.StreamReader = ff.StandardOutput
        '    sOutput = oStreamReader.ReadToEnd()
        'End Using
        Using oStreamReader As System.IO.StreamReader = ff.StandardError
            sOutput += oStreamReader.ReadToEnd()
        End Using
        ff.WaitForExit()
        RaiseEvent ProcOutput("Probe Done")
        Return sOutput
    End Function

End Class

Public Class AlphanumComparator 'http://www.dotnetperls.com/alphanumeric-sorting-vbnet
    Implements IComparer(Of String)

    Public Function Compare(ByVal x As String,
       ByVal y As String) As Integer Implements IComparer(Of String).Compare

        ' [1] Validate the arguments.
        Dim s1 As String = x
        If s1 = Nothing Then
            Return 0
        End If

        Dim s2 As String = y
        If s2 = Nothing Then
            Return 0
        End If

        Dim len1 As Integer = s1.Length
        Dim len2 As Integer = s2.Length
        Dim marker1 As Integer = 0
        Dim marker2 As Integer = 0

        ' [2] Loop over both Strings.
        While marker1 < len1 And marker2 < len2

            ' [3] Get Chars.
            Dim ch1 As Char = s1(marker1)
            Dim ch2 As Char = s2(marker2)

            Dim space1(len1) As Char
            Dim loc1 As Integer = 0
            Dim space2(len2) As Char
            Dim loc2 As Integer = 0

            ' [4] Collect digits for String one.
            Do
                space1(loc1) = ch1
                loc1 += 1
                marker1 += 1

                If marker1 < len1 Then
                    ch1 = s1(marker1)
                Else
                    Exit Do
                End If
            Loop While Char.IsDigit(ch1) = Char.IsDigit(space1(0))

            ' [5] Collect digits for String two.
            Do
                space2(loc2) = ch2
                loc2 += 1
                marker2 += 1

                If marker2 < len2 Then
                    ch2 = s2(marker2)
                Else
                    Exit Do
                End If
            Loop While Char.IsDigit(ch2) = Char.IsDigit(space2(0))

            ' [6] Convert to Strings.
            Dim str1 = New String(space1)
            Dim str2 = New String(space2)

            ' [7] Parse Strings into Integers.
            Dim result As Integer
            If Char.IsDigit(space1(0)) And Char.IsDigit(space2(0)) Then
                Dim thisNumericChunk = Integer.Parse(str1)
                Dim thatNumericChunk = Integer.Parse(str2)
                result = thisNumericChunk.CompareTo(thatNumericChunk)
            Else
                result = str1.CompareTo(str2)
            End If

            ' [8] Return result if not equal.
            If Not result = 0 Then
                Return result
            End If
        End While

        ' [9] Compare lengths.
        Return len1 - len2
    End Function
End Class
