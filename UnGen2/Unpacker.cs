namespace UnGen2;

using System.IO;
using System.IO.Compression;

public static class Unpacker
{
	private const int ZlibMagic = 0xDA78;

	public static byte[] Unpack(byte[] data)
	{
		var reader = new BinaryReader(new MemoryStream(data));

		var output = new MemoryStream();

		try
		{
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				var uncompressedSize = (reader.ReadByte() << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | (reader.ReadByte() << 0);
				var compressedSize = (reader.ReadByte() << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | (reader.ReadByte() << 0);

				var magic = reader.ReadUInt16();
				reader.BaseStream.Position -= 2;

				var compressed = reader.ReadBytes(compressedSize);

				if (magic == Unpacker.ZlibMagic)
				{
					var zlib = new ZLibStream(new MemoryStream(compressed), CompressionMode.Decompress);
					var uncompressed = new byte[uncompressedSize];

					for (var written = 0; written < uncompressed.Length;)
						written += zlib.Read(uncompressed, written, uncompressed.Length - written);

					output.Write(uncompressed);
				}
				else
					output.Write(compressed);
			}
		}
		catch
		{
			output.Write(reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)));
		}

		return output.ToArray();
	}
}
