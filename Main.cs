using NAudio.Flac;
using System.Text.RegularExpressions;

namespace FLACDecoder;

internal class Main : Form {
	private static readonly Regex FILE = new(@"^.+[\\/][^\\/]+\.flac$"), DIR = new(@"^.+[\\/]$");
	private static readonly List<WavFile> FILES = [];

	private static string txt = "";

	private readonly ListBox ListFiles = new();
	private readonly Button ButtonRemove = new(), ButtonClear = new(), ButtonExecute = new();
	private readonly Label Label1 = new(), LabelPath = new();
	private readonly TextBox TextOutput = new();

	private void InitializeComponent() {
		SuspendLayout();
		ListFiles.AllowDrop = true;
		ListFiles.ForeColor = Color.DarkGreen;
		ListFiles.FormattingEnabled = true;
		ListFiles.ItemHeight = 15;
		ListFiles.Location = new Point(12, 12);
		ListFiles.Name = "ListFiles";
		ListFiles.SelectionMode = SelectionMode.MultiExtended;
		ListFiles.Size = new Size(348, 94);
		ListFiles.TabStop = false;
		ListFiles.DragDrop += new DragEventHandler(ListFiles_DragDrop);
		ListFiles.DragEnter += new DragEventHandler(ListFiles_DragEnter);
		ButtonRemove.FlatStyle = FlatStyle.Popup;
		ButtonRemove.Location = new Point(366, 12);
		ButtonRemove.Name = "ButtonRemove";
		ButtonRemove.Size = new Size(54, 23);
		ButtonRemove.TabStop = false;
		ButtonRemove.Text = "除外";
		ButtonRemove.Click += new EventHandler(ButtonRemove_Click);
		ButtonClear.FlatStyle = FlatStyle.Popup;
		ButtonClear.Location = new Point(366, 41);
		ButtonClear.Name = "ButtonClear";
		ButtonClear.Size = new Size(54, 23);
		ButtonClear.TabStop = false;
		ButtonClear.Text = "クリア";
		ButtonClear.Click += new EventHandler(ButtonClear_Click);
		ButtonExecute.FlatStyle = FlatStyle.Popup;
		ButtonExecute.Location = new Point(366, 83);
		ButtonExecute.Name = "ButtonExecute";
		ButtonExecute.Size = new Size(54, 23);
		ButtonExecute.TabIndex = 0;
		ButtonExecute.Text = "実行";
		ButtonExecute.Click += new EventHandler(ButtonExecute_Click);
		Label1.AutoSize = true;
		Label1.Location = new Point(12, 115);
		Label1.Name = "Label1";
		Label1.Size = new Size(137, 15);
		Label1.Text = "出力パス(省略で同じ場所)";
		TextOutput.ImeMode = ImeMode.Disable;
		TextOutput.Location = new Point(155, 112);
		TextOutput.Name = "TextOutput";
		TextOutput.Size = new Size(265, 23);
		TextOutput.TabIndex = 1;
		TextOutput.Text = "";
		TextOutput.TextChanged += new EventHandler(TextOutput_TextChanged);
		LabelPath.AutoSize = true;
		LabelPath.Location = new Point(12, 141);
		LabelPath.Name = "LabelPath";
		LabelPath.Size = new Size(0, 15);
		BackColor = Color.Honeydew;
		ClientSize = new Size(432, 165);
		Controls.Add(LabelPath);
		Controls.Add(TextOutput);
		Controls.Add(Label1);
		Controls.Add(ButtonExecute);
		Controls.Add(ButtonClear);
		Controls.Add(ButtonRemove);
		Controls.Add(ListFiles);
		ForeColor = Color.DarkGreen;
		FormBorderStyle = FormBorderStyle.FixedSingle;
		Name = "Main";
		Text = "FLACDecoder";
		ResumeLayout(false);
		PerformLayout();
	}

	internal Main() => InitializeComponent();

