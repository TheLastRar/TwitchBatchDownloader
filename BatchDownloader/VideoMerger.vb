﻿Public Enum Format
    TS
    FLV
    AsSource
    MKV
    MP4
End Enum


Public Class VideoMerger

    Declare Function AllocConsole Lib "kernel32" () As Integer
    Declare Function FreeConsole Lib "kernel32" () As Integer

    Shared Sub OutputHandler(sendingProcess As Object, outLine As DataReceivedEventArgs)
        ' Collect the sort command output.
        If Not String.IsNullOrEmpty(outLine.Data) Then
            ' Add the text to the collected output.
            If outLine.Data.StartsWith("[") Then
                Console.ForegroundColor = ConsoleColor.Yellow
            End If
            LogConverision(outLine.Data)
            Console.ForegroundColor = ConsoleColor.Gray
        End If
    End Sub

    Shared Sub Merge(FolderName As String, SourceFormat As String, TargetFormat As Format)
        If (Not System.IO.File.Exists(FolderName & "\Done.Download")) Then
            Return 'Download Not Compleated
        End If

        If (System.IO.File.Exists(FolderName & "\Done.Convert")) Then
            Return 'Convert already Done
        End If

        Dim Files As String() = IO.Directory.GetFiles(FolderName)

        For Each fileName As String In Files
            If fileName.EndsWith("_Failed") Then
                Return 'Download Failed
            End If
            If fileName.EndsWith("_Muted") Then
                Return 'Download has Muted Files
            End If
        Next

        AllocConsole()
        LogConverision("Hello World")

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
        'If SourceFiles.Count = 1 Then
        '    Return
        'End If

        Select Case SourceFormat
            Case Is = ".ts"
                Select Case TargetFormat
                    Case Format.AsSource
                        JoinTSToMPEGTS(SourceFiles, FileDirectory)
                    Case Format.MKV
                        JoinTSToMKV(SourceFiles, FileDirectory)
                    Case Format.MP4
                        JoinTSToMP4(SourceFiles, FileDirectory)
                End Select
            Case Is = ".flv"
                Select Case TargetFormat
                    Case Format.AsSource
                        JoinFLVToFLV(SourceFiles, FileDirectory)
                    Case Format.MKV
                        JoinFLVToMKV(SourceFiles, FileDirectory)
                    Case Format.MP4
                        'JoinFLVToMP4_MP4Box(SourceFiles, FileDirectory)
                        JoinFLVToMP4_FFMPEG(SourceFiles, FileDirectory)
                End Select
        End Select
        System.IO.File.Create(FolderName & "\Done.Convert").Close()
        FreeConsole()
    End Sub

