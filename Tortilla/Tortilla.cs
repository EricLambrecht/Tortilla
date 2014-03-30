using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Makhani.Tortilla
{
	public class Tortilla
	{
		public List<string> Output { get; private set; }
		public List<string> AudioDevices { get; private set; }
		public List<string> VideoDevices { get; private set; }
		public Process FFmpegProcess{ get; private set; }

		public Tortilla ()
		{
			AudioDevices = new List<string> ();
			VideoDevices = new List<string> ();
			Output = new List<string> ();
			UpdateDevices ();
		}

		public void CaptureLinuxScreen () 
		{
			string args = "-video_size 1024x768 -framerate 25 -f x11grab -i :0.0+100,200 -f alsa -ac 2 -i pulse output.flv";
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args, OnDataReceived);
		}

		public void CaptureWindowsScreen (string audioDeviceName) 
		{
			string args = string.Format("-f dshow -i video=\"UScreenCapture\":audio=\"{0}\" output.flv", audioDeviceName);
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args, OnDataReceived);
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
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args, OnDataReceived);
		}

		public void StreamAudioViaRTP(string url)
		{
			string args = "-re -f lavfi -i aevalsrc=\"sin(400*2*PI*t)\" -ar 8000 -f mulaw -f rtp rtp://127.0.0.1:1234";
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args, OnDataReceived);

			// TODO: get stream with "ffplay rtp://127.0.0.1:1234"
		}
			
		// This function is windows only!
		protected void UpdateDevices() 
		{
			//ffmpeg -list_devices true -f dshow -i dummy
			string args = "-list_devices true -f dshow -i dummy";
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args, OnDataReceived);
			var outputStream = FFmpegProcess.StandardError;

			using (var fileStream = File.Create(Makhani.Environment.ApplicationPath + "\\devices.log")) {
				using (var fileWriter = new StreamWriter (fileStream)) {

					bool audioNextLine = false;
					bool videoNextLine = false;
					while (!outputStream.EndOfStream) {
						string line = outputStream.ReadLine ();
						string device = "";

						int deviceStart = line.IndexOf ('"');
						if (deviceStart != -1) {
							deviceStart += 1;
							int deviceEnd = line.LastIndexOf ('"');
							device = line.Substring (deviceStart, deviceEnd - deviceStart);
						}

						if (device != "") {
							if (videoNextLine) {
								VideoDevices.Add (device);
							} else if (audioNextLine) {
								AudioDevices.Add (device);
							}
						}

						if (line.Contains ("DirectShow audio devices")) {
							audioNextLine = true;
							videoNextLine = false;
						}
						if (line.Contains ("DirectShow video devices")) {
							audioNextLine = false;
							videoNextLine = true;
						}

						fileWriter.WriteLine (line);
					}
				}
			}
		}

		protected void OnDataReceived(object sender, DataReceivedEventArgs received) 
		{
			Output.Add (received.Data);
		}

		public void KillProcess(Process process) 
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

