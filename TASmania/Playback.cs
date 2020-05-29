using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace TASmania
{
	class Playback : INotifyPropertyChanged
	{
		Thread thread;
		Dispatcher dispatcher;
		int delay = 1;
		AutoResetEvent phaseEvent;

		private bool isRunning;
		public bool IsRunning
		{
			get => isRunning;
			set
			{
				isRunning = value;
				OnPropertyChanged(nameof(IsRunning));
			}
		}

		public Playback()
		{
			dispatcher = Dispatcher.CurrentDispatcher;
		}

		public void Run(string macro)
		{
			if (IsRunning)
				return;

			thread = new Thread(() => RunThread(macro));
			thread.Start();
		}

		public void Kill()
		{
			thread?.Abort();
		}

		void RunThread(string macro)
		{
			try
			{
				IsRunning = true;

				foreach (Action action in macro.ToLower().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => ParseInstruction(s)).ToList())
				{
					action();
					Thread.Sleep(delay);
				}
			}
			finally
			{
				IsRunning = false;
			}
		}

		public void SignalPhase()
		{
			phaseEvent?.Set();
		}

		void WaitPhase()
		{
			using (phaseEvent = new AutoResetEvent(false))
			{
				phaseEvent.WaitOne();
			}
			phaseEvent = null;
		}

		Action ParseInstruction(string instruction)
		{
			int delay;
			int delta;
			int button;
			int x;
			int y;
			short key;
			if (instruction.Contains('#'))
				instruction = "";
			if (string.IsNullOrWhiteSpace(instruction))
				return () => { };
			string[] args = instruction.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				switch (args[0])
				{
					case "mousemove":
						x = int.Parse(args[1]);
						y = int.Parse(args[2]);
						return () => Win32.SendInputs(new[] { Win32.MouseMove(x, y) });
					case "mousedown":
						button = int.Parse(args[1]);
						x = int.Parse(args[2]);
						y = int.Parse(args[3]);
						return () => Win32.SendInputs(new[] { Win32.MouseDown(button, x, y) });
					case "mouseup":
						button = int.Parse(args[1]);
						x = int.Parse(args[2]);
						y = int.Parse(args[3]);
						return () => Win32.SendInputs(new[] { Win32.MouseUp(button, x, y) });
					case "mousewheel":
						delta = int.Parse(args[1]);
						return () => Win32.SendInputs(new[] { Win32.MouseWheel(delta) });
					case "keydown":
						key = short.Parse(args[1]);
						return () => Win32.SendInputs(new[] { Win32.KeyDown(key) });
					case "keyup":
						key = short.Parse(args[1]);
						return () => Win32.SendInputs(new[] { Win32.KeyUp(key) });
					case "pause":
						delay = int.Parse(args[1]);
						return () => Thread.Sleep(delay);
					case "setdelay":
						delay = int.Parse(args[1]);
						return () => { this.delay = delay; };
					case "phase":
						return () => WaitPhase();
					default:
						return () => { };
				}
			}
			catch (IndexOutOfRangeException ex)
			{
				throw new Exception($"Too few arguments: {instruction}", ex);
			}
			catch (Exception ex)
			{
				throw new Exception($"Invalid command: {instruction}", ex);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name) => dispatcher.BeginInvoke(new Action(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name))));
	}
}
