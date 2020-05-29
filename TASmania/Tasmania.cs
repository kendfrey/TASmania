using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WF = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;

namespace TASmania
{
	class Tasmania : INotifyPropertyChanged
	{
		public Dictionary<string, Hotkey> Hotkeys { get; } = new Dictionary<string, Hotkey>();

		private bool isRecording;
		public bool IsRecording
		{
			get => isRecording;
			set
			{
				isRecording = value;
				OnPropertyChanged(nameof(IsRecording));
				OnPropertyChanged(nameof(IsBusy));
			}
		}

		private readonly IKeyboardMouseEvents hook;

		public Playback Playback { get; } = new Playback();

		public bool IsBusy => IsRecording || Playback.IsRunning;

		public RecordCommand RecordCmd { get; }
		public PlaybackCommand PlaybackCmd { get; }
		public StopCommand StopCmd { get; }

		private string macro;
		public string Macro
		{
			get => macro;
			set
			{
				macro = value;
				OnPropertyChanged(nameof(Macro));
			}
		}

		public Tasmania()
		{
			Hotkeys.Add("stop", new Hotkey("Stop recording/playback", "Pressing this key will stop recording or playback."));
			Hotkeys.Add("phase", new Hotkey("Next phase", "Pressing this key during recording will insert a phase marker. Pressing this key during playback will resume the playback if it is stopped at a phase marker."));

			hook = Hook.GlobalEvents();
			hook.MouseDown += Hook_MouseDown;
			hook.MouseUp += Hook_MouseUp;
			hook.MouseWheel += Hook_MouseWheel;
			hook.KeyDown += Hook_KeyDown;
			hook.KeyUp += Hook_KeyUp;

			Playback.PropertyChanged += (s, e) => OnPropertyChanged(nameof(IsBusy));

			RecordCmd = new RecordCommand(this);
			PlaybackCmd = new PlaybackCommand(this);
			StopCmd = new StopCommand(this);

			LoadSettings();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		private void Hook_MouseDown(object sender, WF.MouseEventArgs e)
		{
			if (IsRecording)
			{
				Macro += $"mousedown {MouseButton(e.Button)} {e.X} {e.Y}\r\n";
			}
		}

		private void Hook_MouseUp(object sender, WF.MouseEventArgs e)
		{
			if (IsRecording)
			{
				Macro += $"mouseup {MouseButton(e.Button)} {e.X} {e.Y}\r\n";
			}
		}

		private void Hook_MouseWheel(object sender, WF.MouseEventArgs e)
		{
			if (IsRecording)
			{
				Macro += $"mousemove {e.X} {e.Y}\r\n";
				Macro += $"mousewheel {e.Delta / 120}\r\n";
			}
		}

		private void Hook_KeyDown(object sender, WF.KeyEventArgs e)
		{
			int scancode = ((KeyEventArgsExt)e).ScanCode;
			bool shouldSave = false;
			foreach (Hotkey listeningHotkey in Hotkeys.Values.Where(h => h.Key < 0))
			{
				listeningHotkey.Key = scancode;
				shouldSave = true;
			}

			if (shouldSave)
			{
				SaveSettings();
			}
			else if (scancode == Hotkeys["stop"].Key)
			{
				Stop();
			}
			else if (scancode == Hotkeys["phase"].Key)
			{
				if (IsRecording)
				{
					Macro += "phase\r\n";
				}
				if (Playback.IsRunning)
				{
					Playback.SignalPhase();
				}
			}
			else if (IsRecording)
			{
				Macro += $"keydown {scancode}\r\n";
			}
		}

		private void Hook_KeyUp(object sender, WF.KeyEventArgs e)
		{
			int scancode = ((KeyEventArgsExt)e).ScanCode;
			if (scancode == Hotkeys["stop"].Key || scancode == Hotkeys["phase"].Key)
			{
				return;
			}
			if (IsRecording)
			{
				Macro += $"keyup {scancode}\r\n";
			}
		}

		int MouseButton(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					return 0;
				case MouseButtons.Right:
					return 1;
				case MouseButtons.Middle:
					return 2;
				default:
					return 0;
			}
		}

		void StartRecording()
		{
			IsRecording = true;
		}

		void StartPlayback()
		{
			Playback.Run(Macro);
		}

		void Stop()
		{
			IsRecording = false;
			Playback.Kill();
		}

		void LoadSettings()
		{
			if (File.Exists("settings.json"))
			{
				var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
				foreach (var hotkey in settings.Hotkeys.Keys)
				{
					if (Hotkeys.ContainsKey(hotkey))
					{
						Hotkeys[hotkey].Key = settings.Hotkeys[hotkey];
					}
				}
			}
		}

		void SaveSettings()
		{
			var settings = new Settings();
			foreach (var hotkey in Hotkeys.Keys)
			{
				settings.Hotkeys[hotkey] = Hotkeys[hotkey].Key;
			}
			File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
		}

		public class Settings
		{
			[JsonProperty("hotkeys")]
			public Dictionary<string, int> Hotkeys = new Dictionary<string, int>();
		}

		public class RecordCommand : ICommand
		{
			readonly Tasmania tasmania;

			public RecordCommand(Tasmania tasmania)
			{
				this.tasmania = tasmania;
				tasmania.PropertyChanged += Tasmania_PropertyChanged;
			}

			private void Tasmania_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}

			public event EventHandler CanExecuteChanged;

			public bool CanExecute(object parameter)
			{
				return !tasmania.IsBusy;
			}

			public void Execute(object parameter)
			{
				tasmania.StartRecording();
			}
		}

		public class PlaybackCommand : ICommand
		{
			readonly Tasmania tasmania;

			public PlaybackCommand(Tasmania tasmania)
			{
				this.tasmania = tasmania;
				tasmania.PropertyChanged += Tasmania_PropertyChanged;
			}

			private void Tasmania_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}

			public event EventHandler CanExecuteChanged;

			public bool CanExecute(object parameter)
			{
				return !tasmania.IsBusy;
			}

			public void Execute(object parameter)
			{
				tasmania.StartPlayback();
			}
		}

		public class StopCommand : ICommand
		{
			readonly Tasmania tasmania;

			public StopCommand(Tasmania tasmania)
			{
				this.tasmania = tasmania;
				tasmania.PropertyChanged += Tasmania_PropertyChanged;
			}

			private void Tasmania_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}

			public event EventHandler CanExecuteChanged;

			public bool CanExecute(object parameter)
			{
				return tasmania.IsBusy;
			}

			public void Execute(object parameter)
			{
				tasmania.Stop();
			}
		}
	}
}
