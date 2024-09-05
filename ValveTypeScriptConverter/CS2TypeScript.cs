using System.Text;

namespace ValveTypeScriptConverter;

internal class CS2TypeScript
{
	private static readonly int unknown = 131084;
	private static readonly int Version = 8;
	private static readonly int unknown2 = 3;

	private string Data;

	internal CS2TypeScript(string path) {
		using var streamFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		var buffer = new byte[streamFile.Length];
		var bytesRead = streamFile.Read(buffer, 0, buffer.Length);
		this.Data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
	}

	internal void Save(string fPath) {
		var data_Size = this.Data.Length;
		var newData = new List<byte>();
		var STATBytes = CS2KV3.Serialize(this.Data);
		//13*4=52
		var FileSize = data_Size + 52 + STATBytes.Length;

		newData.AddRange(BitConverter.GetBytes(FileSize));
		newData.AddRange(BitConverter.GetBytes(unknown));
		newData.AddRange(BitConverter.GetBytes(Version));
		newData.AddRange(BitConverter.GetBytes(unknown2));
		newData.AddRange(Encoding.ASCII.GetBytes("RED2"));
		newData.AddRange(BitConverter.GetBytes(0)); // offset
		newData.AddRange(BitConverter.GetBytes(0)); // size
		newData.AddRange(Encoding.ASCII.GetBytes("DATA"));
		newData.AddRange(BitConverter.GetBytes(20)); // offset
		newData.AddRange(BitConverter.GetBytes(data_Size)); // size
		newData.AddRange(Encoding.ASCII.GetBytes("STAT"));

		if (STATBytes.Length > 0) {
			newData.AddRange(BitConverter.GetBytes(data_Size + 8)); //offset
			newData.AddRange(BitConverter.GetBytes(STATBytes.Length)); //size
		} else {
			newData.AddRange(BitConverter.GetBytes(0)); // offset
			newData.AddRange(BitConverter.GetBytes(0)); // size
		}

		newData.AddRange(Encoding.ASCII.GetBytes(this.Data)); // size
		newData.AddRange(STATBytes); // size
		
		File.WriteAllBytes(fPath, newData.ToArray());
	}
}
