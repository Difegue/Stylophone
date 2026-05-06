// Vendored from https://github.com/mono/SkiaSharp v2.88.8 — MIT licensed.
// SkiaSharp 3.x dropped its UWP-specific package; the WinUI variant cannot be
// consumed under net9.0-windows + UseUwp=true. Source-generated COM interop
// is used here so it works with <DisableRuntimeMarshalling>true</DisableRuntimeMarshalling>.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Storage.Streams;

namespace SkiaSharp.Views.UWP
{
    [GeneratedComInterface]
    [Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
    internal partial interface IBufferByteAccess
    {
        void Buffer(out IntPtr value);
    }

    internal static class BufferExtensions
    {
        internal static IntPtr GetByteBuffer(this IBuffer buffer)
        {
            if (buffer is not IBufferByteAccess byteAccess)
                throw new InvalidCastException("Unable to convert WriteableBitmap.PixelBuffer to IBufferByteAccess.");

            byteAccess.Buffer(out IntPtr ptr);
            return ptr;
        }
    }
}