#Region "FromTS"
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

    'Private Shared Sub JoinTSToMPEGTS_old(SourceFiles As List(Of String), FileDirectory As String, Optional Extension As String = ".ts")
    '    If IO.File.Exists(FileDirectory & "\Merged" & Extension) Then IO.File.Delete(FileDirectory & "\Merged" & Extension)

    '    Dim FileS As New IO.FileStream(FileDirectory & "\Merged" & Extension, IO.FileMode.Append, IO.FileAccess.Write)
    '    For x As Integer = 0 To SourceFiles.Count - 1 Step 10
    '        Dim Bytes As Byte() = IO.File.ReadAllBytes(FileDirectory & "\" & SourceFiles(x))
    '        FileS.Write(Bytes, 0, Bytes.Length)
    '    Next
    '    FileS.Close()
    'End Sub

    Private Shared Sub JoinTSToMPEGTS(SourceFiles As List(Of String), FileDirectory As String, Optional Extension As String = ".ts")
        LogConverision("Merging TS")
        Dim watch As Stopwatch = Stopwatch.StartNew()
        If IO.File.Exists(FileDirectory & "\Merged" & Extension) Then IO.File.Delete(FileDirectory & "\Merged" & Extension)

        'Buch Reads and writes into 10 iterations each (test)

        Dim FileS As New IO.FileStream(FileDirectory & "\Merged" & Extension, IO.FileMode.Append, IO.FileAccess.Write)

        Dim FileHandles As New List(Of IO.FileStream)

        For x As Integer = 0 To SourceFiles.Count - 1
            FileHandles.Add(New IO.FileStream(FileDirectory & "\" & SourceFiles(x), IO.FileMode.Open, IO.FileAccess.Read))
        Next
        For x As Integer = 0 To SourceFiles.Count - 1
            LogConverision("Appending Part " & x)
            FileHandles(x).CopyTo(FileS)
        Next
        Debug.Print("Save Compleate")
        For x As Integer = 0 To SourceFiles.Count - 1
            FileHandles(x).Close()
        Next
        FileS.Flush()
        FileS.Close()
        watch.Stop()

        Console.ForegroundColor = ConsoleColor.Cyan
        LogConverision("Merging TS took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        Console.ForegroundColor = ConsoleColor.Gray

    End Sub

    Private Shared Sub JoinTSToMKV(SourceFiles As List(Of String), FileDirectory As String, Optional Extension As String = ".mkv")
        LogConverision("Converting TS to MKV")
        Dim argsProtocol As String = GenerateParamsTSProtocol(SourceFiles) & "-f matroska ""Merged" & Extension & """"
        Const ProtocolLimit As Integer = 32768

        Dim watch As Stopwatch = Stopwatch.StartNew()

        If argsProtocol.Length + 1 >= ProtocolLimit Then
            LogConverision("File To Large to use FFMPEG concat, doing it ourselves")
            JoinTSToMPEGTS(SourceFiles, FileDirectory, ".tmp")
            argsProtocol = "-f mpegts -i ""Merged.tmp"" -copyinkf -c:v copy -copyts -c:a copy -y -f matroska ""Merged" & Extension & """"
        End If

        StartFFMPEG(FileDirectory, argsProtocol)
        watch.Stop()

        Console.ForegroundColor = ConsoleColor.Cyan
        LogConverision("Converting TS to MKV took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        Console.ForegroundColor = ConsoleColor.Gray


        If argsProtocol.Length + 1 >= ProtocolLimit Then
            IO.File.Delete(FileDirectory & "\Merged.tmp")
        End If
    End Sub
    Private Shared Sub JoinTSToMP4(SourceFiles As List(Of String), FileDirectory As String)
        LogConverision("Converting TS to MP4")
        LogConverision("Managing Audio Sync")
        JoinTSToMKV(SourceFiles, FileDirectory, ".tmp2")
        LogConverision("Done Managing Audio Sync")
        LogConverision("Starting Final Convert")

        'Then convert to MP4 (A direct convert will cause desync)
        'Note: in the TS-MKV-MP4 convert, AAC-LC gets converted to AAC-LTP
        'That may be the key to a direct convert without midderling convert
        'At this point, I don't care enough to investigate
        Dim args = "-f matroska -i """ & FileDirectory & "\Merged.tmp2"" -copyinkf -c:v copy -copyts -c:a copy -bsf:a aac_adtstoasc -y """ & FileDirectory & "\Merged.mp4"""

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, args)
        watch.Stop()

        Console.ForegroundColor = ConsoleColor.Cyan
        LogConverision("Converting MKV to MP4 took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        Console.ForegroundColor = ConsoleColor.Gray

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

    Private Shared Function PartsFLVToMKV(SourceFiles As List(Of String), FileDirectory As String) As List(Of String)
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
        LogConverision("Converting parts took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        Console.ForegroundColor = ConsoleColor.Gray

        Return TempFiles
    End Function

    Private Shared Sub JoinFLVToFLV(SourceFiles As List(Of String), FileDirectory As String)
        LogConverision("Merging FLV")
        'Errors produced
        'Unkown stream (id 2)
        'Stream found after parsing head
        'Weird Desync issue, fast catching up effect

        Dim argsDemux As String = GenerateParamsAnyDemux(SourceFiles, FileDirectory) & """" & FileDirectory & "\Merged.flv"""

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, argsDemux)
        watch.Stop()

        Console.ForegroundColor = ConsoleColor.Cyan
        LogConverision("Merging FLV took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        Console.ForegroundColor = ConsoleColor.Gray
    End Sub

    Private Shared Sub JoinFLVToMKV(SourceFiles As List(Of String), FileDirectory As String)
        LogConverision("Converting FLV to MKV")
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

        Console.ForegroundColor = ConsoleColor.Cyan
        LogConverision("Converting Parts to MKV took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        Console.ForegroundColor = ConsoleColor.Gray

        IO.Directory.Delete(FileDirectory & "\Temp", True)
    End Sub

    Private Shared Sub JoinFLVToMP4_FFMPEG(SourceFiles As List(Of String), FileDirectory As String)
        LogConverision("Converting FLV to MP4")
        'Errors produced
        'Unkown stream (2)
        'Stream found after parsing head
        'Non-monotonous DTS in output stream 0:0 (video);
        '               Previous: <value>;
        '               current: <value>;
        '               changing to <value+1>. 
        '               This may result in incorrect timestamps in the output file
        'Sometimes get Audio/Visual Curroption

        Dim TempFiles As List(Of String) = PartsFLVToMKV(SourceFiles, FileDirectory)

        Dim argsDemux As String = GenerateParamsAnyDemux(TempFiles, FileDirectory & "\Temp") & """" & FileDirectory & "\Merged.mp4"""

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, argsDemux)
        watch.Stop()

        Console.ForegroundColor = ConsoleColor.Cyan
        LogConverision("Converting Parts to MP4 took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
        Console.ForegroundColor = ConsoleColor.Gray

        IO.Directory.Delete(FileDirectory & "\Temp", True)
    End Sub

    'Private Shared Sub JoinFLVToMP4_MP4Box(SourceFiles As List(Of String), FileDirectory As String)
    '    LogConverision("Converting FLV to MP4")
    '    'Errors produced
    '    'Unkown stream (2)
    '    'Stream found after parsing head

    '    'Still has Visual corruption issues

    '    'Convert Each Part to MP4
    '    If Not IO.Directory.Exists(FileDirectory & "\Temp") Then
    '        IO.Directory.CreateDirectory(FileDirectory & "\Temp")
    '    End If
    '    Dim watchTotal As Stopwatch = Stopwatch.StartNew()
    '    Dim watch As Stopwatch = Stopwatch.StartNew()
    '    For x As Integer = 0 To SourceFiles.Count - 1
    '        Dim args As String = "-i """ & FileDirectory & "\" & SourceFiles(x) & """ -copyinkf -c:v copy -copyts -c:a copy -y " & """" & FileDirectory & "\Temp\" & SourceFiles(x) & ".mp4"""
    '        StartFFMPEG(FileDirectory, args)
    '    Next
    '    watch.Stop()

    '    Console.ForegroundColor = ConsoleColor.Cyan
    '    LogConverision("Converting FLV to MP4 Parts took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
    '    Console.ForegroundColor = ConsoleColor.Gray

    '    LogConverision("Merging MP4 Parts")

    '    Dim argsMP4Box As String = ""
    '    For x As Integer = 0 To SourceFiles.Count - 1
    '        argsMP4Box = argsMP4Box & " -cat """ & "Temp\" & SourceFiles(x) & ".mp4"""
    '    Next
    '    argsMP4Box = argsMP4Box & " -new MergedMP4.mp4"

    '    watch = Stopwatch.StartNew()
    '    Const ProtocolLimit As Integer = 32768
    '    If argsMP4Box.Length + 1 >= ProtocolLimit Then
    '        'error
    '        Throw New Exception("MP4Box Args Too Long")
    '    Else
    '        StartMP4Box(FileDirectory, argsMP4Box) 'Takes 12 Times as long as the 
    '    End If

    '    watch.Stop()
    '    watchTotal.Stop()

    '    Console.ForegroundColor = ConsoleColor.Cyan
    '    LogConverision("Merging MP4 Parts took " & (watch.ElapsedMilliseconds \ 1000L) & "s")
    '    Console.ForegroundColor = ConsoleColor.Gray

    '    Console.ForegroundColor = ConsoleColor.Cyan
    '    LogConverision("Total time taken " & (watchTotal.ElapsedMilliseconds \ 1000L) & "s")
    '    Console.ForegroundColor = ConsoleColor.Gray

    '    IO.Directory.Delete(FileDirectory & "\Temp", True)
    'End Sub
#End Region

    Shared Sub StartFFMPEG(WorkingDirectory As String, Args As String)
        LogConverision("Starting FFMPEG")
        Dim ffSI As New ProcessStartInfo(My.Application.Info.DirectoryPath & "\utils\ffmpeg.exe", Args)
        ffSI.WorkingDirectory = WorkingDirectory
        ffSI.RedirectStandardError = True
        ffSI.RedirectStandardOutput = True
        ffSI.RedirectStandardInput = True
        ffSI.UseShellExecute = False
        ffSI.CreateNoWindow = False

        Dim ff As New Process()
        ff.StartInfo = ffSI
        AddHandler ff.OutputDataReceived, AddressOf OutputHandler
        AddHandler ff.ErrorDataReceived, AddressOf OutputHandler

        ff.Start()
        ff.BeginOutputReadLine()
        ff.BeginErrorReadLine()
        ff.WaitForExit()
        LogConverision("FFMPEG Done")
    End Sub

    Shared Sub StartMP4Box(WorkingDirectory As String, Args As String)
        LogConverision("Starting MP4Box")

        Dim TempDir As String = IO.Directory.GetCurrentDirectory & "\Temp\MP4Box"

        If Not IO.Directory.Exists(IO.Directory.GetCurrentDirectory & "\Temp\MP4Box") Then
            IO.Directory.CreateDirectory(IO.Directory.GetCurrentDirectory & "\Temp\MP4Box")
        End If

        Dim ffSI As New ProcessStartInfo(My.Application.Info.DirectoryPath & "\utils\MP4Box.exe",
                                         "-tmp """ & TempDir & """ " & Args)
        'ffSI.WorkingDirectory = My.Application.Info.DirectoryPath & "\utils"
        ffSI.WorkingDirectory = WorkingDirectory
        ffSI.RedirectStandardError = True
        ffSI.RedirectStandardOutput = True
        ffSI.RedirectStandardInput = True
        ffSI.UseShellExecute = False
        ffSI.CreateNoWindow = False

        Dim ff As New Process()
        ff.StartInfo = ffSI
        AddHandler ff.OutputDataReceived, AddressOf OutputHandler
        AddHandler ff.ErrorDataReceived, AddressOf OutputHandler

        ff.Start()
        ff.BeginOutputReadLine()
        ff.BeginErrorReadLine()
        ff.WaitForExit()
        LogConverision("MP4Box Done")
    End Sub

    Shared Sub LogConverision(str As String)
        Console.WriteLine(str)
        Debug.Print(str)
    End Sub

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
