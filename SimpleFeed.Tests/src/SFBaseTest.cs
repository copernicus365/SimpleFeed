using System.IO;

namespace SFTests;

public class SFBaseTest
{
	public static string DataString(string path)
	{
		string fpath = _getDataPath(path);
		string content = File.ReadAllText(fpath);
		True(content != null);
		return content;
	}

	public static byte[] DataBytes(string path)
	{
		string fpath = _getDataPath(path);
		var content = File.ReadAllBytes(fpath);
		True(content != null);
		return content;
	}

	static string _getDataPath(string path)
	{
		string fpath = Path.Combine(Environment.CurrentDirectory, "data", path); //ProjectPath.deb .BinPath(path), path);
		return fpath;
	}

	public static void True(bool isTrue)
		=> Assert.True(isTrue);
}
