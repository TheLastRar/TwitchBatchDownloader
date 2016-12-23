using System;
using System.Diagnostics;
using System.IO;

namespace TwitchVodDownloaderSharp.Merge
{
    class FFmpeg
    {
        Process ff;
        volatile bool cancel = false;
        object sentry = new object();

        //Probe Lengths
        public long ProbeSizeM = 2; //Million Bytes
        public long AnalyzeDurationM = 2; //Million Microseconds

        public event EventHandler<StringEvent> FFmpegOut;

        public void StartFFMPEG(string workingDirectory, string args)
        {
            lock (sentry)
            {
                if (cancel) { return; }

                string ffP;
                bool is64 = Environment.Is64BitProcess;
                if (is64 == true)
                {
                    ffP = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin64", "ffmpeg.exe");
                }
                else
                {
                    ffP = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin32", "ffmpeg.exe");
                }

                string fullArgs = string.Format("-probesize {0}M -analyzeduration {1}M {2}", ProbeSizeM, AnalyzeDurationM, args);

                ProcessStartInfo ffSI = new ProcessStartInfo(ffP, fullArgs);
                ffSI.WorkingDirectory = workingDirectory;
                ffSI.RedirectStandardError = true;
                ffSI.RedirectStandardOutput = true;
                ffSI.RedirectStandardInput = true;
                ffSI.UseShellExecute = false;
                ffSI.CreateNoWindow = true;

                ff = new Process();
                ff.StartInfo = ffSI;
                ff.OutputDataReceived += OutputHandler;
                ff.ErrorDataReceived += OutputHandler;

                ff.Start();
                ff.BeginOutputReadLine();
                ff.BeginErrorReadLine();
            }
            ff.WaitForExit();
        }

        private void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            Process proc = sender as Process;
            // Collect the command output.
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (e.Data.StartsWith("["))
                {
                    Console.WriteLine("!!!" + e.Data);
                    FFmpegOut?.Invoke(this, new StringEvent(e.Data));
                }
                else
                {
                    Console.WriteLine(e.Data);
                    FFmpegOut?.Invoke(this, new StringEvent(e.Data));
                }
            }
        }

        public void Cancel()
        {
            lock (sentry)
            {
                cancel = true;
                if (ff != null)
                {
                    if (!ff.HasExited)
                    {
                        ff.StandardInput.Write("q");
                        ff.StandardInput.Flush();
                    }
                }
            }
        }
    }
}
