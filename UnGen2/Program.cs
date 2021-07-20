namespace UnGen2
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;

	public static class Program
	{
		public static void Main(string[] args)
		{
			var cats = new Dictionary<string, Cat>();
			var cass = new Dictionary<string, Cas>();
			var dataTrees = new Dictionary<string, DataTree>();

			var path = args.Length > 0 ? args[0] : "";

			foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
			{
				Stream stream = File.OpenRead(file);
				var reader = new BinaryReader(stream);

				var magic = reader.ReadInt32();

				if (magic == Decrypter.Magic)
				{
					var decrypted = Decrypter.Decrypt(reader);

					stream.Dispose();

					stream = new MemoryStream(decrypted);
					reader = new(stream);
				}
				else
					stream.Position = 0;

				// This file contains a single checksum. We can safely ignore it as we wont need it at all.
				if (file.EndsWith("layout.bin"))
					continue;

				if (file.EndsWith(".cat"))
					cats.Add(Path.GetRelativePath(path, file), new(reader));
				else if (file.EndsWith(".cas"))
					cass.Add(Path.GetRelativePath(path, file), new(reader));
				else if (file.EndsWith(".fb2"))
				{
					var zip = new ZipArchive(stream);

					foreach (var entry in zip.Entries.Where(entry => entry.Name != ""))
					{
						Console.WriteLine($"  - {entry.FullName}");

						var data = new byte[entry.Length];
						stream = new MemoryStream(data);
						reader = new(stream);

						entry.Open().Read(data);

						dataTrees.Add(entry.FullName, new(reader));
					}
				}
				else
					dataTrees.Add(Path.GetRelativePath(path, file), new(reader));
			}

			var i = 0;

			foreach (var (name, cat) in cats)
			{
				Console.WriteLine(name);

				foreach (var catEntry in cat.Entries)
				{
					var cas = cass[$"{name[..^4]}_{catEntry.CasId:d2}.cas"];
					var data = cas.GetData(catEntry);
					var reader = new BinaryReader(new MemoryStream(data));

					var unk1 = reader.ReadByte();
					var unk2 = (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | (reader.ReadByte() << 0);

					switch (unk1)
					{
						case 0x00 when unk2 == 0x000008:
							// TODO
							break;

						case 0x00 when unk2 == 0x000010:
							// TODO
							break;

						case 0x00 when unk2 == 0x000800:
							// TODO
							break;

						case 0x00 when unk2 == 0x001000:
							// TODO
							break;

						case 0x00 when unk2 == 0x002000:
							// TODO
							break;

						case 0x00 when unk2 == 0x004000:
							// TODO
							break;

						case 0x00 when unk2 == 0x008000:
							// TODO
							break;

						case 0x00 when unk2 == 0x010000:
							// TODO
							break;

						case 0x00:
							var uncompressedSize = unk2;
							var compressedSize = (reader.ReadByte() << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | (reader.ReadByte() << 0);

							// TODO some ZLIB here... Not sure about the others, possibly LZ4 and ZStd somewhere!
							break;

						case 0x01 when unk2 == 0x100000:
							// TODO
							// Always followed by 0x14000000
							break;

						case 0x48 when unk2 == 0x00000c:
							// TODO
							// Mostly followed by 0x80bb0014, sometimes 0x80bb0414 or 0x80bb0c14 or 44ac0014 or 44ac0414 
							break;

						case 0x4D when unk2 == 0x566864:
							// TODO
							// Always followed by 0x00000020
							break;

						default:
							Console.WriteLine($"{unk1:x8}\t{unk2}\t{data.Length}");

							Directory.CreateDirectory($"output/{unk1}");

							//File.WriteAllBytes($"output/{fileType}/{i}", data);

							break;
					}

					i++;
				}
			}
		}
	}
}
