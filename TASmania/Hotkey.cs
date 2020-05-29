using System.ComponentModel;

namespace TASmania
{
	class Hotkey : INotifyPropertyChanged
	{
		public string Label { get; }

		public string Description { get; }

		int key;
		public int Key
		{
			get => key;
			set
			{
				key = value;
				OnPropertyChanged(nameof(Key));
			}
		}

		public Hotkey(string label, string description)
		{
			Label = label;
			Description = description;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
