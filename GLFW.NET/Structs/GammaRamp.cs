using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace GLFW
{
    /// <summary>
    ///     Describes the gamma ramp for a monitor.
    /// </summary>
    [NativeMarshalling(typeof(GammaRampMarshaller))]
    [StructLayout(LayoutKind.Sequential)]
    public struct GammaRamp
    {
        /// <summary>
        ///     An array of value describing the response of the red channel.
        /// </summary>
        [MarshalAs(UnmanagedType.LPArray)]
        public ushort[] Red;

        /// <summary>
        ///     An array of value describing the response of the green channel.
        /// </summary>
        [MarshalAs(UnmanagedType.LPArray)]
        public readonly ushort[] Green;

        /// <summary>
        ///     An array of value describing the response of the blue channel.
        /// </summary>
        [MarshalAs(UnmanagedType.LPArray)]
        public readonly ushort[] Blue;

        /// <summary>
        ///     The number of elements in each array.
        /// </summary>
        public readonly uint Size;

        /// <summary>
        ///     Creates a new instance of a <see cref="GammaRamp" /> using the specified values.
        ///     <para>WARNING: On some platforms (Windows), each value MUST be 256 in length.</para>
        /// </summary>
        /// <param name="red">An array of value describing the response of the red channel.</param>
        /// <param name="green">An array of value describing the response of the green channel.</param>
        /// <param name="blue">An array of value describing the response of the blue channel.</param>
        public GammaRamp(ushort[] red, ushort[] green, ushort[] blue)
        {
            if (red.Length == green.Length && green.Length == blue.Length)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Size = (uint) red.Length;
            }
            else
            {
                throw new ArgumentException(
                    $"{nameof(red)}, {nameof(green)}, and {nameof(blue)} must all be equal length.");
            }
        }
    }

    [CustomMarshaller(typeof(GammaRamp), MarshalMode.ManagedToUnmanagedIn, typeof(GammaRampMarshaller))]
    [CustomMarshaller(typeof(GammaRamp), MarshalMode.ManagedToUnmanagedOut, typeof(GammaRampMarshaller))]
    internal static unsafe class GammaRampMarshaller
    {
        public struct GammaRampUnmanaged
        {
            public ushort* Red;
            public ushort* Green;
            public ushort* Blue;
            public uint Size;
        }

        public static unsafe GammaRampUnmanaged ConvertToUnmanaged (GammaRamp gammaRamp)
        {
            fixed(ushort* r = &gammaRamp.Red[0], g = &gammaRamp.Green[0], b = &gammaRamp.Blue[0])
            {
                return new GammaRampUnmanaged 
                {
                    Red = r, Green = g, Blue = b, Size = gammaRamp.Size
                };
            }            
        }
        public static unsafe GammaRamp ConvertToManaged (GammaRampUnmanaged gammaRamp)
        {
            var ramp = new GammaRamp(new ushort[gammaRamp.Size], new ushort[gammaRamp.Size], new ushort[gammaRamp.Size]);

            fixed (ushort* r = &ramp.Red[0], g = &ramp.Green[0], b = &ramp.Blue[0])
            {
                int size = (int)gammaRamp.Size * sizeof(ushort);
                Buffer.MemoryCopy(gammaRamp.Red,   r, size, size);
                Buffer.MemoryCopy(gammaRamp.Green, g, size, size);
                Buffer.MemoryCopy(gammaRamp.Blue,  b, size, size);
            }

            return ramp;
        }

        public static void Free (GammaRampUnmanaged unmanaged) { }
    }
}