'*  Copyright (c) MediaArea.net SARL. All Rights Reserved.
'*
'*  Use of this source code is governed by a BSD-style license that can
'*  be found in the License.html file in the root of the source tree.
'*/

'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
'
' Microsoft Visual C# wrapper for MediaInfo Library - converted to VB by Air_Gamer
' See MediaInfo.h for help
'
' To make it working, you must put MediaInfo.Dll
' in the executable folder
'
'+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

imports System
imports System.Runtime.InteropServices

'#pragma warning disable 1591 // Disable XML documentation warnings

Namespace MediaInfoLib

    Public Enum StreamKind
        General
        Video
        Audio
        Text
        Other
        Image
        Menu
    End Enum

    Public Enum InfoKind
        Name
        Text
        Measure
        Options
        NameText
        MeasureText
        Info
        HowTo
    End Enum

    Public Enum InfoOptions
        ShowInInform
        Support
        ShowInSupported
        TypeOfValue
    End Enum

    Public Enum InfoFileOptions
        FileOption_Nothing = &H0
        FileOption_NoRecursive = &H1
        FileOption_CloseAll = &H2
        FileOption_Max = &H4
    End Enum


    Public Class MediaInfo
        'Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_New() As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Sub MediaInfo_Delete(Handle As IntPtr)
        End Sub
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_Open(Handle As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> FileName As String) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoA_Open(Handle As IntPtr, FileName As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_Open_Buffer_Init(Handle As IntPtr, File_Size As Int64, File_Offset As Int64) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoA_Open(Handle As IntPtr, File_Size As Int64, File_Offset As Int64) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_Open_Buffer_Continue(Handle As IntPtr, Buffer As IntPtr, Buffer_Size As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoA_Open_Buffer_Continue(Handle As IntPtr, File_Size As Int64, Buffer As Byte(), Buffer_Size As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_Open_Buffer_Continue_GoTo_Get(Handle As IntPtr) As Int64
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoA_Open_Buffer_Continue_GoTo_Get(Handle As IntPtr) As Int64
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_Open_Buffer_Finalize(Handle As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoA_Open_Buffer_Finalize(Handle As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Sub MediaInfo_Close(Handle As IntPtr)
        End Sub
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_Inform(Handle As IntPtr, Reserved As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoA_Inform(Handle As IntPtr, Reserved As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_GetI(Handle As IntPtr, StreamKind As IntPtr, StreamNumber As IntPtr, Parameter As IntPtr, KindOfInfo As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoA_GetI(Handle As IntPtr, StreamKind As IntPtr, StreamNumber As IntPtr, Parameter As IntPtr, KindOfInfo As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_Get(Handle As IntPtr, StreamKind As IntPtr, StreamNumber As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> Parameter As String, KindOfInfo As IntPtr, KindOfSearch As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoA_Get(Handle As IntPtr, StreamKind As IntPtr, StreamNumber As IntPtr, Parameter As IntPtr, indOfInfo As IntPtr, KindOfSearch As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_Option(Handle As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> OptionStr As String, <MarshalAs(UnmanagedType.LPWStr)> Value As String) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoA_Option(Handle As IntPtr, OptionPtr As IntPtr, Value As IntPtr) As String
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_State_Get(Handle As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfo_Count_Get(Handle As IntPtr, StreamKind As IntPtr, StreamNumber As IntPtr) As IntPtr
        End Function

        'MediaInfo class
        Public Sub New()
            Handle = MediaInfo_New()
            If (Environment.OSVersion.ToString().IndexOf("Windows") = -1) Then
                MustUseAnsi = True
            Else
                MustUseAnsi = False
            End If
        End Sub
        Public Sub Dispose()
            MediaInfo_Delete(Handle)
        End Sub
        Public Function Open(FileName As String) As Integer
            If (MustUseAnsi) Then
                Dim FileName_Ptr As Integer = Marshal.StringToHGlobalAnsi(FileName)
                Dim ToReturn As Integer = (MediaInfoA_Open(Handle, FileName_Ptr))
                Marshal.FreeHGlobal(FileName_Ptr)
                Return ToReturn
            Else
                Return CInt(MediaInfo_Open(Handle, FileName))
            End If
        End Function
        Public Function Open_Buffer_Init(File_Size As Int64, File_Offset As Int64) As Integer
            Return CInt(MediaInfo_Open_Buffer_Init(Handle, File_Size, File_Offset))
        End Function
        Public Function Open_Buffer_Continue(Buffer As IntPtr, Buffer_Size As IntPtr) As Integer
            Return CInt(MediaInfo_Open_Buffer_Continue(Handle, Buffer, Buffer_Size))
        End Function
        Public Function Open_Buffer_Continue_GoTo_Get() As Int64
            Return MediaInfo_Open_Buffer_Continue_GoTo_Get(Handle)
        End Function
        Public Function Open_Buffer_Finalize() As Integer
            Return CInt(MediaInfo_Open_Buffer_Finalize(Handle))
        End Function
        Public Sub Close()
            MediaInfo_Close(Handle)
        End Sub
        Public Function Inform() As String
            If (MustUseAnsi) Then
                Return Marshal.PtrToStringAnsi(MediaInfoA_Inform(Handle, CType(0, IntPtr)))
            Else
                Return Marshal.PtrToStringUni(MediaInfo_Inform(Handle, CType(0, IntPtr)))
            End If
        End Function
        Public Function Get_(StreamKind As StreamKind, StreamNumber As Integer, Parameter As String, KindOfInfo As InfoKind, KindOfSearch As InfoKind) As String
            If (MustUseAnsi) Then
                Dim Parameter_Ptr As IntPtr = Marshal.StringToHGlobalAnsi(Parameter)
                Dim ToReturn As String = Marshal.PtrToStringAnsi(MediaInfoA_Get(Handle, CType(StreamKind, IntPtr), CType(StreamNumber, IntPtr), Parameter_Ptr, CType(KindOfInfo, IntPtr), CType(KindOfSearch, IntPtr)))
                Marshal.FreeHGlobal(Parameter_Ptr)
                Return ToReturn
            Else
                Return Marshal.PtrToStringUni(MediaInfo_Get(Handle, CType(StreamKind, IntPtr), CType(StreamNumber, IntPtr), Parameter, CType(KindOfInfo, IntPtr), CType(KindOfSearch, IntPtr)))
            End If
        End Function
        Public Function Get_(StreamKind As StreamKind, StreamNumber As Integer, Parameter As Integer, KindOfInfo As InfoKind) As String
            If (MustUseAnsi) Then
                Return Marshal.PtrToStringAnsi(MediaInfoA_GetI(Handle, CType(StreamKind, IntPtr), CType(StreamNumber, IntPtr), CType(Parameter, IntPtr), CType(KindOfInfo, IntPtr)))
            Else
                Return Marshal.PtrToStringUni(MediaInfo_GetI(Handle, CType(StreamKind, IntPtr), CType(StreamNumber, IntPtr), CType(Parameter, IntPtr), CType(KindOfInfo, IntPtr)))
            End If
        End Function
        Public Function Option_(OptionStr As String, Value As String) As String
            If (MustUseAnsi) Then
                Dim Option_Ptr As IntPtr = Marshal.StringToHGlobalAnsi(OptionStr)
                Dim Value_Ptr As IntPtr = Marshal.StringToHGlobalAnsi(Value)
                Dim ToReturn As String = Marshal.PtrToStringAnsi(MediaInfoA_Option(Handle, Option_Ptr, Value_Ptr))
                Marshal.FreeHGlobal(Option_Ptr)
                Marshal.FreeHGlobal(Value_Ptr)
                Return ToReturn
            Else
                Return Marshal.PtrToStringUni(MediaInfo_Option(Handle, OptionStr, Value))
            End If
        End Function
        Public Function State_Get() As Integer
            Return CInt(MediaInfo_State_Get(Handle))
        End Function

        Public Function Count_Get(StreamKind As StreamKind, StreamNumber As Integer) As Integer
            Return CInt(MediaInfo_Count_Get(Handle, CType(StreamKind, IntPtr), CType(StreamNumber, IntPtr)))
        End Function
        Private Handle As IntPtr
        Private MustUseAnsi As Boolean

        'Default values, if you know how to set default values in C#, say me
        Public Function Get_(StreamKind As StreamKind, StreamNumber As Integer, Parameter As String, KindOfInfo As InfoKind) As String
            Return Get_(StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name)
        End Function
        Public Function Get_(StreamKind As StreamKind, StreamNumber As Integer, Parameter As String) As String
            Return Get_(StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name)
        End Function
        Public Function Get_(StreamKind As StreamKind, StreamNumber As Integer, Parameter As Integer) As String
            Return Get_(StreamKind, StreamNumber, Parameter, InfoKind.Text)
        End Function
        Public Function Option_(OptionStr As String) As String
            Return Option_(OptionStr, "")
        End Function
        Public Function Count_Get(StreamKind As StreamKind) As Integer
            Return Count_Get(StreamKind, -1)
        End Function
    End Class

    Public Class MediaInfoList
        'Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoList_New() As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Sub MediaInfoList_Delete(Handle As IntPtr)
        End Sub
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoList_Open(Handle As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> FileName As String, Options As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Shared Sub MediaInfoList_Close(Handle As IntPtr, FilePos As IntPtr)
        End Sub
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoList_Inform(Handle As IntPtr, FilePos As IntPtr, Reserved As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoList_GetI(Handle As IntPtr, FilePos As IntPtr, StreamKind As IntPtr, StreamNumber As IntPtr, Parameter As IntPtr, KindOfInfo As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoList_Get(Handle As IntPtr, FilePos As IntPtr, StreamKind As IntPtr, StreamNumber As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> Parameter As String, KindOfInfo As IntPtr, KindOfSearch As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoList_Option(Handle As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> OptionStr As String, <MarshalAs(UnmanagedType.LPWStr)> Value As String) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoList_State_Get(Handle As IntPtr) As IntPtr
        End Function
        <DllImport("MediaInfo.dll")>
        Private Shared Function MediaInfoList_Count_Get(Handle As IntPtr, FilePos As IntPtr, StreamKind As IntPtr, StreamNumber As IntPtr) As IntPtr
        End Function

        'MediaInfo class
        Public Sub New()
            Handle = MediaInfoList_New()
        End Sub
        Public Sub Dispose()
            MediaInfoList_Delete(Handle)
        End Sub
        Public Function Open(FileName As String, Options As InfoFileOptions) As Integer
            Return CInt(MediaInfoList_Open(Handle, FileName, CType(Options, IntPtr)))
        End Function
        Public Sub Close(FilePos As Integer)
            MediaInfoList_Close(Handle, CType(FilePos, IntPtr))
        End Sub
        Public Function Inform(FilePos As Integer) As String
            Return Marshal.PtrToStringUni(MediaInfoList_Inform(Handle, CType(FilePos, Integer), CType(0, IntPtr)))
        End Function
        Public Function Get_(FilePos As Integer, StreamKind As StreamKind, StreamNumber As Integer, Parameter As String, KindOfInfo As InfoKind, KindOfSearch As InfoKind) As String
            Return Marshal.PtrToStringUni(MediaInfoList_Get(Handle, CType(FilePos, IntPtr), CType(StreamKind, IntPtr), CType(StreamNumber, IntPtr), Parameter, CType(KindOfInfo, IntPtr), CType(KindOfSearch, IntPtr)))
        End Function
        Public Function Get_(FilePos As Integer, StreamKind As StreamKind, StreamNumber As Integer, Parameter As Integer, KindOfInfo As InfoKind) As String
            Return Marshal.PtrToStringUni(MediaInfoList_GetI(Handle, CType(FilePos, IntPtr), CType(StreamKind, IntPtr), CType(StreamNumber, IntPtr), CType(Parameter, IntPtr), CType(KindOfInfo, IntPtr)))
        End Function
        Public Function Option_(OptionStr As String, Value As String) As String
            Return Marshal.PtrToStringUni(MediaInfoList_Option(Handle, OptionStr, Value))
        End Function
        Public Function State_Get() As Integer
            Return CInt(MediaInfoList_State_Get(Handle))
        End Function
        Public Function Count_Get(FilePos As Integer, StreamKind As StreamKind, StreamNumber As Integer) As Integer
            Return CInt(MediaInfoList_Count_Get(Handle, CType(FilePos, IntPtr), CType(StreamKind, IntPtr), CType(StreamNumber, IntPtr)))
        End Function
        Private Handle As IntPtr

        'Default values, if you know how to set default values in C#, say me
        Public Sub Open(FileName As String)
            Open(FileName, 0)
        End Sub
        Public Sub Close()
            Close(-1)
        End Sub
        Public Function Get_(FilePos As Integer, StreamKind As StreamKind, StreamNumber As Integer, Parameter As String, KindOfInfo As InfoKind)
            Return Get_(FilePos, StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name)
        End Function
        Public Function Get_(FilePos As Integer, StreamKind As StreamKind, StreamNumber As Integer, Parameter As String) As String
            Return Get_(FilePos, StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name)
        End Function
        Public Function Get_(FilePos As Integer, StreamKind As StreamKind, StreamNumber As Integer, Parameter As Integer) As String
            Return Get_(FilePos, StreamKind, StreamNumber, Parameter, InfoKind.Text)
        End Function
        Public Function Option_(OptionStr As String) As String
            Return Option_(OptionStr, "")
        End Function
        Public Function Count_Get(FilePos As Integer, StreamKind As StreamKind) As Integer
            Return Count_Get(FilePos, StreamKind, -1)
        End Function
    End Class

End Namespace
