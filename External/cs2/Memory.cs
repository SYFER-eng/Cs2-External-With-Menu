using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace cs2
{
    internal static class Memory
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_VM_READ = 0x0010;

        public static bool Initialize(out string mes)
        {
            mes = "The program is already running";
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                return false;

            mes = "CS2 process not found";
            var procList = Process.GetProcessesByName("cs2");
            if (procList.Length == 0)
                return false;

            _proc = procList[0];
            foreach (ProcessModule module in _proc.Modules)
            {
                try
                {
                    if (module.ModuleName == "client.dll")
                    {
                        ClientPtr = module.BaseAddress;
                        return true;
                    }
                }
                catch { }
            }
            throw new DllNotFoundException("client.dll");
        }

        public static T Read<T>(IntPtr address) where T : unmanaged
        {
            var size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];
            int bytesRead;
            if (ReadProcessMemory(_proc.Handle, address, buffer, size, out bytesRead) && bytesRead == size)
            {
                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
            }
            return default;
        }

        public static byte[] ReadBytes(IntPtr lpBaseAddress, int maxLength)
        {
            var buffer = new byte[maxLength];
            int bytesRead;
            if (ReadProcessMemory(_proc.Handle, lpBaseAddress, buffer, maxLength, out bytesRead))
                return buffer;
            return new byte[0]; // return empty if read fails
        }

        public static string ReadString(IntPtr lpBaseAddress, int maxLength = 256)
        {
            var buffer = ReadBytes(lpBaseAddress, maxLength);
            var nullCharIndex = Array.IndexOf(buffer, (byte)'\0');
            return nullCharIndex >= 0
                ? Encoding.UTF8.GetString(buffer, 0, nullCharIndex).Trim()
                : Encoding.UTF8.GetString(buffer).Trim();
        }

        public static IntPtr ClientPtr { get; set; }

        private static Process _proc = null!;
    }
}
