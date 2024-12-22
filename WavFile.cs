namespace FLACDecoder;

internal class WavFile {
	internal readonly DateTime Create, LastWrite;
	internal WavHeader Header;
	internal string Dir, Name, FullName;
	internal long Size, DataSize;

	internal WavFile(FileInfo i) {
		Create = i.CreationTimeUtc;
		LastWrite = i.LastWriteTimeUtc;
		Header = new();
		Dir = (i.DirectoryName?.Replace('\\', '/') ?? "") + '/';
		Name = i.Name.Replace(i.Extension, "");
		FullName = i.FullName.Replace('\\', '/');
		Size = i.Length;
	}
}