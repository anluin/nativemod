#nullable enable

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Vintagestory.API.Common;


[assembly: ModDependency("game", "1.17.10")]
[assembly: ModInfo(
    "NativeMod",
    "nativemod",
    Website = "https://github.com/anluin/nativemod",
    Version = "0.0.1",
    Description = "Proof of concept: Writing mods in rust / as native libraries",
    Authors = new[] { "Anluin" }
)]

namespace NativeMod {
    public class NativeMod : ModSystem {
        private const int RTLD_NOW = 2;

        public delegate void SetCallbackDelegate(IntPtr callback);

        public delegate void SimpleDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StringDelegate(string message);

        private readonly SimpleDelegate start;

        private readonly IntPtr handle;

        static ICoreAPI? CoreAPI;

        [DllImport("libdl.so.2")]
        protected static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl.so.2")]
        protected static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so.2")]
        protected static extern IntPtr dlsym(IntPtr handle, string symbol);

        public static class Interface {
            public static class Logger {
                public static void Debug(string message) {
                    CoreAPI?.Logger.Debug(message);
                }
            }
        }

        public NativeMod() {
            var assembly = Assembly.GetExecutingAssembly();
            var path = Path.GetTempFileName();

            using var stream = assembly.GetManifestResourceStream("libNativeMod.so");

            if (stream != null) {
                using var fileStream = File.OpenWrite(path);

                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            this.handle = dlopen(path, RTLD_NOW);

            File.Delete(path);

            if (handle == IntPtr.Zero) {
                throw new ArgumentException($"Unable to load unmanaged module \"{path}\"");
            }

            this.start = this.getDelegate<SimpleDelegate>("start") ?? throw new InvalidOperationException();

            (this.getDelegate<SetCallbackDelegate>("set_logger_debug_fn") ?? throw new InvalidOperationException())
                (Marshal.GetFunctionPointerForDelegate(new StringDelegate(Interface.Logger.Debug)));
        }

        public override void Start(ICoreAPI api) {
            base.Start(CoreAPI = api);

            this.start();
        }

        private TDelegate? getDelegate<TDelegate>(string methodName) where TDelegate : class {
            var ptr = dlsym(this.handle, methodName);

            if (ptr == IntPtr.Zero) {
                throw new MissingMethodException($"The unmanaged method \"{methodName}\" does not exist");
            }

            return Marshal.GetDelegateForFunctionPointer(ptr, typeof(TDelegate)) as TDelegate;
        }

        public override void Dispose() {
            base.Dispose();
            dlclose(this.handle);
        }
    }
}
