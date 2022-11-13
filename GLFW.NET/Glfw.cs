using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using JetBrains.Annotations;

using static System.Runtime.InteropServices.RuntimeInformation;

#pragma warning disable 0419

namespace GLFW
{
    /// <summary>
    ///     The base class the vast majority of the GLFW functions, excluding only Vulkan and native platform specific
    ///     functions.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static partial class Glfw
    {
        #region Fields and Constants

        /// <summary>
        ///     The native library name,
        ///     <para>For Unix users using an installed version of GLFW, this needs refactored to <c>glfw</c>.</para>
        /// </summary>
        public const string LIBRARY = "glfw";

        private static readonly ErrorCallback errorCallback = GlfwError;

        #endregion

        #region Constructors

        static Glfw()
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), LibraryImporterResolver);

            Init();
            SetErrorCallback(errorCallback);
        }

        private static IntPtr LibraryImporterResolver (string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == LIBRARY)
            {
                if (IsOSPlatform(OSPlatform.Windows)) return NativeLibrary.Load("glfw3", assembly, searchPath);
                if (IsOSPlatform(OSPlatform.Linux)) return NativeLibrary.Load("glfw", assembly, searchPath);
                if (IsOSPlatform(OSPlatform.OSX)) return NativeLibrary.Load("libglfw.3", assembly, searchPath);
            }

            return IntPtr.Zero;
        }

        #endregion

        /// <summary>
        ///     Returns and clears the error code of the last error that occurred on the calling thread, and optionally
        ///     a description of it.
        ///     <para>
        ///         If no error has occurred since the last call, it returns <see cref="ErrorCode.None" /> and the
        ///         description pointer is set to <c>null</c>.
        ///     </para>
        /// </summary>
        /// <param name="description">The description string, or <c>null</c> if there is no error.</param>
        /// <returns>The error code.</returns>
        public static ErrorCode GetError(out string description)
        {
            var code = GetErrorPrivate(out var ptr);
            description = code == ErrorCode.None ? null : Util.PtrToStringUTF8(ptr);
            return code;
        }

        /// <summary>
        ///     Retrieves the content scale for the specified monitor. The content scale is the ratio between the
        ///     current DPI and the platform's default DPI.
        ///     <para>
        ///         This is especially important for text and any UI elements. If the pixel dimensions of your UI scaled by
        ///         this look appropriate on your machine then it should appear at a reasonable size on other machines
        ///         regardless of their DPI and scaling settings. This relies on the system DPI and scaling settings being
        ///         somewhat correct.
        ///     </para>
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <param name="xScale">The scale on the x-axis.</param>
        /// <param name="yScale">The scale on the y-axis.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetMonitorContentScale")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]        
        public static partial void GetMonitorContentScale(IntPtr monitor, out float xScale, out float yScale);

        /// <summary>
        ///     Returns the current value of the user-defined pointer of the specified <paramref name="monitor" />.
        /// </summary>
        /// <param name="monitor">The monitor whose pointer to return.</param>
        /// <returns>The user-pointer, or <see cref="IntPtr.Zero" /> if none is defined.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetMonitorUserPointer")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial IntPtr GetMonitorUserPointer(IntPtr monitor);

        /// <summary>
        ///     This function sets the user-defined pointer of the specified <paramref name="monitor" />.
        ///     <para>The current value is retained until the monitor is disconnected.</para>
        /// </summary>
        /// <param name="monitor">The monitor whose pointer to set.</param>
        /// <param name="pointer">The user-defined pointer value.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetMonitorUserPointer")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetMonitorUserPointer(IntPtr monitor, IntPtr pointer);

        /// <summary>
        ///     Returns the opacity of the window, including any decorations.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>The opacity value of the specified window, a value between <c>0.0</c> and <c>1.0</c> inclusive.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetWindowOpacity")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial float GetWindowOpacity(IntPtr window);

        /// <summary>
        ///     Sets the opacity of the window, including any decorations.
        ///     <para>
        ///         The opacity (or alpha) value is a positive finite number between zero and one, where zero is fully
        ///         transparent and one is fully opaque.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to set the opacity for.</param>
        /// <param name="opacity">The desired opacity of the specified window.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowOpacity")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowOpacity(IntPtr window, float opacity);

        /// <summary>
        ///     Sets hints for the next call to <see cref="CreateWindow" />. The hints, once set, retain their values until
        ///     changed by a call to this function or <see cref="DefaultWindowHints" />, or until the library is terminated.
        ///     <para>
        ///         Some hints are platform specific. These may be set on any platform but they will only affect their
        ///         specific platform. Other platforms will ignore them. Setting these hints requires no platform specific
        ///         headers or functions.
        ///     </para>
        /// </summary>
        /// <param name="hint">The window hit to set.</param>
        /// <param name="value">The new value of the window hint.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwWindowHintString")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void WindowHintString(Hint hint, byte[] value);

        /// <summary>
        ///     Helper function to call <see cref="WindowHintString(Hint, byte[])" /> with UTF-8 encoding.
        /// </summary>
        /// <param name="hint">The window hit to set.</param>
        /// <param name="value">The new value of the window hint.</param>
        // ReSharper disable once InconsistentNaming
        public static void WindowHintStringUTF8(Hint hint, string value)
        {
            WindowHintString(hint, Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        ///     Helper function to call <see cref="WindowHintString(Hint, byte[])" /> with ASCII encoding.
        /// </summary>
        /// <param name="hint">The window hit to set.</param>
        /// <param name="value">The new value of the window hint.</param>
        // ReSharper disable once InconsistentNaming
        public static void WindowHintStringASCII(Hint hint, string value)
        {
            WindowHintString(hint, Encoding.ASCII.GetBytes(value));
        }

        /// <summary>
        ///     Retrieves the content scale for the specified window. The content scale is the ratio between the current DPI and
        ///     the platform's default DPI. This is especially important for text and any UI elements. If the pixel dimensions of
        ///     your UI scaled by this look appropriate on your machine then it should appear at a reasonable size on other
        ///     machines regardless of their DPI and scaling settings. This relies on the system DPI and scaling settings being
        ///     somewhat correct.
        ///     <para>
        ///         On systems where each monitors can have its own content scale, the window content scale will depend on which
        ///         monitor the system considers the window to be on.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <param name="xScale">The content scale on the x-axis.</param>
        /// <param name="yScale">The content scale on the y-axis.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetWindowContentScale")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetWindowContentScale(IntPtr window, out float xScale, out float yScale);

        /// <summary>
        ///     Requests user attention to the specified <paramref name="window" />. On platforms where this is not supported,
        ///     attention is
        ///     requested to the application as a whole.
        ///     <para>
        ///         Once the user has given attention, usually by focusing the window or application, the system will end the
        ///         request automatically.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to request user attention to.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwRequestWindowAttention")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void RequestWindowAttention(IntPtr window);

        /// <summary>
        ///     This function returns whether raw mouse motion is supported on the current system.
        ///     <para>
        ///         This status does not change after GLFW has been initialized so you only need to check this once. If you
        ///         attempt to enable raw motion on a system that does not support it, an error will be emitted.
        ///     </para>
        /// </summary>
        /// <returns><c>true</c> if raw mouse motion is supported on the current machine, or <c>false</c> otherwise.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwRawMouseMotionSupported")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool RawMouseMotionSupported();

        /// <summary>
        ///     Sets the maximization callback of the specified <paramref name="window," /> which is called when the window is
        ///     maximized or restored.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cb">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowMaximizeCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial WindowMaximizedCallback SetWindowMaximizeCallback(IntPtr window,
            WindowMaximizedCallback cb);

        /// <summary>
        ///     Sets the window content scale callback of the specified window, which is called when the content scale of the
        ///     specified window changes.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cb">The new callback, or <c>null</c> to remove the currently set callback</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowContentScaleCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial WindowContentsScaleCallback SetWindowContentScaleCallback(IntPtr window,
            WindowContentsScaleCallback cb);

        /// <summary>
        ///     Returns the platform-specific scan-code of the specified key.
        ///     <para>If the key is <see cref="Keys.Unknown" /> or does not exist on the keyboard this method will return -1.</para>
        /// </summary>
        /// <param name="key">The named key to query.</param>
        /// <returns>The platform-specific scan-code for the key, or -1 if an error occurred.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetKeyScancode")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial int GetKeyScanCode(Keys key);

        /// <summary>
        ///     Sets the value of an attribute of the specified window.
        /// </summary>
        /// <param name="window">
        ///     The window to set the attribute for
        ///     <para>Valid attributes include:</para>
        ///     <para>
        ///         <see cref="WindowAttribute.Decorated" />
        ///     </para>
        ///     <para>
        ///         <see cref="WindowAttribute.Resizable" />
        ///     </para>
        ///     <para>
        ///         <see cref="WindowAttribute.Floating" />
        ///     </para>
        ///     <para>
        ///         <see cref="WindowAttribute.AutoIconify" />
        ///     </para>
        ///     <para>
        ///         <see cref="WindowAttribute.Focused" />
        ///     </para>
        /// </param>
        /// <param name="attr">A supported window attribute.</param>
        /// <param name="value">The value to set.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowAttrib")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowAttribute(IntPtr window, WindowAttribute attr, [MarshalAs(UnmanagedType.Bool)] bool value);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetJoystickHats")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetJoystickHats(int joystickId, out int count);

        /// <summary>
        ///     Returns the state of all hats of the specified joystick as a bitmask.
        /// </summary>
        /// <param name="joystickId">The joystick to query.</param>
        /// <returns>A bitmask enumeration containing the state of the joystick hats.</returns>
        public static Hat GetJoystickHats(int joystickId)
        {
            var hat = Hat.Centered;
            var ptr = GetJoystickHats(joystickId, out var count);
            for (var i = 0; i < count; i++)
            {
                var value = Marshal.ReadByte(ptr, i);
                hat |= (Hat) value;
            }

            return hat;
        }

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetJoystickGUID")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetJoystickGuidPrivate(int joystickId);

        /// <summary>
        ///     Returns the SDL compatible GUID, as a hexadecimal string, of the specified joystick.
        ///     <para>
        ///         The GUID is what connects a joystick to a gamepad mapping. A connected joystick will always have a GUID even
        ///         if there is no gamepad mapping assigned to it.
        ///     </para>
        /// </summary>
        /// <param name="joystickId">The joystick to query.</param>
        /// <returns>The GUID of the joystick, or <c>null</c> if the joystick is not present or an error occurred.</returns>
        public static string GetJoystickGuid(int joystickId)
        {
            var ptr = GetJoystickGuidPrivate(joystickId);
            return ptr == IntPtr.Zero ? null : Util.PtrToStringUTF8(ptr);
        }

        /// <summary>
        ///     This function returns the current value of the user-defined pointer of the specified joystick.
        /// </summary>
        /// <param name="joystickId">The joystick whose pointer to return.</param>
        /// <returns>The user-defined pointer, or <see cref="IntPtr.Zero" /> if never defined.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetJoystickUserPointer")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial IntPtr GetJoystickUserPointer(int joystickId);

        /// <summary>
        ///     This function sets the user-defined pointer of the specified joystick.
        ///     <para>The current value is retained until the joystick is disconnected.</para>
        /// </summary>
        /// <param name="joystickId">The joystick whose pointer to set.</param>
        /// <param name="pointer">The new value.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetJoystickUserPointer")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetJoystickUserPointer(int joystickId, IntPtr pointer);

        /// <summary>
        ///     Returns whether the specified joystick is both present and has a gamepad mapping.
        /// </summary>
        /// <param name="joystickId">The joystick to query.</param>
        /// <returns><c>true</c> if a joystick is both present and has a gamepad mapping, or <c>false</c> otherwise.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwJoystickIsGamepad")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool JoystickIsGamepad(int joystickId);

        [LibraryImport(LIBRARY, EntryPoint = "glfwUpdateGamepadMappings")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UpdateGamepadMappings([NotNull] byte[] mappings);

        /// <summary>
        ///     Parses the specified string and updates the internal list with any gamepad mappings it finds.
        ///     <para>
        ///         This string may contain either a single gamepad mapping or many mappings separated by newlines. The parser
        ///         supports the full format of the SDL <c>gamecontrollerdb.txt</c> source file including empty lines and comments.
        ///     </para>
        /// </summary>
        /// <param name="mappings">The string containing the gamepad mappings.</param>
        /// <returns><c>true</c> if successful, or <c>false</c> if an error occurred.</returns>
        public static bool UpdateGamepadMappings(string mappings)
        {
            return UpdateGamepadMappings(Encoding.ASCII.GetBytes(mappings));
        }

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetGamepadName")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetGamepadNamePrivate(int gamepadId);

        /// <summary>
        ///     Returns the human-readable name of the gamepad from the gamepad mapping assigned to the specified joystick.
        /// </summary>
        /// <param name="gamepadId">The joystick to query.</param>
        /// <returns>
        ///     The name of the gamepad, or <c>null</c> if the joystick is not present, does not have a mapping or an error
        ///     occurred.
        /// </returns>
        public static string GetGamepadName(int gamepadId)
        {
            var ptr = GetGamepadNamePrivate(gamepadId);
            return ptr == IntPtr.Zero ? null : Util.PtrToStringUTF8(ptr);
        }

        /// <summary>
        ///     Retrieves the state of the specified joystick remapped to an Xbox-like gamepad.
        /// </summary>
        /// <param name="id">The joystick to query.</param>
        /// <param name="state">The gamepad input state of the joystick.</param>
        /// <returns>
        ///     <c>true</c> if successful, or <c>false</c> if no joystick is connected, it has no gamepad mapping or an error
        ///     occurred.
        /// </returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetGamepadState")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetGamepadState(int id, out GamePadState state);

        #region Properties

        /// <summary>
        ///     Gets the window whose OpenGL or OpenGL ES context is current on the calling thread, or <see cref="Window.None" />
        ///     if no context is current.
        /// </summary>
        /// <value>
        ///     The current context.
        /// </value>
        public static Window CurrentContext => GetCurrentContext();

        /// <summary>
        ///     Gets an array of handles for all currently connected monitors.
        ///     <para>The primary monitor is always first in the array.</para>
        /// </summary>
        /// <value>
        ///     The monitors.
        /// </value>
        public static Monitor[] Monitors
        {
            get
            {
                var ptr = GetMonitors(out var count);
                var monitors = new Monitor[count];
                var offset = 0;
                for (var i = 0; i < count; i++, offset += IntPtr.Size)
                {
                    monitors[i] = Marshal.PtrToStructure<Monitor>(ptr + offset);
                }

                return monitors;
            }
        }

        /// <summary>
        ///     Gets the primary monitor. This is usually the monitor where elements like the task bar or global menu bar are
        ///     located.
        /// </summary>
        /// <value>
        ///     The primary monitor, or <see cref="Monitor.None" /> if no monitors were found or if an error occurred.
        /// </value>
        public static Monitor PrimaryMonitor => GetPrimaryMonitor();

        /// <summary>
        ///     Gets or sets the value of the GLFW timer.
        ///     <para>
        ///         The resolution of the timer is system dependent, but is usually on the order of a few micro- or nanoseconds.
        ///         It uses the highest-resolution monotonic time source on each supported platform.
        ///     </para>
        /// </summary>
        /// <value>
        ///     The time.
        /// </value>
        public static double Time
        {
            get => GetTime();
            set => SetTime(value);
        }

        /// <summary>
        ///     Gets the frequency, in Hz, of the raw timer.
        /// </summary>
        /// <value>
        ///     The frequency of the timer, in Hz, or zero if an error occurred.
        /// </value>
        public static ulong TimerFrequency => GetTimerFrequency();

        /// <summary>
        ///     Gets the current value of the raw timer, measured in 1 / frequency seconds.
        /// </summary>
        /// <value>
        ///     The timer value.
        /// </value>
        public static ulong TimerValue => GetTimerValue();

        /// <summary>
        ///     Gets the version of the native GLFW library.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public static Version Version
        {
            get
            {
                GetVersion(out var major, out var minor, out var revision);
                return new Version(major, minor, revision);
            }
        }

        /// <summary>
        ///     Gets the compile-time generated version string of the GLFW library binary.
        ///     <para>It describes the version, platform, compiler and any platform-specific compile-time options.</para>
        /// </summary>
        /// <value>
        ///     The version string.
        /// </value>
        public static string VersionString => Util.PtrToStringUTF8(GetVersionString());

        #endregion

        #region External

        /// <summary>
        ///     This function sets hints for the next initialization of GLFW.
        ///     <para>
        ///         The values you set hints to are never reset by GLFW, but they only take effect during initialization.
        ///         Once GLFW has been initialized, any values you set will be ignored until the library is terminated and
        ///         initialized again.>.
        ///     </para>
        /// </summary>
        /// <param name="hint">
        ///     The hint, valid values are <see cref="Hint.JoystickHatButtons" />,
        ///     <see cref="Hint.CocoaMenuBar" />, and <see cref="Hint.CocoaChDirResources" />.
        /// </param>
        /// <param name="value">The value of the hint.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwInitHint")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void InitHint(Hint hint, [MarshalAs(UnmanagedType.Bool)] bool value);

        /// <summary>
        ///     This function initializes the GLFW library. Before most GLFW functions can be used, GLFW must be initialized, and
        ///     before an application terminates GLFW should be terminated in order to free any resources allocated during or after
        ///     initialization.
        ///     <para>
        ///         If this function fails, it calls <see cref="Terminate" /> before returning. If it succeeds, you should call
        ///         <see cref="Terminate" /> before the application exits
        ///     </para>
        ///     <para>
        ///         Additional calls to this function after successful initialization but before termination will return
        ///         <c>true</c> immediately.
        ///     </para>
        /// </summary>
        /// <returns><c>true</c> if successful, or <c>false</c> if an error occurred.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwInit")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool Init();

        /// <summary>
        ///     This function destroys all remaining windows and cursors, restores any modified gamma ramps and frees any other
        ///     allocated resources. Once this function is called, you must again call <see cref="Init" /> successfully before you
        ///     will be able to use most GLFW functions.
        ///     If GLFW has been successfully initialized, this function should be called before the application exits. If
        ///     initialization fails, there is no need to call this function, as it is called by <see cref="Init" /> before it
        ///     returns failure.
        /// </summary>
        /// <note type="warning">
        ///     The contexts of any remaining windows must not be current on any other thread when this function
        ///     is called.
        /// </note>
        [LibraryImport(LIBRARY, EntryPoint = "glfwTerminate")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void Terminate();

        /// <summary>
        ///     Sets the error callback, which is called with an error code and a human-readable description each
        ///     time a GLFW error occurs.
        /// </summary>
        /// <param name="errorHandler">The callback function, or <c>null</c> to unbind this callback.</param>
        /// <returns>The previously set callback function, or <c>null</c> if no callback was already set.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetErrorCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial ErrorCallback SetErrorCallback([MarshalAs(UnmanagedType.FunctionPtr)] ErrorCallback errorHandler);

        [LibraryImport(LIBRARY, EntryPoint = "glfwCreateWindow")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial Window CreateWindow(int width, int height, byte[] title, Monitor monitor, Window share);

        /// <summary>
        ///     This function destroys the specified window and its context. On calling this function, no further callbacks will be
        ///     called for that window.
        ///     <para>If the context of the specified window is current on the main thread, it is detached before being destroyed.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwDestroyWindow")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void DestroyWindow(Window window);

        /// <summary>
        ///     This function makes the specified window visible if it was previously hidden. If the window is already visible or
        ///     is in full screen mode, this function does nothing.
        /// </summary>
        /// <param name="window">A window instance.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwShowWindow")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void ShowWindow(Window window);

        /// <summary>
        ///     This function hides the specified window if it was previously visible. If the window is already hidden or is in
        ///     full screen mode, this function does nothing.
        /// </summary>
        /// <param name="window">A window instance.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwHideWindow")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void HideWindow(Window window);

        /// <summary>
        ///     This function retrieves the position, in screen coordinates, of the upper-left corner of the client area of the
        ///     specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the client area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the client area.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetWindowPos")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetWindowPosition(Window window, out int x, out int y);

        /// <summary>
        ///     Sets the position, in screen coordinates, of the upper-left corner of the client area of the
        ///     specified windowed mode window.
        ///     <para>If the window is a full screen window, this function does nothing.</para>
        /// </summary>
        /// <note type="important">
        ///     Do not use this function to move an already visible window unless you have very good reasons for
        ///     doing so, as it will confuse and annoy the user.
        /// </note>
        /// <param name="window">A window instance.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the client area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the client area.</param>
        /// <remarks>
        ///     The window manager may put limits on what positions are allowed. GLFW cannot and should not override these
        ///     limits.
        /// </remarks>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowPos")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowPosition(Window window, int x, int y);

        /// <summary>
        ///     This function retrieves the size, in screen coordinates, of the client area of the specified window.
        ///     <para>
        ///         If you wish to retrieve the size of the framebuffer of the window in pixels, use
        ///         <see cref="GetFramebufferSize" />.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="width">The width, in screen coordinates.</param>
        /// <param name="height">The height, in screen coordinates.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetWindowSize")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetWindowSize(Window window, out int width, out int height);

        /// <summary>
        ///     Sets the size, in screen coordinates, of the client area of the specified window.
        ///     <para>
        ///         For full screen windows, this function updates the resolution of its desired video mode and switches to the
        ///         video mode closest to it, without affecting the window's context. As the context is unaffected, the bit depths
        ///         of the framebuffer remain unchanged.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="width">The desired width, in screen coordinates, of the window client area.</param>
        /// <param name="height">The desired height, in screen coordinates, of the window client area.</param>
        /// <remarks>The window manager may put limits on what sizes are allowed. GLFW cannot and should not override these limits.</remarks>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowSize")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowSize(Window window, int width, int height);

        /// <summary>
        ///     This function retrieves the size, in pixels, of the framebuffer of the specified window.
        ///     <para>If you wish to retrieve the size of the window in screen coordinates, use <see cref="GetWindowSize" />.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="width">The width, in pixels, of the framebuffer.</param>
        /// <param name="height">The height, in pixels, of the framebuffer.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetFramebufferSize")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetFramebufferSize(Window window, out int width, out int height);

        /// <summary>
        ///     Sets the position callback of the specified window, which is called when the window is moved.
        ///     <para>The callback is provided with the screen position of the upper-left corner of the client area of the window.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="positionCallback">The position callback to be invoked on position changes.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowPosCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial PositionCallback SetWindowPositionCallback(Window window,
            [MarshalAs(UnmanagedType.FunctionPtr)] PositionCallback positionCallback);

        /// <summary>
        ///     Sets the size callback of the specified window, which is called when the window is resized.
        ///     <para>The callback is provided with the size, in screen coordinates, of the client area of the window.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="sizeCallback">The size callback to be invoked on size changes.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowSizeCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial SizeCallback SetWindowSizeCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] SizeCallback sizeCallback);

        /// <summary>
        ///     Sets the window title, encoded as UTF-8, of the specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="title">The title as an array of UTF-8 encoded bytes.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowTitle")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial void SetWindowTitle(Window window, byte[] title);

        /// <summary>
        ///     This function brings the specified window to front and sets input focus. The window should already be visible and
        ///     not iconified.
        /// </summary>
        /// <param name="window">The window.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwFocusWindow")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void FocusWindow(Window window);

        /// <summary>
        ///     Sets the focus callback of the specified window, which is called when the window gains or loses input
        ///     focus.
        ///     <para>
        ///         After the focus callback is called for a window that lost input focus, synthetic key and mouse button release
        ///         events will be generated for all such that had been pressed.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="focusCallback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowFocusCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial FocusCallback SetWindowFocusCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] FocusCallback focusCallback);

        /// <summary>
        ///     This function retrieves the major, minor and revision numbers of the GLFW library.
        ///     <para>
        ///         It is intended for when you are using GLFW as a shared library and want to ensure that you are using the
        ///         minimum required version.
        ///     </para>
        /// </summary>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        /// <param name="revision">The revision.</param>
        /// <seealso cref="Version" />
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetVersion")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetVersion(out int major, out int minor, out int revision);

        /// <summary>
        ///     Gets the compile-time generated version string of the GLFW library binary.
        ///     <para>It describes the version, platform, compiler and any platform-specific compile-time options.</para>
        /// </summary>
        /// <returns>A pointer to the null-terminated UTF-8 encoded version string.</returns>
        /// <seealso cref="VersionString" />
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetVersionString")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetVersionString();

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetTime")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial double GetTime();

        [LibraryImport(LIBRARY, EntryPoint = "glfwSetTime")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial void SetTime(double time);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetTimerFrequency")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial ulong GetTimerFrequency();

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetTimerValue")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial ulong GetTimerValue();

        /// <summary>
        ///     This function retrieves the size, in screen coordinates, of each edge of the frame of the specified window.
        ///     <para>
        ///         This size includes the title bar, if the window has one. The size of the frame may vary depending on the
        ///         window-related hints used to create it.
        ///     </para>
        ///     <para>
        ///         Because this function retrieves the size of each window frame edge and not the offset along a particular
        ///         coordinate axis, the retrieved values will always be zero or positive.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="left">The size, in screen coordinates, of the left edge of the window frame</param>
        /// <param name="top">The size, in screen coordinates, of the top edge of the window frame</param>
        /// <param name="right">The size, in screen coordinates, of the right edge of the window frame.</param>
        /// <param name="bottom">The size, in screen coordinates, of the bottom edge of the window frame</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetWindowFrameSize")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetWindowFrameSize(Window window, out int left, out int top, out int right,
            out int bottom);

        /// <summary>
        ///     This function maximizes the specified window if it was previously not maximized. If the window is already
        ///     maximized, this function does nothing.
        ///     <para>If the specified window is a full screen window, this function does nothing.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwMaximizeWindow")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void MaximizeWindow(Window window);

        /// <summary>
        ///     This function iconifies (minimizes) the specified window if it was previously restored.
        ///     <para>If the window is already iconified, this function does nothing.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwIconifyWindow")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void IconifyWindow(Window window);

        /// <summary>
        ///     This function restores the specified window if it was previously iconified (minimized) or maximized.
        ///     <para>If the window is already restored, this function does nothing.</para>
        ///     <para>
        ///         If the specified window is a full screen window, the resolution chosen for the window is restored on the
        ///         selected monitor.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwRestoreWindow")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void RestoreWindow(Window window);

        /// <summary>
        ///     This function makes the OpenGL or OpenGL ES context of the specified window current on the calling thread.
        ///     <para>
        ///         A context can only be made current on a single thread at a time and each thread can have only a single
        ///         current context at a time.
        ///     </para>
        ///     <para>By default, making a context non-current implicitly forces a pipeline flush.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwMakeContextCurrent")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void MakeContextCurrent(Window window);

        /// <summary>
        ///     This function swaps the front and back buffers of the specified window when rendering with OpenGL or OpenGL ES.
        ///     <para>
        ///         If the swap interval is greater than zero, the GPU driver waits the specified number of screen updates before
        ///         swapping the buffers.
        ///     </para>
        ///     <para>This function does not apply to Vulkan.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSwapBuffers")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SwapBuffers(Window window);

        /// <summary>
        ///     Sets the swap interval for the current OpenGL or OpenGL ES context, i.e. the number of screen updates
        ///     to wait from the time <see cref="SwapBuffers" /> was called before swapping the buffers and returning.
        ///     <para>This is sometimes called vertical synchronization, vertical retrace synchronization or just vsync.</para>
        ///     <para>
        ///         A context must be current on the calling thread. Calling this function without a current context will cause
        ///         an exception.
        ///     </para>
        ///     <para>
        ///         This function does not apply to Vulkan. If you are rendering with Vulkan, see the present mode of your
        ///         swapchain instead.
        ///     </para>
        /// </summary>
        /// <param name="interval">
        ///     The minimum number of screen updates to wait for until the buffers are swapped by
        ///     <see cref="SwapBuffers" />.
        /// </param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSwapInterval")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SwapInterval(int interval);

        /// <summary>
        ///     Gets whether the specified API extension is supported by the current OpenGL or OpenGL ES context.
        ///     <para>It searches both for client API extension and context creation API extensions.</para>
        /// </summary>
        /// <param name="extension">The extension name as an array of ASCII encoded bytes.</param>
        /// <returns><c>true</c> if the extension is supported; otherwise <c>false</c>.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwExtensionSupported")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetExtensionSupported(byte[] extension);

        /// <summary>
        ///     This function resets all window hints to their default values.
        /// </summary>
        [LibraryImport(LIBRARY, EntryPoint = "glfwDefaultWindowHints")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void DefaultWindowHints();

        /// <summary>
        ///     Gets the value of the close flag of the specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns><c>true</c> if close flag is present; otherwise <c>false</c>.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwWindowShouldClose")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WindowShouldClose(Window window);

        /// <summary>
        ///     Sets the value of the close flag of the specified window.
        ///     <para>This can be used to override the user's attempt to close the window, or to signal that it should be closed.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="close"><c>true</c> to set close flag, or <c>false</c> to cancel flag.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowShouldClose")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowShouldClose(Window window, [MarshalAs(UnmanagedType.Bool)] bool close);

        /// <summary>
        ///     Sets the icon of the specified window. If passed an array of candidate images, those of or closest to
        ///     the sizes desired by the system are selected. If no images are specified, the window reverts to its default icon.
        ///     <para>
        ///         The desired image sizes varies depending on platform and system settings. The selected images will be
        ///         rescaled as needed. Good sizes include 16x16, 32x32 and 48x48.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="count">The number of images in <paramref name="images" />.</param>
        /// <param name="images">An array of icon images.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowIcon")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowIcon(Window window, int count, Image[] images);

        /// <summary>
        ///     This function puts the calling thread to sleep until at least one event is available in the event queue. Once one
        ///     or more events are available, it behaves exactly like glfwPollEvents, i.e. the events in the queue are processed
        ///     and the function then returns immediately. Processing events will cause the window and input callbacks associated
        ///     with those events to be called.
        ///     <para>
        ///         Since not all events are associated with callbacks, this function may return without a callback having been
        ///         called even if you are monitoring all callbacks.
        ///     </para>
        ///     <para>
        ///         On some platforms, a window move, resize or menu operation will cause event processing to block. This is due
        ///         to how event processing is designed on those platforms. You can use the window refresh callback to redraw the
        ///         contents of your window when necessary during such operations.
        ///     </para>
        /// </summary>
        [LibraryImport(LIBRARY, EntryPoint = "glfwWaitEvents")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void WaitEvents();

        /// <summary>
        ///     This function processes only those events that are already in the event queue and then returns immediately.
        ///     Processing events will cause the window and input callbacks associated with those events to be called.
        ///     <para>
        ///         On some platforms, a window move, resize or menu operation will cause event processing to block. This is due
        ///         to how event processing is designed on those platforms. You can use the window refresh callback to redraw the
        ///         contents of your window when necessary during such operations.
        ///     </para>
        ///     <para>
        ///         On some platforms, certain events are sent directly to the application without going through the event queue,
        ///         causing callbacks to be called outside of a call to one of the event processing functions.
        ///     </para>
        /// </summary>
        [LibraryImport(LIBRARY, EntryPoint = "glfwPollEvents")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void PollEvents();

        /// <summary>
        ///     This function posts an empty event from the current thread to the event queue, causing <see cref="WaitEvents" /> or
        ///     <see cref="WaitEventsTimeout " /> to return.
        /// </summary>
        [LibraryImport(LIBRARY, EntryPoint = "glfwPostEmptyEvent")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void PostEmptyEvent();

        /// <summary>
        ///     This function puts the calling thread to sleep until at least one event is available in the event queue, or until
        ///     the specified timeout is reached. If one or more events are available, it behaves exactly like
        ///     <see cref="PollEvents" />, i.e. the events in the queue are processed and the function then returns immediately.
        ///     Processing events will cause the window and input callbacks associated with those events to be called.
        ///     <para>The timeout value must be a positive finite number.</para>
        ///     <para>
        ///         Since not all events are associated with callbacks, this function may return without a callback having been
        ///         called even if you are monitoring all callbacks.
        ///     </para>
        ///     <para>
        ///         On some platforms, a window move, resize or menu operation will cause event processing to block. This is due
        ///         to how event processing is designed on those platforms. You can use the window refresh callback to redraw the
        ///         contents of your window when necessary during such operations.
        ///     </para>
        /// </summary>
        /// <param name="timeout">The maximum amount of time, in seconds, to wait.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwWaitEventsTimeout")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void WaitEventsTimeout(double timeout);

        /// <summary>
        ///     Sets the close callback of the specified window, which is called when the user attempts to close the
        ///     window, for example by clicking the close widget in the title bar.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="closeCallback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowCloseCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial WindowCallback SetCloseCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] WindowCallback closeCallback);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetPrimaryMonitor")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial Monitor GetPrimaryMonitor();

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetVideoMode")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetVideoModeInternal(Monitor monitor);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetVideoModes")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetVideoModes(Monitor monitor, out int count);

        /// <summary>
        ///     Gets the handle of the monitor that the specified window is in full screen on.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns>The monitor, or <see cref="Monitor.None" /> if the window is in windowed mode or an error occurred.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetWindowMonitor")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial Monitor GetWindowMonitor(Window window);

        /// <summary>
        ///     Sets the monitor that the window uses for full screen mode or, if the monitor is
        ///     <see cref="Monitor.None" />, makes it windowed mode.
        ///     <para>
        ///         When setting a monitor, this function updates the width, height and refresh rate of the desired video mode
        ///         and switches to the video mode closest to it. The window position is ignored when setting a monitor.
        ///     </para>
        ///     <para>
        ///         When the monitor is <see cref="Monitor.None" />, the position, width and height are used to place the window
        ///         client area. The refresh rate is ignored when no monitor is specified.
        ///     </para>
        ///     <para>
        ///         If you only wish to update the resolution of a full screen window or the size of a windowed mode window, use
        ///         <see cref="SetWindowSize" />.
        ///     </para>
        ///     <para>
        ///         When a window transitions from full screen to windowed mode, this function restores any previous window
        ///         settings such as whether it is decorated, floating, resizable, has size or aspect ratio limits, etc..
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="monitor">The desired monitor, or <see cref="Monitor.None" /> to set windowed mode.</param>
        /// <param name="x">The desired x-coordinate of the upper-left corner of the client area.</param>
        /// <param name="y">The desired y-coordinate of the upper-left corner of the client area.</param>
        /// <param name="width">The desired width, in screen coordinates, of the client area or video mode.</param>
        /// <param name="height">The desired height, in screen coordinates, of the client area or video mode.</param>
        /// <param name="refreshRate">The desired refresh rate, in Hz, of the video mode, or <see cref="Constants.Default" />.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowMonitor")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowMonitor(Window window, Monitor monitor, int x, int y, int width, int height,
            int refreshRate);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetGammaRamp")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        internal static partial IntPtr GetGammaRampInternal(Monitor monitor);

        /// <summary>
        ///     Sets the current gamma ramp for the specified monitor.
        ///     <para>
        ///         The original gamma ramp for that monitor is saved by GLFW the first time this function is called and is
        ///         restored by <see cref="Terminate" />.
        ///     </para>
        ///     <para>WARNING: Gamma ramps with sizes other than 256 are not supported on some platforms (Windows).</para>
        /// </summary>
        /// <param name="monitor">The monitor whose gamma ramp to set.</param>
        /// <param name="gammaRamp">The gamma ramp to use.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetGammaRamp")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetGammaRamp(Monitor monitor, GammaRamp gammaRamp);

        /// <summary>
        ///     This function generates a 256-element gamma ramp from the specified exponent and then calls
        ///     <see cref="SetGammaRamp" /> with it.
        ///     <para>The value must be a finite number greater than zero.</para>
        /// </summary>
        /// <param name="monitor">The monitor whose gamma ramp to set.</param>
        /// <param name="gamma">The desired exponent.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetGamma")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetGamma(Monitor monitor, float gamma);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetClipboardString")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetClipboardStringInternal(Window window);

        [LibraryImport(LIBRARY, EntryPoint = "glfwSetClipboardString")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial void SetClipboardString(Window window, byte[] bytes);

        /// <summary>
        ///     Sets the file drop callback of the specified window, which is called when one or more dragged files
        ///     are dropped on the window.
        ///     <para>
        ///         Because the path array and its strings may have been generated specifically for that event, they are not
        ///         guaranteed to be valid after the callback has returned. If you wish to use them after the callback returns, you
        ///         need to make a deep copy.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="dropCallback">The new file drop callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetDropCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial FileDropCallback SetDropCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] FileDropCallback dropCallback);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetMonitorName")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetMonitorNameInternal(Monitor monitor);

        /// <summary>
        ///     Creates a new custom cursor image that can be set for a window with glfwSetCursor.
        ///     <para>
        ///         The cursor can be destroyed with <see cref="DestroyCursor" />. Any remaining cursors are destroyed by
        ///         <see cref="Terminate" />.
        ///     </para>
        ///     <para>
        ///         The pixels are 32-bit, little-endian, non-premultiplied RGBA, i.e. eight bits per channel. They are arranged
        ///         canonically as packed sequential rows, starting from the top-left corner.
        ///     </para>
        ///     <para>
        ///         The cursor hotspot is specified in pixels, relative to the upper-left corner of the cursor image. Like all
        ///         other coordinate systems in GLFW, the X-axis points to the right and the Y-axis points down.
        ///     </para>
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="xHotspot">The x hotspot.</param>
        /// <param name="yHotspot">The y hotspot.</param>
        /// <returns>The created cursor.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwCreateCursor")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial Cursor CreateCursor(Image image, int xHotspot, int yHotspot);

        /// <summary>
        ///     This function destroys a cursor previously created with <see cref="CreateCursor" />. Any remaining cursors will be
        ///     destroyed by <see cref="Terminate" />.
        /// </summary>
        /// <param name="cursor">The cursor object to destroy.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwDestroyCursor")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void DestroyCursor(Cursor cursor);

        /// <summary>
        ///     Sets the cursor image to be used when the cursor is over the client area of the specified window.
        ///     <para>The set cursor will only be visible when the cursor mode of the window is <see cref="CursorMode.Normal" />.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="cursor">The cursor to set, or <see cref="Cursor.None" /> to switch back to the default arrow cursor.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetCursor")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetCursor(Window window, Cursor cursor);

        /// <summary>
        ///     Returns a cursor with a standard shape, that can be set for a window with <see cref="SetCursor" />.
        /// </summary>
        /// <param name="type">The type of cursor to create.</param>
        /// <returns>A new cursor ready to use or <see cref="Cursor.None" /> if an error occurred.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwCreateStandardCursor")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial Cursor CreateStandardCursor(CursorType type);

        /// <summary>
        ///     Gets the position of the cursor, in screen coordinates, relative to the upper-left corner of the
        ///     client area of the specified window
        ///     <para>
        ///         If the cursor is disabled then the cursor position is unbounded and limited only by the minimum and maximum
        ///         values of a double.
        ///     </para>
        ///     <para>
        ///         The coordinate can be converted to their integer equivalents with the floor function. Casting directly to an
        ///         integer type works for positive coordinates, but fails for negative ones.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="x">The cursor x-coordinate, relative to the left edge of the client area.</param>
        /// <param name="y">The cursor y-coordinate, relative to the left edge of the client area.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetCursorPos")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetCursorPosition(Window window, out double x, out double y);

        /// <summary>
        ///     Sets the position, in screen coordinates, of the cursor relative to the upper-left corner of the
        ///     client area of the specified window. The window must have input focus. If the window does not have input focus when
        ///     this function is called, it fails silently.
        ///     <para>
        ///         If the cursor mode is disabled then the cursor position is unconstrained and limited only by the minimum and
        ///         maximum values of a <see cref="double" />.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="x">The desired x-coordinate, relative to the left edge of the client area.</param>
        /// <param name="y">The desired y-coordinate, relative to the left edge of the client area.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetCursorPos")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetCursorPosition(Window window, double x, double y);

        /// <summary>
        ///     Sets the cursor position callback of the specified window, which is called when the cursor is moved.
        ///     <para>
        ///         The callback is provided with the position, in screen coordinates, relative to the upper-left corner of the
        ///         client area of the window.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="mouseCallback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or<c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetCursorPosCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial MouseCallback SetCursorPositionCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] MouseCallback mouseCallback);

        /// <summary>
        ///     Sets the cursor boundary crossing callback of the specified window, which is called when the cursor
        ///     enters or leaves the client area of the window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="mouseCallback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetCursorEnterCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial MouseEnterCallback SetCursorEnterCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] MouseEnterCallback mouseCallback);

        /// <summary>
        ///     Sets the mouse button callback of the specified window, which is called when a mouse button is
        ///     pressed or released.
        ///     <para>
        ///         When a window loses input focus, it will generate synthetic mouse button release events for all pressed mouse
        ///         buttons. You can tell these events from user-generated events by the fact that the synthetic ones are generated
        ///         after the focus loss event has been processed, i.e. after the window focus callback has been called.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="mouseCallback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetMouseButtonCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial MouseButtonCallback SetMouseButtonCallback(Window window,
            [MarshalAs(UnmanagedType.FunctionPtr)] MouseButtonCallback mouseCallback);

        /// <summary>
        ///     Sets the scroll callback of the specified window, which is called when a scrolling device is used,
        ///     such as a mouse wheel or scrolling area of a touchpad.
        ///     <para>The scroll callback receives all scrolling input, like that from a mouse wheel or a touchpad scrolling area.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="mouseCallback">	The new scroll callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetScrollCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial MouseCallback SetScrollCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] MouseCallback mouseCallback);

        /// <summary>
        ///     Gets the last state reported for the specified mouse button to the specified window.
        ///     <para>
        ///         If the <see cref="InputMode.StickyMouseButton" /> input mode is enabled, this function returns
        ///         <see cref="InputState.Press" /> the first time you call it for a mouse button that was pressed, even if that
        ///         mouse button has already been released.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="button">The desired mouse button.</param>
        /// <returns>The input state of the <paramref name="button" />.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetMouseButton")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial InputState GetMouseButton(Window window, MouseButton button);

        /// <summary>
        ///     Sets the user-defined pointer of the specified window. The current value is retained until the window
        ///     is destroyed. The initial value is <see cref="IntPtr.Zero" />.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="userPointer">The user pointer value.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowUserPointer")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowUserPointer(Window window, IntPtr userPointer);

        /// <summary>
        ///     Gets the current value of the user-defined pointer of the specified window. The initial value is
        ///     <see cref="IntPtr.Zero" />.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns>The user-defined pointer.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetWindowUserPointer")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial IntPtr GetWindowUserPointer(Window window);

        /// <summary>
        ///     Sets the size limits of the client area of the specified window. If the window is full screen, the
        ///     size limits only take effect once it is made windowed. If the window is not resizable, this function does nothing.
        ///     <para>The size limits are applied immediately to a windowed mode window and may cause it to be resized.</para>
        ///     <para>
        ///         The maximum dimensions must be greater than or equal to the minimum dimensions and all must be greater than
        ///         or equal to zero.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="minWidth">The minimum width of the client area.</param>
        /// <param name="minHeight">The minimum height of the client area.</param>
        /// <param name="maxWidth">The maximum width of the client area.</param>
        /// <param name="maxHeight">The maximum height of the client area.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowSizeLimits")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowSizeLimits(Window window, int minWidth, int minHeight, int maxWidth,
            int maxHeight);

        /// <summary>
        ///     Sets the required aspect ratio of the client area of the specified window. If the window is full
        ///     screen, the aspect ratio only takes effect once it is made windowed. If the window is not resizable, this function
        ///     does nothing.
        ///     <para>
        ///         The aspect ratio is specified as a numerator and a denominator and both values must be greater than zero. For
        ///         example, the common 16:9 aspect ratio is specified as 16 and 9, respectively.
        ///     </para>
        ///     <para>
        ///         If the numerator and denominator is set to <see cref="Constants.Default" /> then the aspect ratio limit is
        ///         disabled.
        ///     </para>
        ///     <para>The aspect ratio is applied immediately to a windowed mode window and may cause it to be resized.</para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="numerator">The numerator of the desired aspect ratio.</param>
        /// <param name="denominator">The denominator of the desired aspect ratio.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowAspectRatio")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetWindowAspectRatio(Window window, int numerator, int denominator);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetCurrentContext")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial Window GetCurrentContext();

        /// <summary>
        ///     Gets the size, in millimeters, of the display area of the specified monitor.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <param name="width">The width, in millimeters, of the monitor's display area.</param>
        /// <param name="height">The height, in millimeters, of the monitor's display area.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetMonitorPhysicalSize")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetMonitorPhysicalSize(Monitor monitor, out int width, out int height);

        /// <summary>
        ///     Gets the position, in screen coordinates, of the upper-left corner of the specified monitor.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <param name="x">The monitor x-coordinate.</param>
        /// <param name="y">The monitor y-coordinate.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetMonitorPos")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetMonitorPosition(Monitor monitor, out int x, out int y);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetMonitors")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetMonitors(out int count);

        /// <summary>
        ///     Sets the character callback of the specified window, which is called when a Unicode character is
        ///     input.
        ///     <para>
        ///         The character callback is intended for Unicode text input. As it deals with characters, it is keyboard layout
        ///         dependent, whereas the key callback is not. Characters do not map 1:1 to physical keys, as a key may produce
        ///         zero, one or more characters. If you want to know whether a specific physical key was pressed or released, see
        ///         the key callback instead.
        ///     </para>
        ///     <para>
        ///         The character callback behaves as system text input normally does and will not be called if modifier keys are
        ///         held down that would prevent normal text input on that platform, for example a Super (Command) key on OS X or
        ///         Alt key on Windows. There is a character with modifiers callback that receives these events.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="charCallback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetCharCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial CharCallback SetCharCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] CharCallback charCallback);

        /// <summary>
        ///     Sets the character with modifiers callback of the specified window, which is called when a Unicode
        ///     character is input regardless of what modifier keys are used.
        ///     <para>
        ///         The character with modifiers callback is intended for implementing custom Unicode character input. For
        ///         regular Unicode text input, see the character callback. Like the character callback, the character with
        ///         modifiers callback deals with characters and is keyboard layout dependent. Characters do not map 1:1 to
        ///         physical keys, as a key may produce zero, one or more characters. If you want to know whether a specific
        ///         physical key was pressed or released, see the key callback instead.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="charCallback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or an error occurred.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetCharModsCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial CharModsCallback SetCharModsCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] CharModsCallback charCallback);

        /// <summary>
        ///     Gets the last state reported for the specified key to the specified window.
        ///     <para>The higher-level action <see cref="InputState.Repeat" /> is only reported to the key callback.</para>
        ///     <para>
        ///         If the sticky keys input mode is enabled, this function returns <see cref="InputState.Press" /> the first
        ///         time you call it for a key that was pressed, even if that key has already been released.
        ///     </para>
        ///     <para>
        ///         The key functions deal with physical keys, with key tokens named after their use on the standard US keyboard
        ///         layout. If you want to input text, use the Unicode character callback instead.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="key">The key to query.</param>
        /// <returns>Either <see cref="InputState.Press" /> or <see cref="InputState.Release" />.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetKey")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial InputState GetKey(Window window, Keys key);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetKeyName")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetKeyNameInternal(Keys key, int scanCode);

        /// <summary>
        ///     Sets the framebuffer resize callback of the specified window, which is called when the framebuffer of
        ///     the specified window is resized.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="sizeCallback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetFramebufferSizeCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial SizeCallback SetFramebufferSizeCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] SizeCallback sizeCallback);

        /// <summary>
        ///     Sets the refresh callback of the specified window, which is called when the client area of the window
        ///     needs to be redrawn, for example if the window has been exposed after having been covered by another window.
        ///     <para>
        ///         On compositing window systems such as Aero, Compiz or Aqua, where the window contents are saved off-screen,
        ///         this callback may be called only very infrequently or never at all.
        ///     </para>
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="callback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowRefreshCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial WindowCallback SetWindowRefreshCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] WindowCallback callback);

        /// <summary>
        ///     Sets the key callback of the specified window, which is called when a key is pressed, repeated or
        ///     released.
        ///     <para>
        ///         The key functions deal with physical keys, with layout independent key tokens named after their values in the
        ///         standard US keyboard layout. If you want to input text, use the character callback instead.
        ///     </para>
        ///     <para>
        ///         When a window loses input focus, it will generate synthetic key release events for all pressed keys. You can
        ///         tell these events from user-generated events by the fact that the synthetic ones are generated after the focus
        ///         loss event has been processed, i.e. after the window focus callback has been called.
        ///     </para>
        ///     <para>
        ///         The scancode of a key is specific to that platform or sometimes even to that machine. Scancodes are intended
        ///         to allow users to bind keys that don't have a GLFW key token. Such keys have key set to
        ///         <see cref="Keys.Unknown" />, their state is not saved and so it cannot be queried with <see cref="GetKey" />.
        ///     </para>
        ///     <para>Sometimes GLFW needs to generate synthetic key events, in which case the scancode may be zero.</para>
        /// </summary>
        /// <param name="window">The new key callback, or <c>null</c> to remove the currently set callback.</param>
        /// <param name="keyCallback">The key callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetKeyCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial KeyCallback SetKeyCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] KeyCallback keyCallback);

        /// <summary>
        ///     Gets whether the specified joystick is present.
        /// </summary>
        /// <param name="joystick">The joystick to query.</param>
        /// <returns><c>true</c> if the joystick is present, or <c>false</c> otherwise.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwJoystickPresent")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool JoystickPresent(Joystick joystick);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetJoystickName")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetJoystickNameInternal(Joystick joystick);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetJoystickAxes")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetJoystickAxes(Joystick joystic, out int count);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetJoystickButtons")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetJoystickButtons(Joystick joystick, out int count);

        /// <summary>
        ///     Sets the joystick configuration callback, or removes the currently set callback.
        ///     <para>This is called when a joystick is connected to or disconnected from the system.</para>
        /// </summary>
        /// <param name="callback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetJoystickCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial JoystickCallback SetJoystickCallback([MarshalAs(UnmanagedType.FunctionPtr)] JoystickCallback callback);

        /// <summary>
        ///     Sets the monitor configuration callback, or removes the currently set callback. This is called when a
        ///     monitor is connected to or disconnected from the system.
        /// </summary>
        /// <param name="monitorCallback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetMonitorCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial MonitorCallback SetMonitorCallback([MarshalAs(UnmanagedType.FunctionPtr)] MonitorCallback monitorCallback);

        /// <summary>
        ///     Sets the iconification callback of the specified window, which is called when the window is iconified
        ///     or restored.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="callback">The new callback, or <c>null</c> to remove the currently set callback.</param>
        /// <returns>The previously set callback, or <c>null</c> if no callback was set or the library had not been initialized.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetWindowIconifyCallback")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        public static partial IconifyCallback SetWindowIconifyCallback(Window window, [MarshalAs(UnmanagedType.FunctionPtr)] IconifyCallback callback);

        /// <summary>
        ///     Sets an input mode option for the specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="mode">The mode to set a new value for.</param>
        /// <param name="value">The new value of the specified input mode.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwSetInputMode")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void SetInputMode(Window window, InputMode mode, int value);

        /// <summary>
        ///     Gets the value of an input option for the specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="mode">The mode to query.</param>
        /// <returns>Dependent on mode being queried.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetInputMode")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial int GetInputMode(Window window, InputMode mode);

        /// <summary>
        ///     Returns the position, in screen coordinates, of the upper-left corner of the work area of the specified
        ///     monitor along with the work area size in screen coordinates.
        ///     <para>
        ///         The work area is defined as the area of the monitor not occluded by the operating system task bar
        ///         where present. If no task bar exists then the work area is the monitor resolution in screen
        ///         coordinates.
        ///     </para>
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="width">The monitor width.</param>
        /// <param name="height">The monitor height.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetMonitorWorkarea")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void GetMonitorWorkArea(IntPtr monitor, out int x, out int y, out int width,
            out int height);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetProcAddress")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetProcAddress(byte[] procName);

        /// <summary>
        ///     Sets hints for the next call to <see cref="CreateWindow" />. The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or <see cref="DefaultWindowHints" />, or until the
        ///     library is
        ///     terminated.
        ///     <para>
        ///         This function does not check whether the specified hint values are valid. If you set hints to invalid values
        ///         this will instead be reported by the next call to <see cref="CreateWindow" />.
        ///     </para>
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="value">The value.</param>
        [LibraryImport(LIBRARY, EntryPoint = "glfwWindowHint")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        public static partial void WindowHint(Hint hint, int value);

        /// <summary>
        ///     Gets the value of the specified window attribute.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="attribute">The attribute to retrieve.</param>
        /// <returns>The value of the <paramref name="attribute" />.</returns>
        [LibraryImport(LIBRARY, EntryPoint = "glfwGetWindowAttrib")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial int GetWindowAttribute(Window window, int attribute);

        [LibraryImport(LIBRARY, EntryPoint = "glfwGetError")]
        [UnmanagedCallConv(CallConvs = new [] { typeof(CallConvCdecl) })]
        private static partial ErrorCode GetErrorPrivate(out IntPtr description);

        #endregion

        #region Methods

        /// <summary>
        ///     This function creates a window and its associated OpenGL or OpenGL ES context. Most of the options controlling how
        ///     the window and its context should be created are specified with window hints.
        /// </summary>
        /// <param name="width">The desired width, in screen coordinates, of the window. This must be greater than zero.</param>
        /// <param name="height">The desired height, in screen coordinates, of the window. This must be greater than zero.</param>
        /// <param name="title">The initial window title.</param>
        /// <param name="monitor">The monitor to use for full screen mode, or <see cref="Monitor.None" /> for windowed mode.</param>
        /// <param name="share">
        ///     A window instance whose context to share resources with, or <see cref="Window.None" /> to not share
        ///     resources..
        /// </param>
        /// <returns>The created window, or <see cref="Window.None" /> if an error occurred.</returns>
        public static Window CreateWindow(int width, int height, [NotNull] string title, Monitor monitor, Window share)
        {
            return CreateWindow(width, height, Encoding.UTF8.GetBytes(title), monitor, share);
        }

        /// <summary>
        ///     Gets the client API.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns>The client API.</returns>
        public static ClientApi GetClientApi(Window window)
        {
            return (ClientApi) GetWindowAttribute(window, (int) ContextAttributes.ClientApi);
        }

        /// <summary>
        ///     Gets the contents of the system clipboard, if it contains or is convertible to a UTF-8 encoded
        ///     string.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns>The contents of the clipboard as a UTF-8 encoded string, or <c>null</c> if an error occurred.</returns>
        [NotNull]
        public static string GetClipboardString(Window window)
        {
            return Util.PtrToStringUTF8(GetClipboardStringInternal(window));
        }

        /// <summary>
        ///     Gets the API used to create the context of the specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns>The API used to create the context.</returns>
        public static ContextApi GetContextCreationApi(Window window)
        {
            return (ContextApi) GetWindowAttribute(window, (int) ContextAttributes.ContextCreationApi);
        }

        /// <summary>
        ///     Gets the context version of the specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns>The context version.</returns>
        public static Version GetContextVersion(Window window)
        {
            GetContextVersion(window, out var major, out var minor, out var revision);
            return new Version(major, minor, revision);
        }

        /// <summary>
        ///     Gets whether the specified API extension is supported by the current OpenGL or OpenGL ES context.
        ///     <para>It searches both for client API extension and context creation API extensions.</para>
        /// </summary>
        /// <param name="extension">The extension name.</param>
        /// <returns><c>true</c> if the extension is supported; otherwise <c>false</c>.</returns>
        public static bool GetExtensionSupported(string extension)
        {
            return GetExtensionSupported(Encoding.ASCII.GetBytes(extension));
        }

        /// <summary>
        ///     Gets the current gamma ramp of the specified monitor.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>The current gamma ramp, or empty structure if an error occurred.</returns>
        public static GammaRamp GetGammaRamp(Monitor monitor)
        {
            return (GammaRamp) Marshal.PtrToStructure<GammaRampInternal>(GetGammaRampInternal(monitor));
        }

        /// <summary>
        ///     Gets value indicating if specified window is using a debug context.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns><c>true</c> if window context is debug context, otherwise <c>false</c>.</returns>
        public static bool GetIsDebugContext(Window window)
        {
            return GetWindowAttribute(window, (int) ContextAttributes.OpenglDebugContext) == (int) Constants.True;
        }

        /// <summary>
        ///     Gets value indicating if specified window is using a forward compatible context.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns><c>true</c> if window context is forward compatible, otherwise <c>false</c>.</returns>
        public static bool GetIsForwardCompatible(Window window)
        {
            return GetWindowAttribute(window, (int) ContextAttributes.OpenglForwardCompat) == (int) Constants.True;
        }

        /// <summary>
        ///     Gets the values of all axes of the specified joystick. Each element in the array is a value
        ///     between -1.0 and 1.0.
        ///     <para>
        ///         Querying a joystick slot with no device present is not an error, but will return an empty array. Call
        ///         <see cref="JoystickPresent" /> to check device presence.
        ///     </para>
        /// </summary>
        /// <param name="joystick">The joystick to query.</param>
        /// <returns>An array of axes values.</returns>
        public static float[] GetJoystickAxes(Joystick joystick)
        {
            var ptr = GetJoystickAxes(joystick, out var count);
            var axes = new float[count];
            if (count > 0)
                Marshal.Copy(ptr, axes, 0, count);
            return axes;
        }

        /// <summary>
        ///     Gets the state of all buttons of the specified joystick.
        /// </summary>
        /// <param name="joystick">The joystick to query.</param>
        /// <returns>An array of values, either <see cref="InputState.Press" /> and <see cref="InputState.Release" />.</returns>
        public static InputState[] GetJoystickButtons(Joystick joystick)
        {
            var ptr = GetJoystickButtons(joystick, out var count);
            var states = new InputState[count];
            for (var i = 0; i < count; i++)
                states[i] = (InputState) Marshal.ReadByte(ptr, i);
            return states;
        }

        /// <summary>
        ///     Gets the name of the specified joystick.
        ///     <para>
        ///         Querying a joystick slot with no device present is not an error. <see cref="JoystickPresent" /> to check
        ///         device presence.
        ///     </para>
        /// </summary>
        /// <param name="joystick">The joystick to query.</param>
        /// <returns>The name of the joystick, or <c>null</c> if the joystick is not present or an error occurred.</returns>
        public static string GetJoystickName(Joystick joystick)
        {
            return Util.PtrToStringUTF8(GetJoystickNameInternal(joystick));
        }

        /// <summary>
        ///     Gets the localized name of the specified printable key. This is intended for displaying key
        ///     bindings to the user.
        ///     <para>
        ///         If the key is <see cref="Keys.Unknown" />, the scancode is used instead, otherwise the scancode is ignored.
        ///         If a non-printable key or (if the key is <see cref="Keys.Unknown" />) a scancode that maps to a non-printable
        ///         key is specified, this function returns NULL.
        ///     </para>
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <param name="scanCode">The scancode of the key to query.</param>
        /// <returns>The localized name of the key.</returns>
        public static string GetKeyName(Keys key, int scanCode)
        {
            return Util.PtrToStringUTF8(GetKeyNameInternal(key, scanCode));
        }

        /// <summary>
        ///     Gets a human-readable name, encoded as UTF-8, of the specified monitor.
        ///     <para>
        ///         The name typically reflects the make and model of the monitor and is not guaranteed to be unique among the
        ///         connected monitors.
        ///     </para>
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>The name of the monitor, or <c>null</c> if an error occurred.</returns>
        public static string GetMonitorName(Monitor monitor)
        {
            return Util.PtrToStringUTF8(GetMonitorNameInternal(monitor));
        }

        /// <summary>
        ///     Gets the address of the specified OpenGL or OpenGL ES core or extension function, if it is
        ///     supported by the current context.
        ///     This function does not apply to Vulkan. If you are rendering with Vulkan, use
        ///     <see cref="Vulkan.GetInstanceProcAddress" /> instead.
        /// </summary>
        /// <param name="procName">Name of the function.</param>
        /// <returns>The address of the function, or <see cref="IntPtr.Zero" /> if an error occurred.</returns>
        public static IntPtr GetProcAddress(string procName)
        {
            return GetProcAddress(Encoding.ASCII.GetBytes(procName));
        }

        /// <summary>
        ///     Gets the profile of the specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns>Profile of the window.</returns>
        public static Profile GetProfile(Window window)
        {
            return (Profile) GetWindowAttribute(window, (int) ContextAttributes.OpenglProfile);
        }

        /// <summary>
        ///     Gets the robustness value of the specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <returns>Current set value of the robustness.</returns>
        public static Robustness GetRobustness(Window window)
        {
            return (Robustness) GetWindowAttribute(window, (int) ContextAttributes.ContextRobustness);
        }

        /// <summary>
        ///     Gets the current video mode of the specified monitor.
        ///     <para>
        ///         If you have created a full screen window for that monitor, the return value will depend on whether that
        ///         window is iconified.
        ///     </para>
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>The current mode of the monitor, or <c>null</c> if an error occurred.</returns>
        public static VideoMode GetVideoMode(Monitor monitor)
        {
            var ptr = GetVideoModeInternal(monitor);
            return Marshal.PtrToStructure<VideoMode>(ptr);
        }

        /// <summary>
        ///     Gets an array of all video modes supported by the specified monitor.
        ///     <para>
        ///         The returned array is sorted in ascending order, first by color bit depth (the sum of all channel depths) and
        ///         then by resolution area (the product of width and height).
        ///     </para>
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>The array of video modes.</returns>
        public static VideoMode[] GetVideoModes(Monitor monitor)
        {
            var pointer = GetVideoModes(monitor, out var count);
            var modes = new VideoMode[count];
            for (var i = 0; i < count; i++, pointer += Marshal.SizeOf<VideoMode>())
                modes[i] = Marshal.PtrToStructure<VideoMode>(pointer);
            return modes;
        }

        /// <summary>
        ///     Gets the value of an attribute of the specified window or its OpenGL or OpenGL ES context.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="attribute">The window attribute whose value to return.</param>
        /// <returns>The value of the attribute, or zero if an error occurred.</returns>
        public static bool GetWindowAttribute(Window window, WindowAttribute attribute)
        {
            return GetWindowAttribute(window, (int) attribute) == (int) Constants.True;
        }

        /// <summary>
        ///     Sets the system clipboard to the specified string.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="str">The string to set to the clipboard.</param>
        public static void SetClipboardString(Window window, string str)
        {
            SetClipboardString(window, Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        ///     Sets the window title, encoded as UTF-8, of the specified window.
        /// </summary>
        /// <param name="window">A window instance.</param>
        /// <param name="title">The title to set.</param>
        public static void SetWindowTitle(Window window, string title)
        {
            SetWindowTitle(window, Encoding.UTF8.GetBytes(title));
        }

        /// <summary>
        ///     Sets hints for the next call to <see cref="CreateWindow" />. The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or <see cref="DefaultWindowHints" />, or until the
        ///     library is
        ///     terminated.
        ///     <para>
        ///         This function does not check whether the specified hint values are valid. If you set hints to invalid values
        ///         this will instead be reported by the next call to <see cref="CreateWindow" />.
        ///     </para>
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="value">The value.</param>
        public static void WindowHint(Hint hint, [MarshalAs(UnmanagedType.Bool)] bool value)
        {
            WindowHint(hint, value ? Constants.True : Constants.False);
        }

        /// <summary>
        ///     Sets hints for the next call to <see cref="CreateWindow" />. The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or <see cref="DefaultWindowHints" />, or until the
        ///     library is
        ///     terminated.
        ///     <para>
        ///         This function does not check whether the specified hint values are valid. If you set hints to invalid values
        ///         this will instead be reported by the next call to <see cref="CreateWindow" />.
        ///     </para>
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="value">The value.</param>
        public static void WindowHint(Hint hint, ClientApi value) { WindowHint(hint, (int) value); }

        /// <summary>
        ///     Sets hints for the next call to <see cref="CreateWindow" />. The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or <see cref="DefaultWindowHints" />, or until the
        ///     library is
        ///     terminated.
        ///     <para>
        ///         This function does not check whether the specified hint values are valid. If you set hints to invalid values
        ///         this will instead be reported by the next call to <see cref="CreateWindow" />.
        ///     </para>
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="value">The value.</param>
        public static void WindowHint(Hint hint, Constants value) { WindowHint(hint, (int) value); }

        /// <summary>
        ///     Sets hints for the next call to <see cref="CreateWindow" />. The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or <see cref="DefaultWindowHints" />, or until the
        ///     library is
        ///     terminated.
        ///     <para>
        ///         This function does not check whether the specified hint values are valid. If you set hints to invalid values
        ///         this will instead be reported by the next call to <see cref="CreateWindow" />.
        ///     </para>
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="value">The value.</param>
        public static void WindowHint(Hint hint, ContextApi value) { WindowHint(hint, (int) value); }

        /// <summary>
        ///     Sets hints for the next call to <see cref="CreateWindow" />. The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or <see cref="DefaultWindowHints" />, or until the
        ///     library is
        ///     terminated.
        ///     <para>
        ///         This function does not check whether the specified hint values are valid. If you set hints to invalid values
        ///         this will instead be reported by the next call to <see cref="CreateWindow" />.
        ///     </para>
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="value">The value.</param>
        public static void WindowHint(Hint hint, Robustness value) { WindowHint(hint, (int) value); }

        /// <summary>
        ///     Sets hints for the next call to <see cref="CreateWindow" />. The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or <see cref="DefaultWindowHints" />, or until the
        ///     library is
        ///     terminated.
        ///     <para>
        ///         This function does not check whether the specified hint values are valid. If you set hints to invalid values
        ///         this will instead be reported by the next call to <see cref="CreateWindow" />.
        ///     </para>
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="value">The value.</param>
        public static void WindowHint(Hint hint, Profile value) { WindowHint(hint, (int) value); }

        /// <summary>
        ///     Sets hints for the next call to <see cref="CreateWindow" />. The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or <see cref="DefaultWindowHints" />, or until the
        ///     library is
        ///     terminated.
        ///     <para>
        ///         This function does not check whether the specified hint values are valid. If you set hints to invalid values
        ///         this will instead be reported by the next call to <see cref="CreateWindow" />.
        ///     </para>
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="value">The value.</param>
        public static void WindowHint(Hint hint, ReleaseBehavior value) { WindowHint(hint, (int) value); }

        private static void GetContextVersion(Window window, out int major, out int minor, out int revision)
        {
            major = GetWindowAttribute(window, (int) ContextAttributes.ContextVersionMajor);
            minor = GetWindowAttribute(window, (int) ContextAttributes.ContextVersionMinor);
            revision = GetWindowAttribute(window, (int) ContextAttributes.ContextVersionRevision);
        }

        private static void GlfwError(ErrorCode code, IntPtr message)
        {
            throw new Exception(Util.PtrToStringUTF8(message));
        }

        #endregion
    }
}