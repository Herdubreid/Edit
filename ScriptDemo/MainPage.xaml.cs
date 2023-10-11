using SharpHook;
using SharpHook.Native;
using System.Diagnostics;

namespace ScriptDemo
{
	public partial class MainPage : ContentPage
	{
		string[] _lines;
		int _lineNo;
		bool _running = false;
		public string StartLabel => _running ? "Stop" : "Start";
		private async void KeyReleased(object sender, KeyboardHookEventArgs e)
		{
			if (!_running) return;
			if (_lineNo >= _lines.Length)
			{
				OnPropertyChanged(nameof(StartLabel));
				_running = false;
				return;
			}

			if (e.Data.KeyCode == KeyCode.VcV &&
				(e.RawEvent.Mask & ModifierMask.RightCtrl) != ModifierMask.None)
			{
				await Clipboard.SetTextAsync(_lines[_lineNo++]);
			}
		}
		private async void StartClicked(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(editor.Text)) return;

			_running = !_running;
			if (_running)
			{
				_lines = editor.Text.Split('\r');
				_lineNo = 1;
				if (_lines.Length > 0)
				{
					await Clipboard.SetTextAsync(_lines[0]);
				}
				else
				{
					_running = false;
				}
			}
			OnPropertyChanged(nameof(StartLabel));
		}

		private TaskPoolGlobalHook _hook;
		public MainPage()
		{
			InitializeComponent();
			BindingContext = this;

			_hook = new TaskPoolGlobalHook();
			_hook.KeyReleased += KeyReleased;

			_ = Task.Run(() => _hook.Run());
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_hook.Dispose();
		}
	}
}
