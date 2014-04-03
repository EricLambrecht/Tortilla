using System;

namespace Makhani.Tortilla
{
	public class OutputReceivedEventArgs
	{
		/// <summary>
		/// Gets or sets the name of the application that has send the outputted.
		/// </summary>
		/// <value>The name of the application.</value>
		public string ApplicationName { get; set; } 
		/// <summary>
		/// Gets or sets the line that has been output.
		/// </summary>
		/// <value>The line.</value>
		public string Line { get; set; } 

		/// <summary>
		/// Initializes a new instance of the <see cref="Makhani.Tortilla.OutputReceivedEventArgs"/> class.
		/// </summary>
		/// <param name="line">Line that has been outputted.</param>
		/// <param name="name">The name of the application that generated this output.</param>
		public OutputReceivedEventArgs (string line, string name)
		{
			ApplicationName = name;
			Line = line;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Makhani.Tortilla.OutputReceivedEventArgs"/> class.
		/// The application name will be set to "Unknown".
		/// </summary>
		/// <param name="line">Line that has been outputted.</param>
		public OutputReceivedEventArgs (string line)
		{
			ApplicationName = "Unknown";
			Line = line;		
		}

		/// <summary>
		/// Generates a string of the form "Application Name: line".
		/// </summary>
		/// <returns>A string of the form "Application Name: line".</returns>
		/// <filterpriority>2</filterpriority>
		public override string ToString ()
		{
			return string.Format ("{0}: {1}", ApplicationName, Line);
		}
	}
}

