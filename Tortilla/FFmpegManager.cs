using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;


namespace Makhani.Tortilla
{
	public static class FFmpegManager
	{
		public static string FFmpegDefaultPath = Path.Combine(Makhani.Environment.ApplicationPath, "ffmpeg\\bin");

		public static Process RunFFmpegProcess (string arguments) {
			return RunFFmpegProcess (arguments, FFmpegDefaultPath);
		}

		public static Process RunFFmpegProcess (string arguments, string path)
		{
			try {
				var process = GetFFmpegProcess(arguments, path);
				process.Start();
				process.WaitForExit();
				return process;
			}
			catch(FileNotFoundException e) {
				throw new FileNotFoundException (e.Message + " " + path, e);
			}
			catch(System.ComponentModel.Win32Exception e) {
				throw new FileNotFoundException (e.Message + " " + path, e);
			}
		}

		public static Process GetFFmpegProcess (string arguments) {
			return GetFFmpegProcess (arguments, FFmpegDefaultPath);
		}

		public static Process GetFFmpegProcess (string arguments, string path)
		{
			try {
				Process process = new Process();
				// Configure the process using the StartInfo properties.
				process.StartInfo.FileName = Path.Combine(FFmpegDefaultPath, "ffmpeg.exe");
				process.StartInfo.Arguments = arguments;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.WorkingDirectory = path;
				process.EnableRaisingEvents = true;
				return process;
			}
			catch(FileNotFoundException e) {
				throw new FileNotFoundException (e.Message + " " + path, e);
			}
			catch(System.ComponentModel.Win32Exception e) {
				throw new FileNotFoundException (e.Message + " " + path, e);
			}
		}

		public static bool IsInstalled() {
			switch (Makhani.Environment.GetOS ()) {
			case Makhani.Environment.OS.Win:
				string path = Path.Combine (FFmpegDefaultPath, "ffmpeg.exe");
				if (File.Exists (path)) {
					return true;
				} else {
					return false;
				}
				break;
			case Makhani.Environment.OS.Linux:
			case Makhani.Environment.OS.Mac:
			default:
				return false;
				break;
			}
		}

	}
}

