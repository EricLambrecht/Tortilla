using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Makhani.Tortilla
{
	public class Tortilla
	{
		/// <summary>
		/// Stores all output from this tortillas processes.
		/// </summary>
		/// <value>All received lines of console output.</value>
		public List<string> Output { get; private set; }
		/// <summary>
		/// Gets all available audio devices on the current system.
		/// </summary>
		/// <value>A list of all available audio devices.</value>
		public List<string> AudioDevices { get; private set; }
		/// <summary>
		/// Gets all available video devices on the current system.
		/// </summary>
		/// <value>A list of all available video devices.</value>
		public List<string> VideoDevices { get; private set; }
		/// <summary>
		/// Gets the FFmpeg process (if ffmpeg is running).
		/// </summary>
		/// <value>The F fmpeg process.</value>
		public Process FFmpegProcess{ get; private set; }
		/// <summary>
		/// Occurs when console output is received.
		/// </summary>
		public event EventHandler<OutputReceivedEventArgs> OutputReceived;

		public Tortilla ()
		{
			AudioDevices = new List<string> ();
			VideoDevices = new List<string> ();
			Output = new List<string> ();
		}

		/// <summary>
		/// Captures the screen on linux systems.
		/// </summary>
		/// <param name="inputResolution">Input screen resolution.</param>
		/// <param name="frameRate">Desired frame rate.</param>
		public void CaptureLinuxScreen (Resolution inputResolution, int frameRate) 
		{
			string args = string.Format("-video_size {0} -framerate {1} -f x11grab -i :0.0+100,200 -f alsa -ac 2 -i pulse output.flv", inputResolution, frameRate.ToString());
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args);
			FFmpegProcess.ErrorDataReceived += OnDataReceived;
		}

		/// <summary>
		/// Captures the screen on windows systems.
		/// </summary>
		/// <param name="audioDeviceName">Audio device name.</param>
		public void CaptureWindowsScreen (string audioDeviceName) 
		{
			string args = string.Format("-f dshow -i video=\"UScreenCapture\":audio=\"{0}\" output.flv", audioDeviceName);
			FFmpegProcess = FFmpegManager.RunFFmpegProcess(args);
			FFmpegProcess.ErrorDataReceived += OnDataReceived;
		}

		/// <summary>
		/// Asynchronously streams a windows screen and audio capture to a specified IP-Address.
		/// </summary>
		/// <returns>The windows screen to ip async.</returns>
		/// <param name="videoDeviceName">Video device name.</param>
		/// <param name="audioDeviceName">Audio device name.</param>
		/// <param name="ip">IP-Address.</param>
		/// <param name="mode">Streaming mode.</param>
		/// <param name="frameRate">Desired frame rate.</param>
		/// <param name="quality">Quality of compression.</param>
		public async Task<bool> StreamWindowsScreenToIpAsync (string videoDeviceName, string audioDeviceName, string ip, string port, VideoCodec vcodec, AudioCodec acodec, StreamingMode mode, int frameRate, Resolution outputSize, string videoExtras, int quality = 20) 
		{
			// TODO: -b for bitrate
			string input = string.Format (
				"-f dshow  -i video=\"{0}\":audio=\"{1}\" -r {2} -async 1 -vcodec {3} {4} -q {5} -s {6} -maxrate 750k -bufsize 3000k -acodec {7} -ab 128k",
				videoDeviceName, 
				audioDeviceName, 
				frameRate.ToString(), 
				FFmpegManager.GetCodecName(vcodec), 
				videoExtras,
				quality.ToString(),
				outputSize,
				FFmpegManager.GetCodecName(acodec)		
			);
			string output = string.Format (
				"-f mpegts udp://{0}:{1}?pkt_size=188?buffer_size=10000000?fifo_size=100000", 
				ip, 
				port
			);

			string args = input + " " + output;

			try {
				FFmpegProcess = FFmpegManager.GetFFmpegProcess(args);
			}
			catch(FileNotFoundException e) {
				throw new FileNotFoundException (e.Message, e);
			}

			FFmpegProcess.Start ();
			await Task.Run(() => FFmpegProcess.WaitForExit ());
			return true;
		}

		/// <summary>
		/// Streams the video to URL.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="inputResolution">Input resolution.</param>
		/// <param name="outputResolution">Output resolution.</param>
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

		/// <summary>
		/// Asynchronously reads FFmpeg console output.
		/// </summary>
		/// <returns>The F fmpeg output.</returns>
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
			
		/// <summary>
		/// This function is Windows-only!
		/// Updates video and audio devices.
		/// </summary>
		public void UpdateDevices() 
		{
			//ffmpeg -list_devices true -f dshow -i dummy
			string args = "-list_devices true -f dshow -i dummy";
			try {
				FFmpegProcess = FFmpegManager.RunFFmpegProcess(args);
			}
			catch(FileNotFoundException e) {
				throw new FileNotFoundException (e.Message, e); // TODO: Write more specific custom exception
			}

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

//		public string GetVideoCodecParams(VideoCodec codec, int frameRate, int quality, string preset, string extra) {
//			string codecName = FFmpegManager.GetCodecName (codec);
//			/return string.Format("-vcodec {0} -preset {1} -tune zerolatency -r {3}", codecName);
//		}

		/// <summary>
		/// Sends the input to FFmpeg-process.
		/// </summary>
		/// <param name="ch">A char that will be send as input.</param>
		public void SendInputToFFmpegProcess(char ch) {
			FFmpegProcess.StandardInput.Write (ch);
		}
		/// <summary>
		/// Sends the input to FFmpeg-process.
		/// </summary>
		/// <param name="str">A string that will be send as input.</param>
		public void SendInputToFFmpegProcess(string str) {
			FFmpegProcess.StandardInput.WriteLine(str);
		}

		/// <summary>
		/// Raises the output received event.
		/// </summary>
		/// <param name="line">Received line of console output.</param>
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

		/// <summary>
		/// Tries to kill all tortilla encoder processes.
		/// </summary>
		public void Kill() 
		{
			try {
				FFmpegProcess.Dispose ();
				FFmpegProcess.Kill ();
			}
			catch (Exception e) {
				throw new Exception (e.Message, e);
			}
		}
			
	}
}

