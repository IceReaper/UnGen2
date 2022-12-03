namespace UnGen2;

using System.IO;
using System.Linq;
using System.Text;

public static class Decrypter
{
	public const uint Magic = 0x00CED100;

	public static byte[] Decrypt(BinaryReader reader)
	{
		var magic = reader.ReadUInt32();
		var unk1 = reader.ReadInt32();

		// layout.bin has a similar structure
		var unk2 = reader.ReadChar();
		var unkHash = Encoding.ASCII.GetString(reader.ReadBytes(256));
		var unk3 = reader.ReadChar();

		var unk4 = reader.ReadBytes(30);
		var table = reader.ReadBytes(257);
		var unk5 = reader.ReadBytes(3);
		var encrypted = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

		if (magic != Decrypter.Magic)
			throw new($"Decrypter: magic must be {Decrypter.Magic:X8}");

		if (unk1 != 0)
			throw new("Decrypter: unk1 must be empty");

		if (unk2 != 'x')
			throw new("Decrypter: unk2");

		// TODO hash

		if (unk3 != 'x')
			throw new("Decrypter: unk3");

		if (unk4.Any(value => value != 0))
			throw new("Decrypter: unk4 must be empty");

		if (unk5.Any(value => value != 0))
			throw new("Decrypter: unk5 must be empty");

		return encrypted.Select((value, index) => (byte)(value ^ table[index % table.Length] ^ 0x7b)).ToArray();
	}
}