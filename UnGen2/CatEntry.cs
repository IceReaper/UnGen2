namespace UnGen2
{
	public class CatEntry
	{
		public readonly byte[] Sha1;
		public readonly int Offset;
		public readonly int Size;
		public readonly int CasId;

		public CatEntry(byte[] sha1, int offset, int size, int casId)
		{
			this.Sha1 = sha1;
			this.Offset = offset;
			this.Size = size;
			this.CasId = casId;
		}
	}
}
