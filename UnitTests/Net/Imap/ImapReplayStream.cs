//
// ImapReplayStream.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using NUnit.Framework;

using MimeKit.IO;
using MimeKit.IO.Filters;

namespace UnitTests.Net.Imap {
	enum ImapReplayCommandResponse {
		OK,
		NO,
		BAD
	}

	class ImapReplayCommand
	{
		public string Command { get; private set; }
		public byte[] Response { get; private set; }

		public ImapReplayCommand (string command, byte[] response)
		{
			Command = command;
			Response = response;
		}

		public ImapReplayCommand (string command, string resource)
		{
			Command = command;

			using (var stream = GetType ().Assembly.GetManifestResourceStream ("UnitTests.Net.Imap.Resources." + resource)) {
				var memory = new MemoryBlockStream ();

				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());
					stream.CopyTo (filtered, 4096);
				}

				Response = memory.ToArray ();
			}
		}

		public ImapReplayCommand (string command, ImapReplayCommandResponse response)
		{
			var tokens = command.Split (' ');
			var cmd = (tokens[1] == "UID" ? tokens[2] : tokens[1]).TrimEnd ();
			var tag = tokens[0];

			var text = string.Format ("{0} {1} {2} completed\r\n", tag, response, cmd);
			Response = Encoding.ASCII.GetBytes (text);
			Command = command;
		}
	}

	enum ImapReplayState {
		SendResponse,
		WaitForCommand,
	}

	class ImapReplayStream : Stream
	{
		static readonly Encoding Latin1 = Encoding.GetEncoding (28591);
		readonly MemoryStream sent = new MemoryStream ();
		readonly IList<ImapReplayCommand> commands;
		readonly bool testUnixFormat;
		ImapReplayState state;
		int timeout = 100000;
		Stream stream;
		bool disposed;
		int index;

		public ImapReplayStream (IList<ImapReplayCommand> commands, bool testUnixFormat)
		{
			stream = GetResponseStream (commands[0]);
			state = ImapReplayState.SendResponse;
			this.testUnixFormat = testUnixFormat;
			this.commands = commands;
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException ("ImapReplayStream");
		}

		#region implemented abstract members of Stream

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
			get { return true; }
		}

		public override bool CanWrite {
			get { return true; }
		}

		public override bool CanTimeout {
			get { return true; }
		}

		public override long Length {
			get { return stream.Length; }
		}

		public override long Position {
			get { return stream.Position; }
			set { throw new NotSupportedException (); }
		}

		public override int ReadTimeout {
			get { return timeout; }
			set { timeout = value; }
		}

		public override int WriteTimeout {
			get { return timeout; }
			set { timeout = value; }
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();

			if (state != ImapReplayState.SendResponse) {
				var command = Latin1.GetString (sent.GetBuffer (), 0, (int) sent.Length);

				Assert.AreEqual (ImapReplayState.SendResponse, state, "Trying to read before command received. Sent so far: {0}", command);
			}
			Assert.IsNotNull (stream, "Trying to read when no data available.");

			int nread = stream.Read (buffer, offset, count);

			if (stream.Position == stream.Length) {
				state = ImapReplayState.WaitForCommand;
				index++;
			}

			return nread;
		}

		Stream GetResponseStream (ImapReplayCommand command)
		{
			MemoryStream memory;

			if (testUnixFormat) {
				memory = new MemoryStream ();

				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Dos2UnixFilter ());
					filtered.Write (command.Response, 0, command.Response.Length);
					filtered.Flush ();
				}

				memory.Position = 0;
			} else {
				memory = new MemoryStream (command.Response, false);
			}

			return memory;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();

			Assert.AreEqual (ImapReplayState.WaitForCommand, state, "Trying to write when a command has already been given.");

			sent.Write (buffer, offset, count);

			if (sent.Length >= commands[index].Command.Length) {
				var command = Latin1.GetString (sent.GetBuffer (), 0, (int) sent.Length);

				Assert.AreEqual (commands[index].Command, command, "Commands did not match.");

				if (stream != null)
					stream.Dispose ();

				stream = GetResponseStream (commands[index]);
				state = ImapReplayState.SendResponse;
				sent.SetLength (0);
			}
		}

		public override void Flush ()
		{
			CheckDisposed ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		#endregion

		protected override void Dispose (bool disposing)
		{
			if (stream != null)
				stream.Dispose ();

			base.Dispose (disposing);
			disposed = true;
		}
	}
}
