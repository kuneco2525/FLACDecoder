namespace FLACDecoder;

internal class WavHeader {
	internal const long UINTMOD = (long)uint.MaxValue + 1;
	private static readonly byte[] RIFF = [82, 73, 70, 70], WAVE = [87, 65, 86, 69], FMT = [102, 109, 116, 32], DATA = [100, 97, 116, 97];

	internal byte[] RiffID, WavID, FmtID, DataID;
	internal uint Size, FmtSize, SampleRate, BytePerSec, DataSize;
	internal ushort Format, Channels, BlockSize, Bit;

	internal WavHeader() {
		RiffID = (byte[])RIFF.Clone();
		Size = 0;
		WavID = (byte[])WAVE.Clone();
		FmtID = (byte[])FMT.Clone();
		FmtSize = 16;
		Format = 1;
		Channels = 2;
		SampleRate = 192000;
		BytePerSec = 768000;
		BlockSize = 4;
		Bit = 16;
		DataID = (byte[])DATA.Clone();
		DataSize = 0;
	}

	internal void WriteHeader(BinaryWriter b) {
		b.BaseStream.Position = 0;
		b.Write(RiffID);
		b.Write(Size);
		b.Write(WavID);
		b.Write(FmtID);
		b.Write(FmtSize);
		b.Write(Format);
		b.Write(Channels);
		b.Write(SampleRate);
		b.Write(BytePerSec);
		b.Write(BlockSize);
		b.Write(Bit);
		b.Write(DataID);
		b.Write(DataSize);
	}
}