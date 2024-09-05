using System.Text;

namespace ValveTypeScriptConverter;

internal class CS2KV3
{
	private static readonly byte firstByte = 4;
	private static readonly string firstText = "3VK";
	private static readonly long unknown01 = 5086823378859464316;
	private static readonly long unknown02 = -1785799619110309201;
	private static readonly int type = 0;
	private static readonly long unknown03 = 0;
	private static readonly int unknown05Const = 2;
	private static readonly long unknown08Const = 0;
	private static readonly long unknown09Const = 0;
	private static readonly long unknown10Const = 1;
	private static readonly int endUnknown = -1123072;

	public static byte[] Serialize(string data) {
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

		var textSize = textForBytes.Length;
		var numberStrings = 1 + (publicMethods.Count * 2);
		var numberKeyValue = publicMethods.Count;

		var unknown04 = 4 + (numberKeyValue * 2);
		var unknown06Const = 20 + textSize + (numberKeyValue * 8);
		var unknown07Const = 20 + textSize + (numberKeyValue * 8);

		bytes.Add(firstByte);
		bytes.AddRange(Encoding.ASCII.GetBytes(firstText));
		bytes.AddRange(BitConverter.GetBytes(unknown01));
		bytes.AddRange(BitConverter.GetBytes(unknown02));
		bytes.AddRange(BitConverter.GetBytes(type));
		bytes.AddRange(BitConverter.GetBytes(unknown03));
		bytes.AddRange(BitConverter.GetBytes(unknown04));
		bytes.AddRange(BitConverter.GetBytes(textSize));
		bytes.AddRange(BitConverter.GetBytes(unknown05Const));
		bytes.AddRange(BitConverter.GetBytes(unknown06Const));
		bytes.AddRange(BitConverter.GetBytes(unknown07Const));
		bytes.AddRange(BitConverter.GetBytes(unknown08Const));
		bytes.AddRange(BitConverter.GetBytes(unknown09Const));
		bytes.AddRange(BitConverter.GetBytes(numberStrings));
		bytes.AddRange(BitConverter.GetBytes(unknown10Const));
		bytes.AddRange(BitConverter.GetBytes(numberKeyValue));

		for (int i = 0; i < numberKeyValue * 2; i++) {
			bytes.AddRange(BitConverter.GetBytes(i + 1));
		}

		bytes.AddRange(Encoding.ASCII.GetBytes(textForBytes));
		bytes.AddRange(BitConverter.GetBytes(endUnknown));

		return bytes.ToArray();
	}
}
