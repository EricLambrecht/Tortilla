using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Makhani.Tortilla
{
	public class FFmpegProcess
	{
		public List<string> Output { get; private set; }
		protected Process process;

		public FFmpegProcess ()
		{
		}

		public void Start(string arguments) 
		{
			process = new Process();
			// Configure the process using the StartInfo properties.
			process.StartInfo.FileName = "ffmpeg";
			process.StartInfo.Arguments = arguments;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.OutputDataReceived += OnDataReceived;
			process.Start();
			process.WaitForExit ();// Waits here for the process to exit.
		}

		public void Run(string arguments) {
			Start (arguments);
			// TODO: Dont restart process every time, instead give new arguments..
		}

		public void CaptureLinuxScreen () 
		{
			string args = "-video_size 1024x768 -framerate 25 -f x11grab -i :0.0+100,200 -f alsa -ac 2 -i pulse output.flv";
			Run (args); 
		}

		public void CaptureWindowsScreen () 
		{
			string args = "-f dshow -i video=\"UScreenCapture\":audio=\"Stereo Mix\" output.flv";
			Run (args); 
		}

		public void StreamVideoToUrl(string url, Resolution inputResolution, Resolution outputResolution) 
		{
			// ffmpeg -f dshow -i video="screen-capture-recorder":audio="Stereo Mix (IDT High Definition)" -vcodec libx264 -preset ultrafast -tune zerolatency -r 10 -async 1 -acodec libmp3lame -ab 24k -ar 22050 -bsf:v h264_mp4toannexb -maxrate 750k -bufsize 3000k -f mpegts udp://192.168.5.215:48550

			string args = 
				string.Format(
					"-f x11grab -s {0} -r 15 -i :0.0 -c:v libx264 -preset fast -pix_fmt yuv420p -s {1} -threads 0 -f flv \"{2}\"", 
					inputResolution,
					outputResolution,
					url
				);
			Run(args);
		}

		public void StreamAudioViaRTP(string url) {
			string args = "-re -f lavfi -i aevalsrc=\"sin(400*2*PI*t)\" -ar 8000 -f mulaw -f rtp rtp://127.0.0.1:1234";
			Run (args);
		}
			
		// This function is windows only!
		public void ListAudioDevices() 
		{
			//ffmpeg -list_devices true -f dshow -i dummy
			string args = "-list_devices true -f dshow -i dummy";
			Run (args);
		}

		protected void OnDataReceived(object sender, DataReceivedEventArgs received) 
		{
			Output.Add (received.Data);
		}

		public void End() 
		{
			process.Dispose ();
			process.Kill ();
		}
	}

	public struct Resolution {
		// In Pixels
		int Width;
		int Height;

		public Resolution(int width, int height) {
			this.Width = width;
			this.Height = height;
		}

		public override string ToString() {
			return string.Format ("{0}x{1}", Width, Height);
		}
	}
}

