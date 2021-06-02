using System;
using System.Collections.Generic;
using System.Text;

namespace DocScanOpenCV.CameraRenderer
{

    public interface ICaptureUI : IDisposable
    {
        /// <summary>
        /// Starts the Capture Session.
        /// </summary>
        void StartSession();

        /// <summary>
        /// Stops the Capture Session.
        /// </summary>
        void StopSession();

        /// <summary>
        /// Determines if the Capture Session is active.
        /// </summary>
        bool GetSessionActive();

        /// <summary>
        /// res.
        /// </summary>
        string GetResults();

        /// <summary>
        /// Turn on or off the flash.
        /// </summary>
        void onClickFlash();
    }
}
