using System;
using System.IO;
using System.Reflection;

namespace Makhani
{
	public static class Environment
	{
		/// <summary>
		/// Path to the application that runs this assembly.
		/// </summary>
		public static string ApplicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

		public enum OS
		{
			Win,
			Linux,
			Mac
		}

		/// <summary>
		/// Gets the current platform.
		/// </summary>
		/// <returns>The platform that this assembly is running on.</returns>
		public static OS GetOS()
		{
			switch (System.Environment.OSVersion.Platform)
			{
			case PlatformID.Unix:
				// Well, there are chances MacOSX is reported as Unix instead of MacOSX.
				// Instead of platform check, we'll do a feature checks (Mac specific root folders)
				if (Directory.Exists("/Applications")
					& Directory.Exists("/System")
					& Directory.Exists("/Users")
					& Directory.Exists("/Volumes"))
					return OS.Mac;
				else
					return OS.Linux;

			case PlatformID.MacOSX:
				return OS.Mac;

			default:
				return OS.Win;
			}
		}
	}
}

