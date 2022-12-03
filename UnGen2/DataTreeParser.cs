namespace UnGen2;

using System;
using System.Collections.Generic;
using System.Linq;

public static class DataTreeParser
{
#region General Files

	public record SuperBundle(string? Name, IEnumerable<Bundle> Bundles, IEnumerable<Chunk> Chunks);

	public record Bundle(
		string Path,
		int MagicSalt,
		IEnumerable<EbxDbx> Ebx,
		IEnumerable<EbxDbx> Dbx,
		IEnumerable<Resource> Res,
		IEnumerable<Chunk> Chunks,
		IEnumerable<ChunkMeta> ChunkMeta,
		bool AlignMembers,
		long TotalSize,
		long DbxTotalSize
	);

	public record EbxDbx(string Name, byte[] Sha1, long Size, long OriginalSize, byte[]? Idata);

	public record Resource(string Name, byte[] Sha1, long Size, long OriginalSize, int ResType, byte[] ResMeta, byte[]? Idata);

	public record Chunk(Guid Id, byte[] Sha1, long? Size, byte[]? Idata);

	public record ChunkMeta(int H32, Meta Meta);

	public record Meta;

	public static SuperBundle ParseSuperBundle(DataTree dataTree)
	{
		var root = (Dictionary<string, object>)dataTree.Root;

		return DataTreeParser.ParseSuperBundle(root);
	}

