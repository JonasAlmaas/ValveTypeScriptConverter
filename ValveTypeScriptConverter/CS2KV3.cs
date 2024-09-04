using System.Text;

namespace ValveTypeScriptConverter;

internal class CS2KV3
{
	public byte[]? Bytes {get; set;}
	public byte firstByte {get; set;} = 4;
	public string firstText {get; set;} = "3VK";
	public long unknown01 {get; set;} = 5086823378859464316;
	public long unknown02 {get; set;} = -1785799619110309201;
	public int type {get; set;} = 0;
	public long unknown03 {get; set;} = 0;
	public long unknown04 {get; set;} = 6;
	public int TextSize {get; set;} = 0;
	public int unknown05Const {get; set;} = 2;
	public int unknown06Const {get; set;} = 64;
	public int unknown07Const {get; set;} = 64;
	public long unknown08Const {get; set;} = 0;
	public long unknown09Const {get; set;} = 0;
	public int numberStrings {get; set;} = 0;
	public long unknown10Const {get; set;} = 1;
	public int numberKeyValue {get; set;} = 0;
	public int endUnknown {get; set;} = -1123072;

	internal CS2KV3() { }

	internal CS2KV3(byte[] bytes) {
		if (bytes.Length == 0) {
			return;
		}

		this.Bytes = bytes;
		var text2 = Encoding.ASCII.GetString(this.Bytes);

		var stream = new MemoryStream(this.Bytes);
		var binaryReader = new BinaryReader(stream);

		this.firstByte = binaryReader.ReadByte();
		this.firstText = new string(binaryReader.ReadChars(3));
		this.unknown01 = binaryReader.ReadInt64();
		this.unknown02 = binaryReader.ReadInt64();
		//var this.unknown03=binaryReader.ReadInt32();
		//var this.unknown04=binaryReader.ReadInt32();
		//binaryReader.ReadBytes(16);
		this.type = binaryReader.ReadInt32();

		var list = new List<uint>();
		var list0 = new List<uint>();

		if (type == 0) {
			unknown03 = binaryReader.ReadInt64();
			unknown04 = binaryReader.ReadInt64();
			//for (int i = 0; i < 4; i++) {
			//    list0.Add(binaryReader.ReadUInt32());
			//}
			//binaryReader.ReadBytes(16);
			this.TextSize = binaryReader.ReadInt32();
			this.unknown05Const = binaryReader.ReadInt32();
			this.unknown06Const = binaryReader.ReadInt32();
			this.unknown07Const = binaryReader.ReadInt32();
			this.unknown08Const = binaryReader.ReadInt64();
			this.unknown09Const = binaryReader.ReadInt64();

			this.numberStrings = binaryReader.ReadInt32();
			this.unknown10Const = binaryReader.ReadInt64();
			this.numberKeyValue = binaryReader.ReadInt32();

			var arrKeyValue = new int[numberKeyValue, 2];
			for (int i = 0; i < numberKeyValue; i++) {
				arrKeyValue[i, 0] = binaryReader.ReadInt32();
				arrKeyValue[i, 1] = binaryReader.ReadInt32();
			}

			var textBySize = new string(binaryReader.ReadChars(TextSize)); //\u0006 A value for each key?
			var arrText = textBySize.Split('\0');
			var keyValuePairs = new Dictionary<string, string>();
			for (int i = 0; i < numberKeyValue; i++) {
				keyValuePairs.Add(arrText[arrKeyValue[i, 0]], arrText[arrKeyValue[i, 1]]);
			}
			int endUnknown = binaryReader.ReadInt32();
		} else {
			var list2 = new List<uint>();
			var list3 = new List<ushort>();
			//binaryReader.ReadBytes(3);
			binaryReader.ReadBytes(16);
			var textSize = binaryReader.ReadInt32();
			for (var i = 0; i < 7; i++) {
				list2.Add(binaryReader.ReadUInt32());
			}

			binaryReader.ReadByte();
			//binaryReader.ReadByte();
			//binaryReader.ReadByte();
			//string text5 = new string(binaryReader.ReadChars(100)) ;
			for (var i = 0; i < 100; i++) {
				list3.Add(binaryReader.ReadUInt16());
			}
		}
	}

	public static byte[] Serialize(string data) {
		var stat = new CS2KV3();
		var bytes = new List<byte>();
		var publicMethods = new List<string>();
		var publicSplt = data.Split("PublicMethod(");
		var textForBytes = "publicMethods\0";

		for (var i = 1; i < publicSplt.Length; i++) {
			var quoteType = publicSplt[i][0];
			var methodSplt = publicSplt[i].Trim().Split(quoteType);
			if (methodSplt.Length < 2) {
				throw new Exception($"Invalid public method decleration \"{publicSplt[i]}\"");
			}

			var method = methodSplt[1];
			publicMethods.Add(method);
			var typeSplt = methodSplt[2].Trim().Split("*");

			if (typeSplt.Length > 1) {
				textForBytes += method + $"\0{typeSplt[1].Trim()}\0";
			} else {
				textForBytes += method + "\0none\0";
			}
		}

		textForBytes += "\t\t";
		for (var i = 0; i < publicMethods.Count; i++) {
			textForBytes += '\u0006';
		}

		stat.TextSize = textForBytes.Length;
		stat.numberStrings = 1 + (publicMethods.Count * 2);
		stat.numberKeyValue = publicMethods.Count;

		stat.unknown04 = 4 + (stat.numberKeyValue * 2);
		stat.unknown06Const = 20 + stat.TextSize + (stat.numberKeyValue * 8);
		stat.unknown07Const = 20 + stat.TextSize + (stat.numberKeyValue * 8);
		bytes.Add(stat.firstByte);
		bytes.AddRange(Encoding.ASCII.GetBytes(stat.firstText));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown01));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown02));
		bytes.AddRange(BitConverter.GetBytes(stat.type));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown03));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown04));
		bytes.AddRange(BitConverter.GetBytes(stat.TextSize));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown05Const));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown06Const));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown07Const));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown08Const));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown09Const));
		bytes.AddRange(BitConverter.GetBytes(stat.numberStrings));
		bytes.AddRange(BitConverter.GetBytes(stat.unknown10Const));
		bytes.AddRange(BitConverter.GetBytes(stat.numberKeyValue));

		for (int i = 0; i < stat.numberKeyValue * 2; i++) {
			//Console.WriteLine(i + 1);
			bytes.AddRange(BitConverter.GetBytes(i + 1));
		}

		bytes.AddRange(Encoding.ASCII.GetBytes(textForBytes));
		bytes.AddRange(BitConverter.GetBytes(stat.endUnknown));

		return bytes.ToArray();
	}
}
