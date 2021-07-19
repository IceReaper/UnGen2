namespace UnGen2
{
	using System.IO;
	using System.Linq;

	public class Cas
	{
		private readonly BinaryReader reader;

		public Cas(BinaryReader reader)
		{
			this.reader = reader;
		}

		public byte[] GetData(CatEntry catEntry)
		{
			this.reader.BaseStream.Position = catEntry.Offset - 32;
			var unk1 = this.reader.ReadUInt32();
			var sha1 = this.reader.ReadBytes(20);
			var fileSize = this.reader.ReadInt32();
			var unk2 = this.reader.ReadInt32();

			if (unk1 != 0xF00FCEFA) // FACE0FF :D
				throw new("Cas: Wrong unk1");
			
			if (!catEntry.Sha1.SequenceEqual(sha1))
				throw new("Cas: Wrong sha1");

			if (catEntry.Size != fileSize)
				throw new("Cas: Wrong fileSize");

			if (unk2 != 0)
				throw new("Cas: Wrong unk2");

			return this.reader.ReadBytes(catEntry.Size);
		}
	}
}
