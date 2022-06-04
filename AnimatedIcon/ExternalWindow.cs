using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using PInvoke;

namespace AnimatedIcon
{
    class ExternalWindow
    {
        public IntPtr WindowHandle;
        public IntPtr ProcessHandle;

        public static ExternalWindow FromHandle(IntPtr hWindow)
        {
            var process = new ExternalWindow();

            int ProcessId;
            User32.GetWindowThreadProcessId(hWindow, out ProcessId);
            if (ProcessId == 0)
                return process;

            var access = (uint)Kernel32.ACCESS_MASK.GenericRight.GENERIC_ALL;
            process.ProcessHandle = NativeMethods.OpenProcess(access, false, ProcessId);
            process.WindowHandle = hWindow;

            return process;
        }

        public IntPtr AllocateMemory(int size)
        {
            uint MEM_COMMIT = 0x00001000;
            uint PAGE_READWRITE = 0x04;

            return NativeMethods.VirtualAllocEx(this.ProcessHandle, IntPtr.Zero, size, MEM_COMMIT, PAGE_READWRITE);
        }

        public void FreeMemory(IntPtr address)
        {
            uint MEM_RELEASE = 0x8000;
            NativeMethods.VirtualFreeEx(this.ProcessHandle, address, 0, MEM_RELEASE);
        }

        public bool ReadMemory(IntPtr address, byte[] buffer)
        {
            int read;
            return NativeMethods.ReadProcessMemory(this.ProcessHandle, address, buffer, buffer.Length, out read);
        }

        public byte[] ReadMemory(IntPtr address, int size)
        {
            var buffer = new byte[size];
            int read;
            NativeMethods.ReadProcessMemory(this.ProcessHandle, address, buffer, buffer.Length, out read);
            Array.Resize(ref buffer, read);
            return buffer;
        }

        public T ReadMemory<T>(IntPtr address) where T : struct
        {
            var buffer = new byte[Marshal.SizeOf(new T())];
            int read;
            NativeMethods.ReadProcessMemory(this.ProcessHandle, address, buffer, buffer.Length, out read);
            return CastingHelper.CastToStruct<T>(buffer);
        }

        public bool WriteToMemory(IntPtr address, byte[] data)
        {
            int written;
            return NativeMethods.WriteProcessMemory(this.ProcessHandle, address, data, data.Length, out written);
        }

        public bool WriteToMemory<T>(IntPtr address, T data) where T : struct
        {
            return WriteToMemory(address, CastingHelper.CastToBytes(data));
        }

        public IntPtr WriteToMemory<T>(T data) where T : struct
        {
            IntPtr address = AllocateMemory(Marshal.SizeOf(data));
            WriteToMemory(address, CastingHelper.CastToBytes(data));
            return address; 
        }

        public IntPtr SendMessage(uint wMsg, IntPtr wParam, IntPtr lParam)
        {
            return NativeMethods.SendMessage(this.WindowHandle, wMsg, wParam, lParam);
        }

        public IntPtr SendMessage(uint wMsg)
        {
            return this.SendMessage(wMsg, IntPtr.Zero, IntPtr.Zero);
        }

        public bool SendMessage<T>(uint wMsg, IntPtr wParam, ref T lParam) where T : struct
		{
            if (this.ProcessHandle == IntPtr.Zero)
                return false;

            var tSize = Marshal.SizeOf(lParam);

			var pMemory = this.AllocateMemory(tSize);
			if (pMemory == IntPtr.Zero)
				return false;


            this.WriteToMemory(pMemory, lParam);

			var buffer = CastingHelper.CastToBytes<T>(lParam);

			int written;
			var success = NativeMethods.WriteProcessMemory(this.ProcessHandle, pMemory, buffer, tSize, out written);
			if (!success)
				return false;

			NativeMethods.SendMessage(this.WindowHandle, wMsg, wParam, pMemory);

			int read;
			success = NativeMethods.ReadProcessMemory(this.ProcessHandle, pMemory, buffer, tSize, out read);
			if (!success)
				return false;

			lParam = CastingHelper.CastToStruct<T>(buffer);
			return true;
		}
	}
}
