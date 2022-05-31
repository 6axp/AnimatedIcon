using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using PInvoke;

namespace AnimatedIcon
{
	public static class CastingHelper
	{
		public static T CastToStruct<T>(byte[] data) where T : struct
		{
			var pData = GCHandle.Alloc(data, GCHandleType.Pinned);
			var result = (T)Marshal.PtrToStructure(pData.AddrOfPinnedObject(), typeof(T));
			pData.Free();
			return result;
		}

		public static byte[] CastToArray<T>(T data) where T : struct
		{
			var result = new byte[Marshal.SizeOf(typeof(T))];
			var pResult = GCHandle.Alloc(result, GCHandleType.Pinned);
			Marshal.StructureToPtr(data, pResult.AddrOfPinnedObject(), true);
			pResult.Free();
			return result;
		}
	}

	#region dll import

	class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr IpBaseadress, byte[] IpBuffer, int dwSize, out int IpNumberOfBytesWritten);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		public static IntPtr GetProcessHandle(IntPtr hWindow)
		{
			int ProcessId;
			User32.GetWindowThreadProcessId(hWindow, out ProcessId);
			if (ProcessId == 0)
				return IntPtr.Zero;

			var access = (uint)Kernel32.ACCESS_MASK.GenericRight.GENERIC_ALL;
			return OpenProcess(access, false, ProcessId);
		}

		public static IntPtr AllocateMemory(IntPtr hProcess, int size)
        {
			uint MEM_COMMIT = 0x00001000;
			uint PAGE_READWRITE = 0x04;

			return VirtualAllocEx(hProcess, IntPtr.Zero, size, MEM_COMMIT, PAGE_READWRITE);
		}

		public static bool SendMessageExternal<T>(IntPtr hWindow, uint wMsg, IntPtr wParam, ref T lParam) where T : struct
		{
			var tSize = Marshal.SizeOf(lParam);

			int ProcessId;
			User32.GetWindowThreadProcessId(hWindow, out ProcessId);
			if (ProcessId == 0)
				return false;

			var access = (uint)Kernel32.ACCESS_MASK.GenericRight.GENERIC_ALL;
			var process = OpenProcess(access, false, ProcessId);
			if (process == IntPtr.Zero)
				return false;

			uint MEM_COMMIT = 0x00001000;
			uint PAGE_READWRITE = 0x04;

			var pMemory = VirtualAllocEx(process, IntPtr.Zero, tSize, MEM_COMMIT, PAGE_READWRITE);
			if (pMemory == IntPtr.Zero)
				return false;

			var buffer = CastingHelper.CastToArray<T>(lParam);

			int written;
			var success = WriteProcessMemory(process, pMemory, buffer, tSize, out written);
			if (!success)
				return false;

			var res = SendMessage(hWindow, wMsg, wParam, pMemory);

			int read;
			success = ReadProcessMemory(process, pMemory, buffer, tSize, out read);
			if (!success)
				return false;

			lParam = CastingHelper.CastToStruct<T>(buffer);

			return true;
		}

		public static void Constants()
        {
			uint LVM_FIRST = 4096;
			uint LVM_GETITEMCOUNT = LVM_FIRST + 4;
			uint LVM_GETITEMRECT = 4110;
			uint LVM_GETITEMW = 4171;
			uint LVM_GETITEMA = 4101;
			uint LVM_GETITEMTEXTW = 4211;
			uint LVM_GETTILEINFO = 4261;
			uint LVM_GETHEADER = 4127;
			uint LVM_GETCOUNTPERPAGE = 4136;
			uint LVM_GETCOLUMNW = 4191;
			uint LVM_GETGROUPINFO = 4245; // user get info by index instead!!!!

			uint LVM_INSERTITEM = LVM_FIRST + 77;

			uint LVM_GETGROUPCOUNT = LVM_FIRST + 152;

			uint HDM_GETITEMCOUNT = 4608;
			uint HDM_GETITEM = 0x1200;

			uint LVM_GETSUBITEMRECT = 4152;

			uint LVIF_TEXT = 0x00000001;
			uint LVIF_IMAGE = 0x00000002;
			uint LVIF_PARAM = 0x00000004;
			uint LVIF_STATE = 0x00000008;
			uint LVIF_INDENT = 0x00000010;
			uint LVIF_NORECOMPUTE = 0x00000800;
			uint LVIF_GROUPID = 0x00000100;
			uint LVIF_COLUMNS = 0x00000200;

			uint LVGF_ITEMS = 0x4000;
			uint LVGF_STATE = 0x00000004;

			uint HDI_TEXT = 0x0002;
			uint HDI_WIDTH = 0x0001;
			/*
#define HDI_HEIGHT              HDI_WIDTH
#define HDI_TEXT                0x0002
#define HDI_FORMAT              0x0004
#define HDI_LPARAM              0x0008
#define HDI_BITMAP              0x0010
#define HDI_IMAGE               0x0020
#define HDI_DI_SETITEM          0x0040
#define HDI_ORDER               0x0080
#define HDI_FILTER              0x0100
            */
		}
	}

    #endregion
}
