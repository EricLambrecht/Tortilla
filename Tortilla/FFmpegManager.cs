using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;


namespace Makhani.Tortilla
{
	public static class FFmpegManager
	{
		/// <summary>
		/// The FFmpeg default path. By default this path is relative to the application's executable path, regardless of whether the assembly's location is a different one. 
		/// </summary>
		public static string FFmpegDefaultPath = Path.Combine(Makhani.Environment.ApplicationPath, "ffmpeg\\bin");

		/// <summary>
		/// Runs a FFmpeg process. Uses default path.
		/// </summary>
		/// <returns>The FFmpeg process.</returns>
		/// <param name="arguments">Starting arguments.</param>
		public static Process RunFFmpegProcess (string arguments) {
			return RunFFmpegProcess (arguments, FFmpegDefaultPath);
		}

		/// <summary>
		/// Runs a FFmpeg process.
		/// </summary>
		/// <returns>The FFmpeg process.</returns>
		/// <param name="arguments">Starting arguments.</param>
		/// <param name="path">Path to FFmpeg.</param>
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

		/// <summary>
		/// Gets a unstarted FFmpeg process. Uses default path.
		/// </summary>
		/// <returns>The FFmpeg process.</returns>
		/// <param name="arguments">Starting arguments.</param>
		public static Process GetFFmpegProcess (string arguments) {
			return GetFFmpegProcess (arguments, FFmpegDefaultPath);
		}

		/// <summary>
		/// Gets a unstarted FFmpeg process.
		/// </summary>
		/// <returns>The FFmpeg process.</returns>
		/// <param name="arguments">Starting arguments.</param>
		/// <param name="path">Path to FFmpeg.</param>
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

		/// <summary>
		/// Determines if FFmpeg is installed on the current system.
		/// </summary>
		/// <returns><c>true</c> if it's installed; otherwise, <c>false</c>.</returns>
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

