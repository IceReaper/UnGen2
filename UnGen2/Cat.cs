namespace UnGen2;

using System.Collections.Generic;
using System.IO;

public class Cat
{
	public record CatEntry(byte[] Sha1, int Offset, int Size, int CasId);

	private const string Magic = "NyanNyanNyanNyan";

	public readonly List<CatEntry> Entries = new();

	public Cat(BinaryReader reader)
	{
		var magic = new string(reader.ReadChars(16));

		if (magic != Cat.Magic)
			throw new($"Cat: magic must be {magic}");

		while (reader.BaseStream.Position < reader.BaseStream.Length)
		{
			var sha1 = reader.ReadBytes(20);
			var offset = reader.ReadInt32();
			var size = reader.ReadInt32();
			var casId = reader.ReadInt32();

			this.Entries.Add(new(sha1, offset, size, casId));
		}
	}
}