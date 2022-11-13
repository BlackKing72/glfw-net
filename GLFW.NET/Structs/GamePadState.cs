using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace GLFW
{
    /// <summary>
    ///     Represents the state of a gamepad.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeMarshalling(typeof(GamePadStateMarshaller))]
    public struct GamePadState
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        internal readonly InputState[] states;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        internal readonly float[] axes;

        /// <summary>
        ///     Gets the state of the specified <paramref name="button" />.
        /// </summary>
        /// <param name="button">The button to retrieve the state of.</param>
        /// <returns>The button state, either <see cref="InputState.Press" /> or <see cref="InputState.Release" />.</returns>
        public InputState GetButtonState(GamePadButton button) { return states[(int) button]; }

        /// <summary>
        ///     Gets the value of the specified <paramref name="axis" />.
        /// </summary>
        /// <param name="axis">The axis to retrieve the value of.</param>
        /// <returns>The axis value, in the range of <c>-1.0</c> and <c>1.0</c> inclusive.</returns>
        public float GetAxis(GamePadAxis axis) { return axes[(int) axis]; }
    }

    [CustomMarshaller(typeof(GamePadState), MarshalMode.ManagedToUnmanagedIn, typeof(GamePadStateMarshaller))]
    [CustomMarshaller(typeof(GamePadState), MarshalMode.ManagedToUnmanagedOut, typeof(GamePadStateMarshaller))]
    internal static unsafe class GamePadStateMarshaller
    {
        public unsafe struct GamePadStateUnmanaged
        {
            public fixed byte Buttons[15];
            public fixed float Axes[6];
        }

        public static unsafe GamePadStateUnmanaged ConvertToUnmanaged (GamePadState gamePadState)
        {
            var state = new GamePadStateUnmanaged();

            fixed(GLFW.InputState* b = &gamePadState.states[0])
            {
                const int size = sizeof(byte) * 15;
                Buffer.MemoryCopy(b, state.Buttons, size, size);
            }

            fixed(float* a = &gamePadState.axes[0])
            {
                const int size = sizeof(float) * 6;
                Buffer.MemoryCopy(a, state.Axes, size, size);
            }

            return state;
        } 

        public static unsafe GamePadState ConvertToManaged (GamePadStateUnmanaged gamePadState)
        {
            var state = new GamePadState();

            fixed (GLFW.InputState* b = &state.states[0])
            {
                const int size = sizeof(byte) * 15;
                Buffer.MemoryCopy(gamePadState.Buttons, b, size, size);
            }

            fixed(float* a = &state.axes[0])
            {
                const int size = sizeof(float) * 6;
                Buffer.MemoryCopy(gamePadState.Axes, a, size, size);
            }

            return state;
        }

        public static void Free (GamePadStateUnmanaged unmanaged) { }
    }
}