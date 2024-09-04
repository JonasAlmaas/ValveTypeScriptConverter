using System;
using System.IO;

namespace ValveTypeScriptConverter;

internal class Program
{
	static void Main(string[] args) {
		/*
		if (args.Length != 1) {
			Console.WriteLine("Usage: ValveTypeScriptConverter <addonPath>");
			Console.WriteLine("  addonPath: The path to the root of the addons content folder");
			Console.WriteLine("    Example: C:\\Program Files (x86)\\Steam\\steamapps\\common\\Counter-Strike Global Offensive\\content\\csgo_addons\\my_addon");
			return;
		}

		var contentPath = args[^1];
		*/

		var contentPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Counter-Strike Global Offensive\\content\\csgo_addons\\warden";

		if (!Directory.Exists(contentPath)) {
			Console.WriteLine(
				$"The specified path \"{contentPath}\" does not exist. Please make sure the path is correct and try again.");
			return;
		}

		var app = new Application(contentPath);
		app.Run();
	}
}

internal class Application
{
	private readonly string contentPath;
	private readonly string gamePath;

	private readonly string relativeVscriptsPath;
	private readonly string contentVscriptsPath;

	private readonly Dictionary<string, FileSystemWatcher> watchers = [];

	private static readonly string contentPattern = Path.Combine("content", "csgo_addons");
	private static readonly string gamePattern = Path.Combine("game", "csgo_addons");

	internal Application(string contentPath) {
		this.contentPath = contentPath;
		this.gamePath = this.contentPath.Replace(contentPattern, gamePattern);
		this.relativeVscriptsPath = Path.Combine("scripts", "vscripts");
		this.contentVscriptsPath = Path.Combine(this.contentPath, this.relativeVscriptsPath);
	}

	internal void Run() {
		log("Starting up ...");
		log($"Content path: \"{this.contentPath}\"");
		log($"Game path: \"{this.gamePath}\"");
		log($"Watching for changes in \"{this.contentVscriptsPath}\" ...");

		while (true) {
			foreach (var fPath in this.getVtsFiles()) {
				if (!watchers.ContainsKey(fPath)) {
					var watcher = this.watchFile(fPath);
					if (watcher != null) {
						watchers.Add(fPath, watcher);
					}
				}
			}

			Thread.Sleep(TimeSpan.FromMilliseconds(500));
		}
	}

	private static void log(string message) {
		Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
	}

	private string[] getVtsFiles() {
		return Directory.GetFiles(contentVscriptsPath, "*.vts", SearchOption.AllDirectories);
	}

	private void compileFile(string fPath) {
		var relativeFPath = Path.GetRelativePath(this.contentPath, fPath);
		var newPath = Path.Combine(this.gamePath, relativeFPath);
		var dPath = Path.GetDirectoryName(newPath);
		if (dPath != null) {
			if (!Directory.Exists(dPath)) {
				Directory.CreateDirectory(dPath);
			}

			try {
				var cs2ts = new CS2TypeScript(fPath);
				cs2ts.Save($"{newPath}_c");
				log($"Successfully compiled \"{relativeFPath}\"");
			} catch (Exception e) {
				log($"Failed to compile \"{relativeFPath}\" - {e.Message}");
			}
		}
	}

	private FileSystemWatcher? watchFile(string fPath) {
		var dirNam = Path.GetDirectoryName(fPath);
		var localFPath = Path.GetRelativePath(this.contentVscriptsPath, fPath);

		if (dirNam == null) {
			return null;
		}
		
		var watcher = new FileSystemWatcher(dirNam) {
			Filter = Path.GetFileName(fPath),
			NotifyFilter = NotifyFilters.LastWrite,
			EnableRaisingEvents = true,
		};

		watcher.Changed += (sender, e) => {
			if (e.FullPath == fPath) {
				log($"File \"{localFPath}\" has been changed");
				this.compileFile(fPath);
			}
		};

		log($"Tracking changes to \"{localFPath}\"");

		// Run the "compiler" once to generate the initial files
		this.compileFile(fPath);

		return watcher;
	}
}
