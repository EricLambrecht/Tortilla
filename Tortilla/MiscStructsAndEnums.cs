using System;

namespace Makhani.Tortilla
{
	public enum StreamingMode {
		UDP,
		RTP
	}

	public enum AudioCodec {
		LameMP3,
		WMA
	}

	public enum VideoCodec {
		Mpeg4,
		x264
	}

	/// <summary>
	/// A screen resolution consisting of height and width.
	/// </summary>
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

