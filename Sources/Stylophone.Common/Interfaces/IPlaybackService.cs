using System;

namespace Stylophone.Common.Interfaces
{
    /// <summary>
    /// Abstracts the local audio output used to play back the MPD server's
    /// <c>httpd_output</c> stream (or any continuous HTTP audio stream).
    /// Implemented per platform — historically by LibVLC, currently by SoundFlow.
    /// </summary>
    public interface IPlaybackService : IDisposable
    {
        /// <summary>True while a stream is actively playing.</summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Volume on a 0..100 scale to match the existing UI/viewmodel contract.
        /// Setting this while playing must update the live stream volume.
        /// </summary>
        int Volume { get; set; }

        /// <summary>
        /// Open and play <paramref name="streamUrl"/>. Stops any prior stream first.
        /// May throw — callers should surface errors via <see cref="INotificationService"/>.
        /// </summary>
        void Play(Uri streamUrl);

        /// <summary>Stop and tear down the current stream. Safe to call when idle.</summary>
        void Stop();
    }
}
