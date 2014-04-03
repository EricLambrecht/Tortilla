using System;

namespace Makhani.Tortilla
{
	public class OutputReceivedEventArgs
	{
		public string ApplicationName { get; set; } 
		public string Line { get; set; } 

		public OutputReceivedEventArgs (string line, string name)
		{
			ApplicationName = name;
			Line = line;
		}

		public OutputReceivedEventArgs (string line)
		{
			ApplicationName = "Unknown";
			Line = line;		
		}

		public override string ToString ()
		{
			return string.Format ("{0}: {1}", ApplicationName, Line);
		}
	}
}

