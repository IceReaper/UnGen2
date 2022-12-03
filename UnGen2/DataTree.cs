namespace UnGen2;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class DataTree
{
	public readonly object Root;

	public DataTree(BinaryReader reader)
	{
		var (type, flags) = DataTree.ReadHeader(reader);
		this.Root = DataTree.Read(reader, type, flags);
	}

	private static (int type, int flags) ReadHeader(BinaryReader reader)
	{
		var value = reader.ReadByte();

		return (value & 0x1f, value >> 5);
	}

	private static List<object> ReadList(BinaryReader reader)
	{
		var list = new List<object>();
		var size = DataTree.ReadPackedInt(reader);
		var start = reader.BaseStream.Position;

		while (true)
		{
			var (type, flags) = DataTree.ReadHeader(reader);

			if (type == 0x00)
				break;

			list.Add(DataTree.Read(reader, type, flags));
		}

		if (size != reader.BaseStream.Position - start)
			throw new("DataTree: size for list mismatch");

		return list;
	}

	private static Dictionary<string, object> ReadDictionary(BinaryReader reader)
	{
		var dictionary = new Dictionary<string, object>();
		var size = DataTree.ReadPackedInt(reader);
		var start = reader.BaseStream.Position;

		while (true)
		{
			var (type, flags) = DataTree.ReadHeader(reader);

			if (type == 0x00)
				break;

			dictionary.Add(DataTree.ReadString(reader, true), DataTree.Read(reader, type, flags));
		}

		if (size != reader.BaseStream.Position - start)
			throw new("DataTree: size for dictionary mismatch");

		return dictionary;
	}

	private static string ReadString(BinaryReader reader, bool skipLength = false)
	{
		var result = new List<byte>();
		byte value;

		var length = skipLength ? 0 : reader.ReadByte();

		while ((value = reader.ReadByte()) != 0)
			result.Add(value);

		if (!skipLength && result.Count + 1 != length)
			throw new("DataTree: Wrong string");

		return Encoding.ASCII.GetString(result.ToArray());
	}

	private static object Read(BinaryReader reader, int type, int flags)
	{
		// TODO use flags!
		return type switch
		{
			0x00 => throw new("DataTree: Type 0x00 not implemented"),
			0x01 => DataTree.ReadList(reader),
			0x02 => DataTree.ReadDictionary(reader),
			0x03 => throw new("DataTree: Type 0x03 not implemented"),
			0x04 => throw new("DataTree: Type 0x04 not implemented"),
			0x05 => throw new("DataTree: Type 0x05 not implemented"),
			0x06 => reader.ReadByte() != 0x00,
			0x07 => DataTree.ReadString(reader),
			0x08 => reader.ReadInt32(),
			0x09 => reader.ReadInt64(),
			0x0a => throw new("DataTree: Type 0x0a not implemented"),
			0x0b => throw new("DataTree: Type 0x0b not implemented"),
			0x0c => throw new("DataTree: Type 0x0c not implemented"),
			0x0d => throw new("DataTree: Type 0x0d not implemented"),
			0x0e => throw new("DataTree: Type 0x0e not implemented"),
			0x0f => new Guid(reader.ReadBytes(16)),
			0x10 => reader.ReadBytes(20),
			0x11 => throw new("DataTree: Type 0x11 not implemented"),
			0x12 => throw new("DataTree: Type 0x12 not implemented"),
			0x13 => reader.ReadBytes(DataTree.ReadPackedInt(reader)),
			0x14 => throw new("DataTree: Type 0x14 not implemented"),
			0x15 => throw new("DataTree: Type 0x15 not implemented"),
			0x16 => throw new("DataTree: Type 0x16 not implemented"),
			0x17 => throw new("DataTree: Type 0x17 not implemented"),
			0x18 => throw new("DataTree: Type 0x18 not implemented"),
			0x19 => throw new("DataTree: Type 0x19 not implemented"),
			0x1a => throw new("DataTree: Type 0x1a not implemented"),
			0x1b => throw new("DataTree: Type 0x1b not implemented"),
			0x1c => throw new("DataTree: Type 0x1c not implemented"),
			0x1d => throw new("DataTree: Type 0x1d not implemented"),
			0x1e => throw new("DataTree: Type 0x1e not implemented"),
			0x1f => throw new("DataTree: Type 0x1f not implemented"),
			_ => throw new("DataTree: Wrong type")
		};
	}

	private static int ReadPackedInt(BinaryReader reader)
	{
		var result = 0;

		for (var i = 0;; i++)
		{
			var value = reader.ReadByte();
			result |= (value & 0x7f) << (7 * i);

			if (value >> 7 == 0)
				break;
		}

		return result;
	}
}
