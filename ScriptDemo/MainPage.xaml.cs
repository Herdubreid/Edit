using Microsoft.Maui.Controls.Compatibility;
using System.Runtime.InteropServices;
using SharpHook;
using SharpHook.Native;

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
				_running = false;
				OnPropertyChanged(nameof(StartLabel));
				return;
			}

			if (e.Data.KeyCode == KeyCode.VcV &&
				(e.RawEvent.Mask & ModifierMask.RightCtrl) != ModifierMask.None)
			{
				await Application.Current.Dispatcher.DispatchAsync(()
					=> Clipboard.SetTextAsync(_lines[_lineNo++]));
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

		void OnEditorTextChanged(object sender, TextChangedEventArgs e)
		{
			string oldText = e.OldTextValue;
			string newText = e.NewTextValue;
			string myText = editor.Text;
		}
		void OnEditorCompleted(object sender, EventArgs e)
		{
			string text = ((Editor)sender).Text;
		}
		private readonly TaskPoolGlobalHook _hook;
		public MainPage()
		{
			InitializeComponent();
			BindingContext = this;

			_hook = new TaskPoolGlobalHook();
			_hook.KeyReleased += KeyReleased;

			Task.Run(async () =>
			{
				await _hook.RunAsync();
			});
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_hook.Dispose();
		}

	}
}
