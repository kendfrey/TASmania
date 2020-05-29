using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Forms;

namespace TASmania
{
	class KeyConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int key)
			{
				if (key == 0)
					return "<none>";
				else if (key < 0)
					return "Listening...";
				else
					return ((Keys)MapVirtualKey((uint)key, 3)).ToString();
			}
			else
				throw new NotImplementedException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		[DllImport("user32.dll")]
		static extern uint MapVirtualKey(uint uCode, uint uMapType);
	}
}
