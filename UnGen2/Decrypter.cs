namespace UnGen2
{
	using System.IO;
	using System.Linq;

	public class Decrypter
	{
		public const int Magic = 0x00CED100; // D1CE :D

		public static byte[] Decrypt(BinaryReader reader)
		{
			if (reader.ReadInt32() != 0)
				throw new("Decrypter: unknown value");

			if (reader.ReadChar() != 'x')
				throw new("Decrypter: unknown hash start");

			var hash = reader.ReadBytes(256); // TODO

			if (reader.ReadChar() != 'x')
				throw new("Decrypter: unknown hash end");

			if (reader.ReadBytes(30).Any(value => value != 0))
				throw new("Decrypter: unknown value");

			var table = reader.ReadBytes(257);

			if (reader.ReadBytes(3).Any(value => value != 0))
				throw new("Decrypter: unknown value");

			var decrypted = new byte[reader.BaseStream.Length - reader.BaseStream.Position];

			for (var i = 0; i < decrypted.Length; i++)
				decrypted[i] = (byte)(reader.ReadByte() ^ table[i % table.Length] ^ 0x7b);

			return decrypted;
		}
	}
}
