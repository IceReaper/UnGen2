namespace UnGen2;

using System.IO;
using System.Linq;
using System.Security.Cryptography;

public class Cas
{
	private const uint Magic = 0xF00FCEFA;

	private readonly BinaryReader reader;

	public Cas(BinaryReader reader)
	{
		this.reader = reader;
	}

	public byte[] GetData(Cat.CatEntry catEntry)
	{
		this.reader.BaseStream.Position = catEntry.Offset - 32;

		var magic = this.reader.ReadUInt32();
		var sha1 = this.reader.ReadBytes(20);
		var size = this.reader.ReadInt32();
		var unk = this.reader.ReadInt32();
		var data = this.reader.ReadBytes(size);

		if (magic != Cas.Magic)
			throw new($"Cas: magic must be {Cas.Magic:X8}");

		if (!catEntry.Sha1.SequenceEqual(sha1))
			throw new("Cas: sha1 not equal to cat sha1");

		if (catEntry.Size != size)
			throw new("Cas: size not equal to cat size");

		if (unk != 0)
			throw new("Cas: unk must be empty");

		if (!SHA1.HashData(data).SequenceEqual(sha1))
			throw new("Cas: data sha1 mismatch");

		return Unpacker.Unpack(data);
	}
}
