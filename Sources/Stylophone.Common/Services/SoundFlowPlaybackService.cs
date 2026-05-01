#nullable enable
using System;
using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;
using Stylophone.Common.Interfaces;

namespace Stylophone.Common.Services
{
    /// <summary>
    /// <see cref="IPlaybackService"/> implementation backed by SoundFlow's MiniAudio engine.
    /// Replaces the prior LibVLC-based implementation; works on UWP (net9-windows) and iOS,
    /// since LibVLC's UWP build does not exist for net9.
    /// </summary>
    public sealed class SoundFlowPlaybackService : IPlaybackService
    {
        // Engine + output device live for the lifetime of the service (typically the app).
        // Per-stream resources (player + provider) are recreated on each Play().
        private readonly AudioEngine _engine;
        private readonly AudioFormat _format = AudioFormat.DvdHq; // 48 kHz / F32 / stereo
        private readonly AudioPlaybackDevice _device;

        private SoundPlayer? _player;
        private NetworkDataProvider? _provider;

        // 0..100, mirrors the legacy LibVLC scale used by LocalPlaybackViewModel.
        private int _volume = 50;
        private bool _disposed;

        public SoundFlowPlaybackService()
        {
            _engine = new MiniAudioEngine();
            _device = _engine.InitializePlaybackDevice(null, _format);
            _device.Start();
        }

        public bool IsPlaying => _player?.State == PlaybackState.Playing;

        public int Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0, 100);
                if (_player != null)
                    _player.Volume = _volume / 100f;
            }
        }

        public void Play(Uri streamUrl)
        {
            if (streamUrl is null) throw new ArgumentNullException(nameof(streamUrl));
            ThrowIfDisposed();

            // Tear down any previous stream first so we don't stack mixer components.
            StopInternal();

            _provider = new NetworkDataProvider(_engine, streamUrl.ToString());
            _player = new SoundPlayer(_engine, _format, _provider)
            {
                Volume = _volume / 100f
            };

            _device.MasterMixer.AddComponent(_player);
            _player.Play();
        }

        public void Stop()
        {
            if (_disposed) return;
            StopInternal();
        }

        private void StopInternal()
        {
            if (_player != null)
            {
                try { _player.Stop(); } catch { /* best effort */ }
                try { _device.MasterMixer.RemoveComponent(_player); } catch { /* best effort */ }
                _player.Dispose();
                _player = null;
            }

            if (_provider != null)
            {
                _provider.Dispose();
                _provider = null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopInternal();

            try { _device.Stop(); } catch { /* best effort */ }
            _device.Dispose();
            _engine.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SoundFlowPlaybackService));
        }
    }
}