	private void AddFiles(string p) {
		Match m = FILE.Match(p);
		if(m.Success) {
			FILES.Add(new(new(p)));
			return;
		}
		if(!DIR.Match(p).Success) { return; }
		foreach(string f in Directory.GetFiles(p)) { AddFiles(f); }
		foreach(string d in Directory.GetDirectories(p)) { AddFiles(d); }
	}
	private void ListFiles_DragDrop(object? sender, DragEventArgs e) {
		string[] files = (string[])(e.Data?.GetData(DataFormats.FileDrop, false) ?? "");
		for(int i = 0; i < files.Length; ++i) { AddFiles(files[i]); }
		for(int i = 0; i < FILES.Count; ++i) { _ = ListFiles.Items.Add(files[i].Split('/', '\\')[^1]); }
	}
	private void ListFiles_DragEnter(object? sender, DragEventArgs e) { if(e.Data?.GetDataPresent(DataFormats.FileDrop) == true) { e.Effect = DragDropEffects.Copy; } }

	private void ButtonRemove_Click(object? sender, EventArgs e) {
		for(int i = ListFiles.SelectedIndices.Count - 1; i >= 0; --i) {
			FILES.RemoveAt(ListFiles.SelectedIndices[i]);
			ListFiles.Items.RemoveAt(ListFiles.SelectedIndices[i]);
		}
	}

	private void ButtonClear_Click(object? sender, EventArgs e) {
		ListFiles.Items.Clear();
		FILES.Clear();
	}

	private void Decode() {
		foreach(WavFile f in FILES) {
			if(!File.Exists(f.FullName)) { continue; }
			f.Dir = string.IsNullOrWhiteSpace(txt) ? f.Dir : txt.Replace('\\', '/');
			if(!f.Dir.EndsWith('/')) { f.Dir += '/'; }
			if(f.FullName != $"{f.Dir}{f.Name}.flac") { File.Move(f.FullName, f.FullName = $"{f.Dir}{f.Name}.flac"); }
			_ = Invoke(new MethodInvoker(() => LabelPath.Text = f.FullName));
			using(FlacReader o = new(f.FullName)) {
				FlacMetadataStreamInfo meta = (FlacMetadataStreamInfo)o.Metadata[0];
				f.Header = new() {
					Bit = (ushort)meta.BitsPerSample,
					Channels = (ushort)meta.Channels,
					SampleRate = (uint)meta.SampleRate,
				};
				f.Header.BlockSize = (ushort)(f.Header.Bit / 8 * f.Header.Channels);
				f.Header.BytePerSec = f.Header.BlockSize * f.Header.SampleRate;
				f.DataSize = f.Header.BlockSize * meta.TotalSamples;
				f.Size = f.DataSize + 44;
				f.Header.DataSize = (uint)(f.DataSize % WavHeader.UINTMOD);
				f.Header.Size = (uint)(f.Size % WavHeader.UINTMOD);
				using FileStream s = new($"{f.Dir}{f.Name}_tmp.wav", FileMode.Create, FileAccess.Write);
				using BinaryWriter r = new(s);
				f.Header.WriteHeader(r);
				o.CopyTo(s);
			}
			File.Delete(f.FullName);
			File.Move($"{f.Dir}{f.Name}_tmp.wav", $"{f.Dir}{f.Name}.wav");
			File.SetCreationTimeUtc($"{f.Dir}{f.Name}.wav", f.Create);
			File.SetLastWriteTimeUtc($"{f.Dir}{f.Name}.wav", f.LastWrite);
			GC.Collect();
		}
	}
	private async void ButtonExecute_Click(object? sender, EventArgs e) {
		long total = 0;
		foreach(WavFile f in FILES) { total += f.Size; }
		if(total < 1) { return; }
		bool b = false;
		ButtonRemove.Enabled = b;
		ButtonClear.Enabled = b;
		ButtonExecute.Enabled = b;
		TextOutput.Enabled = b;
		await Task.Run(Decode);
		Close();
	}

	private void TextOutput_TextChanged(object? sender, EventArgs e) => txt = TextOutput.Text;
}