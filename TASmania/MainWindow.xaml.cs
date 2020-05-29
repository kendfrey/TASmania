using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TASmania
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Hotkey_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Hotkey hotkey = (Hotkey)((TextBlock)sender).DataContext;
			hotkey.Key = -1;
		}

		private void Hotkey_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			Hotkey hotkey = (Hotkey)((TextBlock)sender).DataContext;
			hotkey.Key = 0;
		}
	}
}
