using System.Text;

namespace ValveTypeScriptConverter;

internal class CS2TypeScript
{
	public int FileSize {get; set;}
	public int unknown {get; set;} = 131084;
	public int Version {get; set;} = 8;
	public int unknown2 {get; set;} = 3;

	public CS2KV3? RED2 {get; set;}
	public int RED2_Offset {get; set;} = 0;
	public int RED2_Size {get; set;} = 0;

	public string? Data {get; set;}
	public int Data_Offset {get; set;} = 0;
	public int Data_Size {get; set;} = 0;

	public CS2KV3? STAT {get; set;}
	public int STAT_Offset {get; set;} = 0;
	public int STAT_Size {get; set;} = 0;

	internal CS2TypeScript(string path) {
		var ext = Path.GetExtension(path).Trim();
		if (ext == ".vts") {
			using var streamFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var buffer = new byte[streamFile.Length];
			var bytesRead = streamFile.Read(buffer, 0, buffer.Length);
			this.Data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
		}
	}

	internal bool Save(string fPath) {
		if (this.Data == null) {
			return false;
		}

		this.Data_Size = this.Data.Length;
		var newData = new List<byte>();
		var STATBytes = CS2KV3.Serialize(this.Data);
		//13*4=52
		this.FileSize = this.Data_Size + 52 + STATBytes.Length;

		newData.AddRange(BitConverter.GetBytes(this.FileSize));
		newData.AddRange(BitConverter.GetBytes(this.unknown));
		newData.AddRange(BitConverter.GetBytes(this.Version));
		newData.AddRange(BitConverter.GetBytes(this.unknown2));
		newData.AddRange(Encoding.ASCII.GetBytes("RED2"));
		newData.AddRange(BitConverter.GetBytes((int)0)); //offset
		newData.AddRange(BitConverter.GetBytes((int)0)); //size
		newData.AddRange(Encoding.ASCII.GetBytes("DATA"));
		newData.AddRange(BitConverter.GetBytes(20)); //offset
		newData.AddRange(BitConverter.GetBytes(this.Data_Size)); //size
		newData.AddRange(Encoding.ASCII.GetBytes("STAT"));

		if (STATBytes.Length > 0) {
			newData.AddRange(BitConverter.GetBytes(this.Data_Size + 8)); //offset
			newData.AddRange(BitConverter.GetBytes(STATBytes.Length)); //size
		} else {
			newData.AddRange(BitConverter.GetBytes((int)0)); //offset
			newData.AddRange(BitConverter.GetBytes((int)0)); //size
		}

		newData.AddRange(Encoding.ASCII.GetBytes(this.Data)); //size
		newData.AddRange(STATBytes); //size
		
		File.WriteAllBytes(fPath, newData.ToArray());

		return true;
	}
}
