using System;
using System.IO;

namespace Makhani
{
	public static class Utilities
	{
		public enum OS
		{
			Win,
			Linux,
			Mac
		}

		public static OS GetOS()
		{
			switch (Environment.OSVersion.Platform)
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

