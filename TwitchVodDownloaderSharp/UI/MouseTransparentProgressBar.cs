using System;
using System.Windows.Forms;

namespace TwitchVodDownloaderSharp.UI
{
    class MouseTransparentProgressBar : ProgressBar
    {
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = -1;

            if (DesignMode)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    m.Result = (IntPtr)HTTRANSPARENT;
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
