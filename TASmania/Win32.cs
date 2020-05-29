using System;
using System.Runtime.InteropServices;

namespace TASmania
{
	class Win32
	{
		const int ScreenWidth = 1919;
		const int ScreenHeight = 1079;
		const int Scale = 65535;

		public static INPUT MouseMove(int x, int y)
		{
			return MouseInput(x, y);
		}

		public static INPUT MouseDown(int button, int x, int y)
		{
			var input = MouseInput(x, y);
			switch (button)
			{
				case 0:
					input.U.mi.dwFlags |= MOUSEEVENTF.LEFTDOWN;
					break;
				case 1:
					input.U.mi.dwFlags |= MOUSEEVENTF.RIGHTDOWN;
					break;
				case 2:
					input.U.mi.dwFlags |= MOUSEEVENTF.MIDDLEDOWN;
					break;
			}
			return input;
		}

		public static INPUT MouseUp(int button, int x, int y)
		{
			var input = MouseInput(x, y);
			switch (button)
			{
				case 0:
					input.U.mi.dwFlags |= MOUSEEVENTF.LEFTUP;
					break;
				case 1:
					input.U.mi.dwFlags |= MOUSEEVENTF.RIGHTUP;
					break;
				case 2:
					input.U.mi.dwFlags |= MOUSEEVENTF.MIDDLEUP;
					break;
			}
			return input;
		}

		public static INPUT MouseWheel(int delta)
		{
			var input = MouseInput(0, 0);
			input.U.mi.dwFlags = MOUSEEVENTF.WHEEL;
			input.U.mi.mouseData = delta * 120;
			return input;
		}

		public static INPUT KeyDown(short key)
		{
			return KeyInput(key);
		}

		public static INPUT KeyUp(short key)
		{
			var input = KeyInput(key);
			input.U.ki.dwFlags |= KEYEVENTF.KEYUP;
			return input;
		}

		static INPUT MouseInput(int x, int y)
		{
			return new INPUT
			{
				type = InputType.INPUT_MOUSE,
				U = new InputUnion
				{
					mi = new MOUSEINPUT
					{
						dx = x * Scale / ScreenWidth,
						dy = y * Scale / ScreenHeight,
						dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.VIRTUALDESK | MOUSEEVENTF.MOVE,
					}
				}
			};
		}

		static INPUT KeyInput(short key)
		{
			return new INPUT
			{
				type = InputType.INPUT_KEYBOARD,
				U = new InputUnion
				{
					ki = new KEYBDINPUT
					{
						wScan = key,
						dwFlags = KEYEVENTF.SCANCODE,
					}
				}
			};
		}

		public static uint SendInputs(INPUT[] inputs) => SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

		[DllImport("user32.dll")]
		static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
	}

	enum InputType : uint
	{
		INPUT_MOUSE = 0,
		INPUT_KEYBOARD = 1,
		/*INPUT_HARDWARE = 2,*/
	}

	[StructLayout(LayoutKind.Sequential)]
	struct INPUT
	{
		public InputType type;
		public InputUnion U;
	}

	[StructLayout(LayoutKind.Explicit)]
	struct InputUnion
	{
		[FieldOffset(0)]
		public MOUSEINPUT mi;
		[FieldOffset(0)]
		public KEYBDINPUT ki;
		/*[FieldOffset(0)]
		public HARDWAREINPUT hi;*/
	}

	[StructLayout(LayoutKind.Sequential)]
	struct MOUSEINPUT
	{
		public int dx;
		public int dy;
		public int mouseData;
		public MOUSEEVENTF dwFlags;
		public uint time;
		public UIntPtr dwExtraInfo;
	}

	[Flags]
	enum MOUSEEVENTF : uint
	{
		ABSOLUTE = 0x8000,
		HWHEEL = 0x01000,
		MOVE = 0x0001,
		MOVE_NOCOALESCE = 0x2000,
		LEFTDOWN = 0x0002,
		LEFTUP = 0x0004,
		RIGHTDOWN = 0x0008,
		RIGHTUP = 0x0010,
		MIDDLEDOWN = 0x0020,
		MIDDLEUP = 0x0040,
		VIRTUALDESK = 0x4000,
		WHEEL = 0x0800,
		XDOWN = 0x0080,
		XUP = 0x0100
	}

	[StructLayout(LayoutKind.Sequential)]
	struct KEYBDINPUT
	{
		public short wVk;
		public short wScan;
		public KEYEVENTF dwFlags;
		public int time;
		public UIntPtr dwExtraInfo;
	}

	[Flags]
	enum KEYEVENTF : uint
	{
		EXTENDEDKEY = 0x0001,
		KEYUP = 0x0002,
		SCANCODE = 0x0008,
		UNICODE = 0x0004
	}
}
