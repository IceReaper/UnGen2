namespace UnGen2;

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

			if (stream.Length >= 4)
			{
				var test = reader.ReadUInt32();
				stream.Position = 0;

				if (test == Decrypter.Magic)
					reader = new(stream = new MemoryStream(Decrypter.Decrypt(reader)));
			}

			if (file.EndsWith(".cat"))
				cats.Add(Path.GetRelativePath(path, file).Replace('\\', '/'), new(reader));
			else if (file.EndsWith(".cas"))
				cass.Add(Path.GetRelativePath(path, file).Replace('\\', '/'), new(reader));
			else if (file.EndsWith(".fb2"))
			{
				var zip = new ZipArchive(stream);

				foreach (var entry in zip.Entries.Where(entry => entry.Name != ""))
				{
					var data = new byte[entry.Length];
					stream = new MemoryStream(data);
					reader = new(stream);
					var entryStream = entry.Open();

					for (var written = 0; written < data.Length;)
						written += entryStream.Read(data, written, data.Length - written);

					dataTrees.Add(entry.FullName, new(reader));
				}
			}
			else if (file.EndsWith(".bin"))
			{
				// This file is one large string with the following setup:
				// \r\n\r\n
				// layout.bin
				// _contents_
				// 'x' + (char[256] hash) + 'x' // Decrypter has a similar structure
				// _crc_check
				// \r\n\r\n\r\n
			}
			else if (file.EndsWith(".toc") || file.EndsWith(".sb"))
				dataTrees.Add(Path.GetRelativePath(path, file).Replace('\\', '/'), new(reader));
		}

		foreach (var (dataTreeName, dataTree) in dataTrees)
		{
			// TODO there is ALWAYS a .,toc for a .sb => Maybe toc points to SB entries?
			Console.WriteLine($"DataTree {dataTreeName}:");

			if (dataTreeName == "Data/Win32/Loc/en.sb")
			{
				// TODO this can NOT be - this file is 450MB!
				var data = DataTreeParser.ParseLocaleSuperBundle(dataTree);
			}
			else if (dataTreeName == "Data/Win32/Loc/en.toc")
			{
				var data = DataTreeParser.ParseLocaleTableOfContents(dataTree);
			}
			else if (dataTreeName is "layout.m" or "Data/layout.toc")
			{
				var data = DataTreeParser.ParseLayout(dataTree);
			}
			else if (dataTreeName.EndsWith(".m"))
			{
				var data = DataTreeParser.ParseManifests(dataTree);
			}
			else if (dataTreeName.EndsWith(".sb"))
			{
				var data = DataTreeParser.ParseSuperBundle(dataTree);
			}
			else if (dataTreeName.EndsWith(".toc"))
			{
				var data = DataTreeParser.ParseTableOfContents(dataTree);
			}
		}

		foreach (var (name, cat) in cats)
		{
			for (var i = 0; i < cat.Entries.Count; i++)
			{
				var catEntry = cat.Entries[i];
				var cas = cass[$"{name[..^4]}_{catEntry.CasId:d2}.cas"];
				var data = cas.GetData(catEntry);
				Directory.CreateDirectory($"output/{name}");
			}
		}
	}
}