	private static SuperBundle ParseSuperBundle(IReadOnlyDictionary<string, object> superBundle)
	{
		return new(
			superBundle.TryGetValue("name", out var name) ? (string)name : null,
			superBundle.TryGetValue("bundles", out var bundles)
				? ((List<object>)superBundle["bundles"]).Select(e => DataTreeParser.ParseBundle((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<Bundle>(),
			superBundle.TryGetValue("chunks", out var chunks)
				? ((List<object>)chunks).Select(e => DataTreeParser.ParseChunk((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<Chunk>()
		);
	}

	private static Bundle ParseBundle(IReadOnlyDictionary<string, object> bundle)
	{
		return new(
			(string)bundle["path"],
			(int)bundle["magicSalt"],
			bundle.ContainsKey("ebx")
				? ((List<object>)bundle["ebx"]).Select(e => DataTreeParser.ParseEbxDbx((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<EbxDbx>(),
			bundle.ContainsKey("dbx")
				? ((List<object>)bundle["dbx"]).Select(e => DataTreeParser.ParseEbxDbx((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<EbxDbx>(),
			bundle.ContainsKey("res")
				? ((List<object>)bundle["res"]).Select(e => DataTreeParser.ParseResource((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<Resource>(),
			bundle.ContainsKey("chunks")
				? ((List<object>)bundle["chunks"]).Select(e => DataTreeParser.ParseChunk((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<Chunk>(),
			bundle.ContainsKey("chunkMeta")
				? ((List<object>)bundle["chunkMeta"]).Select(e => DataTreeParser.ParseChunkMeta((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<ChunkMeta>(),
			(bool)bundle["alignMembers"],
			(long)bundle["totalSize"],
			(long)bundle["dbxTotalSize"]
		);
	}

	private static EbxDbx ParseEbxDbx(IReadOnlyDictionary<string, object> ebxDbx)
	{
		return new(
			(string)ebxDbx["name"],
			(byte[])ebxDbx["sha1"],
			(long)ebxDbx["size"],
			(long)ebxDbx["originalSize"],
			ebxDbx.TryGetValue("idata", out var idata) ? (byte[])idata : null
		);
	}

	private static Resource ParseResource(IReadOnlyDictionary<string, object> resource)
	{
		return new(
			(string)resource["name"],
			(byte[])resource["sha1"],
			(long)resource["size"],
			(long)resource["originalSize"],
			(int)resource["resType"],
			(byte[])resource["resMeta"],
			resource.TryGetValue("idata", out var idata) ? (byte[])idata : null
		);
	}

	private static Chunk ParseChunk(IReadOnlyDictionary<string, object> chunk)
	{
		return new(
			(Guid)chunk["id"],
			(byte[])chunk["sha1"],
			chunk.TryGetValue("size", out var size) ? (long)size : null,
			chunk.TryGetValue("idata", out var idata) ? (byte[])idata : null
		);
	}

	private static ChunkMeta ParseChunkMeta(IReadOnlyDictionary<string, object> chunkMeta)
	{
		return new((int)chunkMeta["h32"], DataTreeParser.ParseMeta((Dictionary<string, object>)chunkMeta["meta"]));
	}

	private static Meta ParseMeta(IReadOnlyDictionary<string, object> meta)
	{
		if (meta.Count > 0)
			throw new NotSupportedException();

		return new();
	}

#endregion

#region Locale Files

	public record LocaleSuperBundle(int Value);

	public record LocaleTableOfContents(
		Guid Tag,
		IEnumerable<LocaleTableOfContentsBundle> Bundles,
		IEnumerable<LocaleTableOfContentsChunk> Chunks,
		string Name,
		bool AlwaysEmitSuperbundle
	);

	public record LocaleTableOfContentsBundle;

	public record LocaleTableOfContentsChunk(Guid Id, long Offset, int Size);

	public static LocaleSuperBundle ParseLocaleSuperBundle(DataTree dataTree)
	{
		return new((int)dataTree.Root);
	}

	public static LocaleTableOfContents ParseLocaleTableOfContents(DataTree dataTree)
	{
		var root = (Dictionary<string, object>)dataTree.Root;

		return new(
			(Guid)root["tag"],
			((List<object>)root["bundles"]).Select<object, LocaleTableOfContentsBundle>(e => throw new NotSupportedException()).ToArray(),
			root.TryGetValue("chunks", out var chunks)
				? ((List<object>)chunks).Select(e => DataTreeParser.ParseLocaleTableOfContentsChunk((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<LocaleTableOfContentsChunk>(),
			(string)root["name"],
			(bool)root["alwaysEmitSuperbundle"]
		);
	}

	private static LocaleTableOfContentsChunk ParseLocaleTableOfContentsChunk(Dictionary<string, object> chunk)
	{
		return new((Guid)chunk["id"], (long)chunk["offset"], (int)chunk["size"]);
	}

#endregion

#region Manifest Files

	public record Manifest(long Pos, string Kind, Guid? Manifests, string? Name, int? Osize, byte[]? Sha1, long? Offset, long? Length, byte[]? Bytes);

	public static IEnumerable<Manifest> ParseManifests(DataTree dataTree)
	{
		var root = (List<object>)dataTree.Root;

		return root.Select(e => (Dictionary<string, object>)e)
			.Select(
				entry => new Manifest(
					(long)entry["pos"],
					(string)entry["kind"],
					entry.TryGetValue("id", out var id) ? (Guid)id : null,
					entry.TryGetValue("name", out var name) ? (string)name : null,
					entry.TryGetValue("osize", out var osize) ? (int)osize : null,
					entry.TryGetValue("sha1", out var sha1) ? (byte[])sha1 : null,
					entry.TryGetValue("offset", out var offset) ? (long)offset : null,
					entry.TryGetValue("length", out var length) ? length as int? ?? (long)entry["length"] : null,
					entry.TryGetValue("manifest", out var manifest) ? (byte[])manifest : null
				)
			)
			.ToArray();
	}

#endregion

#region Layout Files

	public record Layout(IEnumerable<SuperBundle> SuperBundles, IEnumerable<string> Fs, int Head);

	public static Layout ParseLayout(DataTree dataTree)
	{
		var root = (Dictionary<string, object>)dataTree.Root;

		return new(
			((List<object>)root["superBundles"]).Select(e => DataTreeParser.ParseSuperBundle((Dictionary<string, object>)e)).ToArray(),
			root.TryGetValue("fs", out var fs) ? ((List<object>)fs).Select(e => (string)e).ToArray() : Array.Empty<string>(),
			(int)root["head"]
		);
	}

#endregion

#region Table Of Contents Files

	public record TableOfContents(IEnumerable<TableOfContentsBundle> Bundles, IEnumerable<Chunk> Chunks, bool Cas, string Name, bool AlwaysEmitSuperbundle);

	public record TableOfContentsBundle(string Id, long Offset, int Size);

	public static TableOfContents ParseTableOfContents(DataTree dataTree)
	{
		var root = (Dictionary<string, object>)dataTree.Root;

		return new(
			root.TryGetValue("bundles", out var bundles)
				? ((List<object>)bundles).Select(e => DataTreeParser.ParseTableOfContentsBundle((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<TableOfContentsBundle>(),
			root.TryGetValue("chunks", out var chunks)
				? ((List<object>)chunks).Select(e => DataTreeParser.ParseChunk((Dictionary<string, object>)e)).ToArray()
				: Array.Empty<Chunk>(),
			(bool)root["cas"],
			(string)root["name"],
			(bool)root["alwaysEmitSuperbundle"]
		);
	}

	private static TableOfContentsBundle ParseTableOfContentsBundle(IReadOnlyDictionary<string, object> bundle)
	{
		return new((string)bundle["id"], (long)bundle["offset"], (int)bundle["size"]);
	}

#endregion
}
