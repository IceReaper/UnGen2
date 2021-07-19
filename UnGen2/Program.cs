namespace UnGen2
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;

	public static class Program
	{
		public static void Main()
		{
			var cats = new Dictionary<string, Cat>();
			var cass = new Dictionary<string, Cas>();

			var path = "C:/Users/andre/Desktop/Generals2/Data";

			foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
			{
				Console.Write(Path.GetRelativePath(path, file));

				Stream stream = File.OpenRead(file);
				var reader = new BinaryReader(stream);

				var magic = reader.ReadInt32();

				if (magic == Decrypter.Magic)
				{
					Console.Write(" [Encrypted]");
					var decrypted = Decrypter.Decrypt(reader);

					stream.Dispose();

					stream = new MemoryStream(decrypted);
					reader = new(stream);
				}
				else
					stream.Position = 0;

				if (file.EndsWith(".cat"))
					cats.Add(file, new(reader));

				if (file.EndsWith(".cas"))
					cass.Add(file, new(reader));

				if (file.EndsWith(".fb2"))
				{
					Console.Write(" [Zip]");
					var zip = new ZipArchive(stream);

					foreach (var entry in zip.Entries.Where(entry => entry.Name != ""))
						Console.Write($"\n {entry.FullName}");
				}

				Console.Write("\n");
			}

			Console.WriteLine("---");
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
						case 0x00:
							var uncompressedSize = unk2;
							var compressedSize = (reader.ReadByte() << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | (reader.ReadByte() << 0);

							// TODO something is odd here...
							/*if (compressedSize != data.Length - 8)
								Console.WriteLine($"{compressedSize}\t{uncompressedSize}\t{data.Length - 8}");*/

							// TODO some ZLIB here... Not sure about the others, possibly LZ4 and ZStd somewhere!
							break;

						case 0x01 when unk2 == 0x100000:
							// TODO
							// Always followed by 0x14000000
							break;

						case 0x48 when unk2 == 0x00000c:
							// TODO
							// Mosty followed by 0x80bb0014, sometimes 0x80bb0414 or 0x80bb0c14 or 44ac0014 or 44ac0414 
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
