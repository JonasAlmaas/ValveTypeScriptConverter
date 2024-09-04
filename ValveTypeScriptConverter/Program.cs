namespace ValveTypeScriptConverter;

internal class Program
{
	static void Main(string[] args) {
		Console.WriteLine($"ValveTypeScriptConverter v{BuildNo()}");

		var cfg = new Application.Config {
			ContentPath = "",
			RelSrcPath = "",
			RelDstPath = "",
			Watch = false
		};

		var positionalArgsIx = 0;

		for (var i=0; i<args.Length; ++i) {
			var arg = args[i];

			if (arg == "-w") {
				cfg.Watch = true;
			} else if (arg == "-h" || arg == "--help") {
				printHelp();
				return;
			} else if (arg.StartsWith('-')) {
				Console.WriteLine($"Unknown option \"{arg}\"");
				printHelp();
				return;
			} else {
				if (positionalArgsIx == 0) {
					++positionalArgsIx;
					cfg.RelSrcPath = arg;
				} else if (positionalArgsIx == 1) {
					++positionalArgsIx;
					cfg.RelDstPath = arg;
				} else if (positionalArgsIx == 2) {
					++positionalArgsIx;
					cfg.ContentPath = arg;
				} else {
					Console.WriteLine("Too many positional arguments");
					printHelp();
					return;
				}
			}
		}

		if (positionalArgsIx != 3) {
			Console.WriteLine("Missing positional arguments");
			printHelp();
			return;
		}

		if (!Directory.Exists(cfg.ContentPath)) {
			Console.WriteLine(
				$"The specified path \"{cfg.ContentPath}\" does not exist. Please make sure the path is correct and try again.");
			return;
		}

		var app = new Application(cfg);
		app.Run();
	}

	internal static void printHelp() {
		Console.WriteLine("Usage: ValveTypeScriptConverter.exe <options> <srcPath> <dstPath> <addonPath>");
		Console.WriteLine("  addonPath: The path to the root of the addons content directory");
		Console.WriteLine("    Example: \"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Counter-Strike Global Offensive\\content\\csgo_addons\\my_addon\"");
		Console.WriteLine("  srcPath: Relative path to the source directory");
		Console.WriteLine("    Example: \"my_module\\src\"");
		Console.WriteLine("  dstPath: Relative path to the destination directory");
		Console.WriteLine("      Example: \"my_module\"");
		Console.WriteLine("  Options:");
		Console.WriteLine("    -h: Display this help message");
		Console.WriteLine("    -w: Watch for changes in the ts files and compile them automatically");
	}

	internal static string BuildNo() {
		return "1";
	}
}

internal class Application(Application.Config cfg)
{
	private readonly Config cfg = cfg;
	private readonly Dictionary<string /*fPath*/, FileSystemWatcher> watchers = [];

	#region Data types

	internal class Config
	{
		public required string ContentPath;
		public required string RelSrcPath;
		public required string RelDstPath;

		public bool Watch;

		public string GamePath => this.ContentPath.Replace(
			Path.Combine("content", "csgo_addons"),
			Path.Combine("game", "csgo_addons"));
		public string ContentVscriptsPath => Path.Combine(
			this.ContentPath,
			Path.Combine("scripts", "vscripts"));
		public string GameVscriptsPath => Path.Combine(
			this.GamePath,
			Path.Combine("scripts", "vscripts"));
		public string SrcPath => Path.Combine(
			this.ContentVscriptsPath,
			this.RelSrcPath);
		public string DstPath => Path.Combine(
			this.GameVscriptsPath,
			this.RelDstPath);
	}

	#endregion // Data types

	#region Support functions

	private static void log(string message) {
		Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
	}

	private string[] getTSFiles() {
		return Directory.GetFiles(
				this.cfg.SrcPath,
				"*.ts",
				SearchOption.AllDirectories)
			.Where(f =>
				!f.EndsWith(".d.ts") &&
				!f.EndsWith(".test.ts") &&
				!f.EndsWith(".spec.ts"))
			.ToArray();
	}

	private string getDstFPath(string fPath) {
		var relativeFPath = Path.GetRelativePath(this.cfg.SrcPath, fPath);
		return Path.ChangeExtension(
			Path.Combine(this.cfg.DstPath, relativeFPath),
			"vts_c");
	}

	private void compileFile(string fPath) {
		var relativeFPath = Path.GetRelativePath(this.cfg.SrcPath, fPath);
		var dstFPath = this.getDstFPath(fPath);

		var dPath = Path.GetDirectoryName(dstFPath);
		if (dPath != null) {
			if (!Directory.Exists(dPath)) {
				Directory.CreateDirectory(dPath);
			}

			try {
				var cs2ts = new CS2TypeScript(fPath);
				cs2ts.Save(dstFPath);
				log($"Successfully compiled \"{relativeFPath}\"");
			} catch (Exception e) {
				log($"Failed to compile \"{relativeFPath}\" - {e.Message}");
			}
		}
	}

	private FileSystemWatcher? watchFile(string fPath) {
		var dirNam = Path.GetDirectoryName(fPath);
		var localFPath = Path.GetRelativePath(this.cfg.SrcPath, fPath);

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

		this.compileFile(fPath);

		log($"Tracking changes to \"{localFPath}\"");

		return watcher;
	}

	#endregion // Support functions

	internal void Run() {
		log("Starting up ...");
		log($"Content path: \"{this.cfg.ContentPath}\"");
		log($"Game path: \"{this.cfg.GamePath}\"");

		// Clear the destination directory
		if (Directory.Exists(this.cfg.DstPath)) {
			Directory.Delete(this.cfg.DstPath, true);
		}

		if (this.cfg.Watch) {
			this.runWatcher();
		} else {
			foreach (var fPath in this.getTSFiles()) {
				this.compileFile(fPath);
			}
		}
	}

	private void runWatcher() {
		log($"Watching for file changes in \"{this.cfg.SrcPath}\" ...");

		while (true) {
			foreach (var fPath in this.getTSFiles()) {
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
}
