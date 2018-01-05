namespace MpcNET
{
    /// <summary>
    /// The possible states of the MPD.
    /// </summary>
    public enum MpdState
    {
        /// <summary>
        /// The state of the MPD could not be translated into this enumeration.
        /// </summary>
        Unknown,
        /// <summary>
        /// The MPD is playing a track.
        /// </summary>
        Play,
        /// <summary>
        /// The MPD is not playing a track.
        /// </summary>
        Stop,
        /// <summary>
        /// The playback of the MPD is currently paused.
        /// </summary>
        Pause
    }
}