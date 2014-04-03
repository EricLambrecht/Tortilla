﻿using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Makhani.Tortilla
{
	public class Tortilla
	{
		public List<string> Output { get; private set; }
		public List<string> AudioDevices { get; private set; }
		public List<string> VideoDevices { get; private set; }
		public Process FFmpegProcess{ get; private set; }
		public event EventHandler<OutputReceivedEventArgs> OutputReceived;



		public Tortilla ()
		{
			AudioDevices = new List<string> ();
			VideoDevices = new List<string> ();
			Output = new List<string> ();
			UpdateDevices ();
		}

		public void CaptureLinuxScreen (Resolution inputResolution, int frameRate) 
		{
			string args = string.Format("-video_size {0} -framerate {1} -f x11grab -i :0.0+100,200 -f alsa -ac 2 -i pulse output.flv", inputResolution, frameRate.ToString());
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args);
			FFmpegProcess.ErrorDataReceived += OnDataReceived;
		}

		public void CaptureWindowsScreen (string audioDeviceName) 
		{
			string args = string.Format("-f dshow -i video=\"UScreenCapture\":audio=\"{0}\" output.flv", audioDeviceName);
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args);
			FFmpegProcess.ErrorDataReceived += OnDataReceived;
		}

		public async Task<bool> StreamWindowsScreenToIpAsync (string videoDeviceName, string audioDeviceName, string ip, StreamingMode mode, int frameRate = 25, int quality = 20) 
		{
			// var tcs = new TaskCompletionSource<bool> ();

			string input = string.Format(
				"-f dshow  -i video=\"{0}\":audio=\"{1}\" -r {2} -vcodec mpeg4 -q {3} -acodec libmp3lame -ab 128k",
				videoDeviceName, audioDeviceName, frameRate.ToString(), quality.ToString()
			);
			string output = string.Format ("-f mpegts udp://{0}:6666?pkt_size=188?buffer_size=65535", ip);

			string args = input + " " + output;

			FFmpegProcess = FFmpegManager.GetFFmpegProcess(args);
			FFmpegProcess.ErrorDataReceived += OnDataReceived;
//			FFmpegProcess.Exited += (sender, e) => {
//				try { tcs.SetResult(true); }
//				catch(Exception exc) { tcs.SetException(exc); }
//				finally { FFmpegProcess.Dispose(); }
//			};

			FFmpegProcess.Start ();
			await Task.Run(() => FFmpegProcess.WaitForExit ());
			return true;

			//return tcs.Task;
		}

		public void StreamVideoToUrl(string url, Resolution inputResolution, Resolution outputResolution) 
		{
			// -preset ultrafast -tune zerolatency TODO: Checken!
			string args = 
				string.Format(
					"-f x11grab -s {0} -r 15 -i :0.0 -c:v libx264 -preset fast -pix_fmt yuv420p -s {1} -threads 0 -f flv \"{2}\"", 
					inputResolution, outputResolution, url
				);
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args);
			FFmpegProcess.ErrorDataReceived += OnDataReceived;
		}

		public void StreamAudioViaRTP(string url)
		{
			string args = "-re -f lavfi -i aevalsrc=\"sin(400*2*PI*t)\" -ar 8000 -f mulaw -f rtp rtp://127.0.0.1:1234";
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args);
			FFmpegProcess.ErrorDataReceived += OnDataReceived;

			// TODO: get stream with "ffplay rtp://127.0.0.1:1234"
		}

		public async Task<bool> LogFFmpegOutput() 
		{
			string line;
			var outputStream = FFmpegProcess.StandardError;
			using (var fileStream = File.Create(Makhani.Environment.ApplicationPath + "\\output.log")) {
				using (var fileWriter = new StreamWriter (fileStream)) {
					while ((line = await outputStream.ReadLineAsync()) != null) {
						fileWriter.WriteLine (line);
						OnOutputReceived (line);
					}
				}
			}
			return true;
		}
			
		// This function is windows only!
		protected void UpdateDevices() 
		{
			//ffmpeg -list_devices true -f dshow -i dummy
			string args = "-list_devices true -f dshow -i dummy";
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args);
			FFmpegProcess.ErrorDataReceived += OnDataReceived;
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

		public void SendInputToFFmpegProcess(char ch) {
			FFmpegProcess.StandardInput.Write (ch);
		}

		public void SendInputToFFmpegProcess(string str) {
			FFmpegProcess.StandardInput.WriteLine(str);
		}

		protected void OnOutputReceived(string line) {
			Output.Add (line);
			EventHandler<OutputReceivedEventArgs> tmpHandler = OutputReceived;
			if (tmpHandler != null) {
				tmpHandler (this, new OutputReceivedEventArgs(line));
			}
		}

		protected void OnDataReceived(object sender, DataReceivedEventArgs received) 
		{
			OnOutputReceived (received.Data);
		}

		public void Kill() 
		{
			FFmpegProcess.Dispose ();
			FFmpegProcess.Kill ();
		}
			
	}
}

