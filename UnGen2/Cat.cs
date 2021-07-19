namespace UnGen2
{
	using System.Collections.Generic;
	using System.IO;

	public class Cat
	{
		private const string Magic = "NyanNyanNyanNyan";

		public readonly List<CatEntry> Entries = new();

		public Cat(BinaryReader reader)
		{
			if (new string(reader.ReadChars(16)) != Cat.Magic)
				throw new("Cat: wrong magic");

			while (reader.BaseStream.Position < reader.BaseStream.Length)
				this.Entries.Add(new(reader.ReadBytes(20), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()));
		}
	}
}
