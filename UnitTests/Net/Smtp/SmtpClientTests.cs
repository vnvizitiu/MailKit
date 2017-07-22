//
// SmtpClientTests.cs
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
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;

using NUnit.Framework;

using MailKit.Net.Smtp;
using MailKit.Security;
using MailKit;

using MimeKit.IO;
using MimeKit;

namespace UnitTests.Net.Smtp {
	[TestFixture]
	public class SmtpClientTests
	{
		MimeMessage CreateSimpleMessage ()
		{
			var message = new MimeMessage ();
			message.From.Add (new MailboxAddress ("Sender Name", "sender@example.com"));
			message.To.Add (new MailboxAddress ("Recipient Name", "recipient@example.com"));
			message.Subject = "This is a test...";

			message.Body = new TextPart ("plain") {
				Text = "This is the message body."
			};

			return message;
		}

		MimeMessage CreateBinaryMessage ()
		{
			var message = new MimeMessage ();
			message.From.Add (new MailboxAddress ("Sender Name", "sender@example.com"));
			message.To.Add (new MailboxAddress ("Recipient Name", "recipient@example.com"));
			message.Subject = "This is a test...";

			message.Body = new TextPart ("plain") {
				Text = "This is the message body with some unicode unicode: ☮ ☯",
				ContentTransferEncoding = ContentEncoding.Binary
			};

			return message;
		}

		MimeMessage CreateEightBitMessage ()
		{
			var message = new MimeMessage ();
			message.From.Add (new MailboxAddress ("Sender Name", "sender@example.com"));
			message.To.Add (new MailboxAddress ("Recipient Name", "recipient@example.com"));
			message.Subject = "This is a test...";

			message.Body = new TextPart ("plain") {
				Text = "This is the message body with some unicode unicode: ☮ ☯"
			};

			return message;
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			using (var client = new SmtpClient ()) {
				var credentials = new NetworkCredential ("username", "password");
				var socket = new Socket (SocketType.Stream, ProtocolType.Tcp);
				var message = CreateSimpleMessage ();
				var sender = message.From.Mailboxes.FirstOrDefault ();
				var recipients = message.To.Mailboxes.ToList ();
				var options = FormatOptions.Default;
				var empty = new MailboxAddress[0];

				// Connect
				Assert.Throws<ArgumentNullException> (() => client.Connect ((Uri) null));
				Assert.Throws<ArgumentNullException> (async () => await client.ConnectAsync ((Uri) null));
				Assert.Throws<ArgumentNullException> (() => client.Connect (null, 25, false));
				Assert.Throws<ArgumentNullException> (async () => await client.ConnectAsync (null, 25, false));
				Assert.Throws<ArgumentException> (() => client.Connect (string.Empty, 25, false));
				Assert.Throws<ArgumentException> (async () => await client.ConnectAsync (string.Empty, 25, false));
				Assert.Throws<ArgumentOutOfRangeException> (() => client.Connect ("host", -1, false));
				Assert.Throws<ArgumentOutOfRangeException> (async () => await client.ConnectAsync ("host", -1, false));
				Assert.Throws<ArgumentNullException> (() => client.Connect (null, 25, SecureSocketOptions.None));
				Assert.Throws<ArgumentNullException> (async () => await client.ConnectAsync (null, 25, SecureSocketOptions.None));
				Assert.Throws<ArgumentException> (() => client.Connect (string.Empty, 25, SecureSocketOptions.None));
				Assert.Throws<ArgumentException> (async () => await client.ConnectAsync (string.Empty, 25, SecureSocketOptions.None));
				Assert.Throws<ArgumentOutOfRangeException> (() => client.Connect ("host", -1, SecureSocketOptions.None));
				Assert.Throws<ArgumentOutOfRangeException> (async () => await client.ConnectAsync ("host", -1, SecureSocketOptions.None));

				Assert.Throws<ArgumentNullException> (() => client.Connect (null, "host", 25, SecureSocketOptions.None));
				Assert.Throws<ArgumentException> (() => client.Connect (socket, "host", 25, SecureSocketOptions.None));

				// Authenticate
				Assert.Throws<ArgumentNullException> (() => client.Authenticate (null));
				Assert.Throws<ArgumentNullException> (async () => await client.AuthenticateAsync (null));
				Assert.Throws<ArgumentNullException> (() => client.Authenticate (null, "password"));
				Assert.Throws<ArgumentNullException> (async () => await client.AuthenticateAsync (null, "password"));
				Assert.Throws<ArgumentNullException> (() => client.Authenticate ("username", null));
				Assert.Throws<ArgumentNullException> (async () => await client.AuthenticateAsync ("username", null));
				Assert.Throws<ArgumentNullException> (() => client.Authenticate (null, credentials));
				Assert.Throws<ArgumentNullException> (async () => await client.AuthenticateAsync (null, credentials));
				Assert.Throws<ArgumentNullException> (() => client.Authenticate (Encoding.UTF8, null));
				Assert.Throws<ArgumentNullException> (async () => await client.AuthenticateAsync (Encoding.UTF8, null));
				Assert.Throws<ArgumentNullException> (() => client.Authenticate (null, "username", "password"));
				Assert.Throws<ArgumentNullException> (async () => await client.AuthenticateAsync (null, "username", "password"));
				Assert.Throws<ArgumentNullException> (() => client.Authenticate (Encoding.UTF8, null, "password"));
				Assert.Throws<ArgumentNullException> (async () => await client.AuthenticateAsync (Encoding.UTF8, null, "password"));
				Assert.Throws<ArgumentNullException> (() => client.Authenticate (Encoding.UTF8, "username", null));
				Assert.Throws<ArgumentNullException> (async () => await client.AuthenticateAsync (Encoding.UTF8, "username", null));

				// Send
				Assert.Throws<ArgumentNullException> (() => client.Send (null));

				Assert.Throws<ArgumentNullException> (() => client.Send (null, message));
				Assert.Throws<ArgumentNullException> (() => client.Send (options, null));

				Assert.Throws<ArgumentNullException> (() => client.Send (message, null, recipients));
				Assert.Throws<ArgumentNullException> (() => client.Send (message, sender, null));
				Assert.Throws<InvalidOperationException> (() => client.Send (message, sender, empty));

				Assert.Throws<ArgumentNullException> (() => client.Send (null, message, sender, recipients));
				Assert.Throws<ArgumentNullException> (() => client.Send (options, null, sender, recipients));
				Assert.Throws<ArgumentNullException> (() => client.Send (options, message, null, recipients));
				Assert.Throws<ArgumentNullException> (() => client.Send (options, message, sender, null));
				Assert.Throws<InvalidOperationException> (() => client.Send (options, message, sender, empty));

				Assert.Throws<ArgumentNullException> (async () => await client.SendAsync (null));

				Assert.Throws<ArgumentNullException> (async () => await client.SendAsync (null, message));
				Assert.Throws<ArgumentNullException> (async () => await client.SendAsync (options, null));

				Assert.Throws<ArgumentNullException> (async () => await client.SendAsync (message, null, recipients));
				Assert.Throws<ArgumentNullException> (async () => await client.SendAsync (message, sender, null));
				Assert.Throws<InvalidOperationException> (async () => await client.SendAsync (message, sender, empty));

				Assert.Throws<ArgumentNullException> (async () => await client.SendAsync (null, message, sender, recipients));
				Assert.Throws<ArgumentNullException> (async () => await client.SendAsync (options, null, sender, recipients));
				Assert.Throws<ArgumentNullException> (async () => await client.SendAsync (options, message, null, recipients));
				Assert.Throws<ArgumentNullException> (async () => await client.SendAsync (options, message, sender, null));
				Assert.Throws<InvalidOperationException> (async () => await client.SendAsync (options, message, sender, empty));

				// Expand
				Assert.Throws<ArgumentNullException> (() => client.Expand (null));
				Assert.Throws<ArgumentException> (() => client.Expand (string.Empty));
				Assert.Throws<ArgumentException> (() => client.Expand ("line1\r\nline2"));
				Assert.Throws<ArgumentNullException> (async () => await client.ExpandAsync (null));
				Assert.Throws<ArgumentException> (async () => await client.ExpandAsync (string.Empty));
				Assert.Throws<ArgumentException> (async () => await client.ExpandAsync ("line1\r\nline2"));

				// Verify
				Assert.Throws<ArgumentNullException> (() => client.Verify (null));
				Assert.Throws<ArgumentException> (() => client.Verify (string.Empty));
				Assert.Throws<ArgumentException> (() => client.Verify ("line1\r\nline2"));
				Assert.Throws<ArgumentNullException> (async () => await client.VerifyAsync (null));
				Assert.Throws<ArgumentException> (async () => await client.VerifyAsync (string.Empty));
				Assert.Throws<ArgumentException> (async () => await client.VerifyAsync ("line1\r\nline2"));
			}
		}

		[Test]
		public async void TestInvalidStateExceptions ()
		{
			var commands = new List<SmtpReplayCommand> ();
			commands.Add (new SmtpReplayCommand ("", "comcast-greeting.txt"));
			commands.Add (new SmtpReplayCommand ("EHLO [127.0.0.1]\r\n", "comcast-ehlo.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "auth-required.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "comcast-mail-from.txt"));
			commands.Add (new SmtpReplayCommand ("RCPT TO:<recipient@example.com>\r\n", "auth-required.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "auth-required.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "comcast-mail-from.txt"));
			commands.Add (new SmtpReplayCommand ("RCPT TO:<recipient@example.com>\r\n", "auth-required.txt"));
			commands.Add (new SmtpReplayCommand ("AUTH PLAIN AHVzZXJuYW1lAHBhc3N3b3Jk\r\n", "comcast-auth-plain.txt"));
			commands.Add (new SmtpReplayCommand ("QUIT\r\n", "comcast-quit.txt"));

			using (var client = new SmtpClient ()) {
				var message = CreateSimpleMessage ();
				var sender = message.From.Mailboxes.FirstOrDefault ();
				var recipients = message.To.Mailboxes.ToList ();
				var options = FormatOptions.Default;

				client.LocalDomain = "127.0.0.1";

				Assert.Throws<ServiceNotConnectedException> (async () => await client.AuthenticateAsync ("username", "password"));
				Assert.Throws<ServiceNotConnectedException> (async () => await client.AuthenticateAsync (new NetworkCredential ("username", "password")));

				Assert.Throws<ServiceNotConnectedException> (async () => await client.NoOpAsync ());

				Assert.Throws<ServiceNotConnectedException> (async () => await client.SendAsync (options, message, sender, recipients));
				Assert.Throws<ServiceNotConnectedException> (async () => await client.SendAsync (message, sender, recipients));
				Assert.Throws<ServiceNotConnectedException> (async () => await client.SendAsync (options, message));
				Assert.Throws<ServiceNotConnectedException> (async () => await client.SendAsync (message));

				try {
					client.ReplayConnect ("localhost", new SmtpReplayStream (commands));
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Connect: {0}", ex);
				}

				Assert.Throws<InvalidOperationException> (async () => await client.ConnectAsync ("host", 465, SecureSocketOptions.SslOnConnect));
				Assert.Throws<InvalidOperationException> (async () => await client.ConnectAsync ("host", 465, true));

				Assert.Throws<ServiceNotAuthenticatedException> (async () => await client.SendAsync (options, message, sender, recipients));
				Assert.Throws<ServiceNotAuthenticatedException> (async () => await client.SendAsync (message, sender, recipients));
				Assert.Throws<ServiceNotAuthenticatedException> (async () => await client.SendAsync (options, message));
				Assert.Throws<ServiceNotAuthenticatedException> (async () => await client.SendAsync (message));

				try {
					await client.AuthenticateAsync ("username", "password");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Authenticate: {0}", ex);
				}

				Assert.Throws<InvalidOperationException> (async () => await client.AuthenticateAsync ("username", "password"));
				Assert.Throws<InvalidOperationException> (async () => await client.AuthenticateAsync (new NetworkCredential ("username", "password")));

				await client.DisconnectAsync (true);
			}
		}

		[Test]
		public async void TestBasicFunctionality ()
		{
			var commands = new List<SmtpReplayCommand> ();
			commands.Add (new SmtpReplayCommand ("", "comcast-greeting.txt"));
			commands.Add (new SmtpReplayCommand ("EHLO unit-tests.mimekit.org\r\n", "comcast-ehlo.txt"));
			commands.Add (new SmtpReplayCommand ("AUTH PLAIN AHVzZXJuYW1lAHBhc3N3b3Jk\r\n", "comcast-auth-plain.txt"));
			commands.Add (new SmtpReplayCommand ("VRFY Smith\r\n", "rfc0821-vrfy.txt"));
			commands.Add (new SmtpReplayCommand ("EXPN Example-People\r\n", "rfc0821-expn.txt"));
			commands.Add (new SmtpReplayCommand ("NOOP\r\n", "comcast-noop.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "comcast-mail-from.txt"));
			commands.Add (new SmtpReplayCommand ("RCPT TO:<recipient@example.com>\r\n", "comcast-rcpt-to.txt"));
			commands.Add (new SmtpReplayCommand ("DATA\r\n", "comcast-data.txt"));
			commands.Add (new SmtpReplayCommand (".\r\n", "comcast-data-done.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "comcast-mail-from.txt"));
			commands.Add (new SmtpReplayCommand ("RCPT TO:<recipient@example.com>\r\n", "comcast-rcpt-to.txt"));
			commands.Add (new SmtpReplayCommand ("DATA\r\n", "comcast-data.txt"));
			commands.Add (new SmtpReplayCommand (".\r\n", "comcast-data-done.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "comcast-mail-from.txt"));
			commands.Add (new SmtpReplayCommand ("RCPT TO:<recipient@example.com>\r\n", "comcast-rcpt-to.txt"));
			commands.Add (new SmtpReplayCommand ("DATA\r\n", "comcast-data.txt"));
			commands.Add (new SmtpReplayCommand (".\r\n", "comcast-data-done.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "comcast-mail-from.txt"));
			commands.Add (new SmtpReplayCommand ("RCPT TO:<recipient@example.com>\r\n", "comcast-rcpt-to.txt"));
			commands.Add (new SmtpReplayCommand ("DATA\r\n", "comcast-data.txt"));
			commands.Add (new SmtpReplayCommand (".\r\n", "comcast-data-done.txt"));
			commands.Add (new SmtpReplayCommand ("QUIT\r\n", "comcast-quit.txt"));

			using (var client = new SmtpClient ()) {
				client.LocalDomain = "unit-tests.mimekit.org";

				try {
					client.ReplayConnect ("localhost", new SmtpReplayStream (commands));
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Connect: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Client failed to connect.");
				Assert.IsFalse (client.IsSecure, "IsSecure should be false.");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Authentication), "Failed to detect AUTH extension");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("LOGIN"), "Failed to detect the LOGIN auth mechanism");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("PLAIN"), "Failed to detect the PLAIN auth mechanism");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EightBitMime), "Failed to detect 8BITMIME extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EnhancedStatusCodes), "Failed to detect ENHANCEDSTATUSCODES extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Size), "Failed to detect SIZE extension");
				Assert.AreEqual (36700160, client.MaxSize, "Failed to parse SIZE correctly");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.StartTLS), "Failed to detect STARTTLS extension");

				Assert.Throws<ArgumentException> (() => client.Capabilities |= SmtpCapabilities.UTF8);

				Assert.AreEqual (100000, client.Timeout, "Timeout");
				client.Timeout *= 2;

				try {
					await client.AuthenticateAsync ("username", "password");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Authenticate: {0}", ex);
				}

				try {
					await client.VerifyAsync ("Smith");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Verify: {0}", ex);
				}

				try {
					await client.ExpandAsync ("Example-People");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Expand: {0}", ex);
				}

				try {
					await client.NoOpAsync ();
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in NoOp: {0}", ex);
				}

				var message = CreateSimpleMessage ();
				var options = FormatOptions.Default;

				try {
					await client.SendAsync (message);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Send: {0}", ex);
				}

				try {
					await client.SendAsync (message, message.From.Mailboxes.FirstOrDefault (), message.To.Mailboxes);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Send: {0}", ex);
				}

				try {
					await client.SendAsync (options, message);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Send: {0}", ex);
				}

				try {
					await client.SendAsync (options, message, message.From.Mailboxes.FirstOrDefault (), message.To.Mailboxes);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Send: {0}", ex);
				}

				try {
					await client.DisconnectAsync (true);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Disconnect: {0}", ex);
				}

				Assert.IsFalse (client.IsConnected, "Failed to disconnect");
			}
		}

		[Test]
		public async void TestEightBitMime ()
		{
			var commands = new List<SmtpReplayCommand> ();
			commands.Add (new SmtpReplayCommand ("", "comcast-greeting.txt"));
			commands.Add (new SmtpReplayCommand ("EHLO [127.0.0.1]\r\n", "comcast-ehlo.txt"));
			commands.Add (new SmtpReplayCommand ("AUTH PLAIN AHVzZXJuYW1lAHBhc3N3b3Jk\r\n", "comcast-auth-plain.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com> BODY=8BITMIME\r\n", "comcast-mail-from.txt"));
			commands.Add (new SmtpReplayCommand ("RCPT TO:<recipient@example.com>\r\n", "comcast-rcpt-to.txt"));
			commands.Add (new SmtpReplayCommand ("DATA\r\n", "comcast-data.txt"));
			commands.Add (new SmtpReplayCommand (".\r\n", "comcast-data-done.txt"));
			commands.Add (new SmtpReplayCommand ("QUIT\r\n", "comcast-quit.txt"));

			using (var client = new SmtpClient ()) {
				try {
					client.ReplayConnect ("localhost", new SmtpReplayStream (commands));
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Connect: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Client failed to connect.");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Authentication), "Failed to detect AUTH extension");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("LOGIN"), "Failed to detect the LOGIN auth mechanism");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("PLAIN"), "Failed to detect the PLAIN auth mechanism");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EightBitMime), "Failed to detect 8BITMIME extension");
				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EnhancedStatusCodes), "Failed to detect ENHANCEDSTATUSCODES extension");
				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Size), "Failed to detect SIZE extension");
				Assert.AreEqual (36700160, client.MaxSize, "Failed to parse SIZE correctly");
				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.StartTLS), "Failed to detect STARTTLS extension");

				try {
					await client.AuthenticateAsync ("username", "password");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Authenticate: {0}", ex);
				}

				try {
					await client.SendAsync (CreateEightBitMessage ());
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Send: {0}", ex);
				}

				try {
					await client.DisconnectAsync (true);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Disconnect: {0}", ex);
				}

				Assert.IsFalse (client.IsConnected, "Failed to disconnect");
			}
		}

		[Test]
		public async void TestBinaryMime ()
		{
			var message = CreateBinaryMessage ();
			string bdat;

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				long size;

				options.NewLineFormat = NewLineFormat.Dos;

				using (var measure = new MeasuringStream ()) {
					message.WriteTo (options, measure);
					size = measure.Length;
				}

				var bytes = Encoding.ASCII.GetBytes (string.Format ("BDAT {0} LAST\r\n", size));
				memory.Write (bytes, 0, bytes.Length);
				message.WriteTo (options, memory);

				bytes = memory.GetBuffer ();

				bdat = Encoding.UTF8.GetString (bytes, 0, (int) memory.Length);
			}

			var commands = new List<SmtpReplayCommand> ();
			commands.Add (new SmtpReplayCommand ("", "comcast-greeting.txt"));
			commands.Add (new SmtpReplayCommand ("EHLO [127.0.0.1]\r\n", "comcast-ehlo+binarymime.txt"));
			commands.Add (new SmtpReplayCommand ("AUTH PLAIN AHVzZXJuYW1lAHBhc3N3b3Jk\r\n", "comcast-auth-plain.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com> BODY=BINARYMIME\r\n", "comcast-mail-from.txt"));
			commands.Add (new SmtpReplayCommand ("RCPT TO:<recipient@example.com>\r\n", "comcast-rcpt-to.txt"));
			commands.Add (new SmtpReplayCommand (bdat, "comcast-data-done.txt"));
			commands.Add (new SmtpReplayCommand ("QUIT\r\n", "comcast-quit.txt"));

			using (var client = new SmtpClient ()) {
				try {
					client.ReplayConnect ("localhost", new SmtpReplayStream (commands));
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Connect: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Client failed to connect.");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Authentication), "Failed to detect AUTH extension");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("LOGIN"), "Failed to detect the LOGIN auth mechanism");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("PLAIN"), "Failed to detect the PLAIN auth mechanism");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EightBitMime), "Failed to detect 8BITMIME extension");
				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EnhancedStatusCodes), "Failed to detect ENHANCEDSTATUSCODES extension");
				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Size), "Failed to detect SIZE extension");
				Assert.AreEqual (36700160, client.MaxSize, "Failed to parse SIZE correctly");
				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.StartTLS), "Failed to detect STARTTLS extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.StartTLS), "Failed to detect STARTTLS extension");

				try {
					await client.AuthenticateAsync ("username", "password");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Authenticate: {0}", ex);
				}

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.BinaryMime), "Failed to detect BINARYMIME extension");
				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Chunking), "Failed to detect CHUNKING extension");

				try {
					await client.SendAsync (message);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Send: {0}", ex);
				}

				try {
					await client.DisconnectAsync (true);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Disconnect: {0}", ex);
				}

				Assert.IsFalse (client.IsConnected, "Failed to disconnect");
			}
		}

		[Test]
		public async void TestPipelining ()
		{
			var commands = new List<SmtpReplayCommand> ();
			commands.Add (new SmtpReplayCommand ("", "comcast-greeting.txt"));
			commands.Add (new SmtpReplayCommand ("EHLO [127.0.0.1]\r\n", "comcast-ehlo+pipelining.txt"));
			commands.Add (new SmtpReplayCommand ("AUTH PLAIN AHVzZXJuYW1lAHBhc3N3b3Jk\r\n", "comcast-auth-plain.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com> BODY=8BITMIME\r\nRCPT TO:<recipient@example.com>\r\n", "pipelined-mail-from-rcpt-to.txt"));
			commands.Add (new SmtpReplayCommand ("DATA\r\n", "comcast-data.txt"));
			commands.Add (new SmtpReplayCommand (".\r\n", "comcast-data-done.txt"));
			commands.Add (new SmtpReplayCommand ("QUIT\r\n", "comcast-quit.txt"));

			using (var client = new SmtpClient ()) {
				try {
					client.ReplayConnect ("localhost", new SmtpReplayStream (commands));
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Connect: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Client failed to connect.");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Authentication), "Failed to detect AUTH extension");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("LOGIN"), "Failed to detect the LOGIN auth mechanism");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("PLAIN"), "Failed to detect the PLAIN auth mechanism");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EightBitMime), "Failed to detect 8BITMIME extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EnhancedStatusCodes), "Failed to detect ENHANCEDSTATUSCODES extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Size), "Failed to detect SIZE extension");
				Assert.AreEqual (36700160, client.MaxSize, "Failed to parse SIZE correctly");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.StartTLS), "Failed to detect STARTTLS extension");

				try {
					await client.AuthenticateAsync ("username", "password");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Authenticate: {0}", ex);
				}

				try {
					await client.SendAsync (CreateEightBitMessage ());
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Send: {0}", ex);
				}

				try {
					await client.DisconnectAsync (true);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Disconnect: {0}", ex);
				}

				Assert.IsFalse (client.IsConnected, "Failed to disconnect");
			}
		}

		[Test]
		public async void TestMailFromMailboxUnavailable ()
		{
			var commands = new List<SmtpReplayCommand> ();
			commands.Add (new SmtpReplayCommand ("", "comcast-greeting.txt"));
			commands.Add (new SmtpReplayCommand ("EHLO [127.0.0.1]\r\n", "comcast-ehlo.txt"));
			commands.Add (new SmtpReplayCommand ("AUTH PLAIN AHVzZXJuYW1lAHBhc3N3b3Jk\r\n", "comcast-auth-plain.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "mailbox-unavailable.txt"));
			commands.Add (new SmtpReplayCommand ("RSET\r\n", "comcast-rset.txt"));
			commands.Add (new SmtpReplayCommand ("QUIT\r\n", "comcast-quit.txt"));

			using (var client = new SmtpClient ()) {
				try {
					client.ReplayConnect ("localhost", new SmtpReplayStream (commands));
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Connect: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Client failed to connect.");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Authentication), "Failed to detect AUTH extension");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("LOGIN"), "Failed to detect the LOGIN auth mechanism");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("PLAIN"), "Failed to detect the PLAIN auth mechanism");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EightBitMime), "Failed to detect 8BITMIME extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EnhancedStatusCodes), "Failed to detect ENHANCEDSTATUSCODES extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Size), "Failed to detect SIZE extension");
				Assert.AreEqual (36700160, client.MaxSize, "Failed to parse SIZE correctly");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.StartTLS), "Failed to detect STARTTLS extension");

				try {
					await client.AuthenticateAsync ("username", "password");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Authenticate: {0}", ex);
				}

				try {
					await client.SendAsync (CreateSimpleMessage ());
					Assert.Fail ("Expected an SmtpException");
				} catch (SmtpCommandException sex) {
					Assert.AreEqual (sex.ErrorCode, SmtpErrorCode.SenderNotAccepted, "Unexpected SmtpErrorCode");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect this exception in Send: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Expected the client to still be connected");

				try {
					await client.DisconnectAsync (true);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Disconnect: {0}", ex);
				}

				Assert.IsFalse (client.IsConnected, "Failed to disconnect");
			}
		}

		[Test]
		public async void TestRcptToMailboxUnavailable ()
		{
			var commands = new List<SmtpReplayCommand> ();
			commands.Add (new SmtpReplayCommand ("", "comcast-greeting.txt"));
			commands.Add (new SmtpReplayCommand ("EHLO [127.0.0.1]\r\n", "comcast-ehlo.txt"));
			commands.Add (new SmtpReplayCommand ("AUTH PLAIN AHVzZXJuYW1lAHBhc3N3b3Jk\r\n", "comcast-auth-plain.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "comcast-mail-from.txt"));
			commands.Add (new SmtpReplayCommand ("RCPT TO:<recipient@example.com>\r\n", "mailbox-unavailable.txt"));
			commands.Add (new SmtpReplayCommand ("RSET\r\n", "comcast-rset.txt"));
			commands.Add (new SmtpReplayCommand ("QUIT\r\n", "comcast-quit.txt"));

			using (var client = new SmtpClient ()) {
				try {
					client.ReplayConnect ("localhost", new SmtpReplayStream (commands));
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Connect: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Client failed to connect.");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Authentication), "Failed to detect AUTH extension");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("LOGIN"), "Failed to detect the LOGIN auth mechanism");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("PLAIN"), "Failed to detect the PLAIN auth mechanism");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EightBitMime), "Failed to detect 8BITMIME extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EnhancedStatusCodes), "Failed to detect ENHANCEDSTATUSCODES extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Size), "Failed to detect SIZE extension");
				Assert.AreEqual (36700160, client.MaxSize, "Failed to parse SIZE correctly");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.StartTLS), "Failed to detect STARTTLS extension");

				try {
					await client.AuthenticateAsync ("username", "password");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Authenticate: {0}", ex);
				}

				try {
					await client.SendAsync (CreateSimpleMessage ());
					Assert.Fail ("Expected an SmtpException");
				} catch (SmtpCommandException sex) {
					Assert.AreEqual (sex.ErrorCode, SmtpErrorCode.RecipientNotAccepted, "Unexpected SmtpErrorCode");
				} catch (Exception ex) {
					Assert.Fail ("Did not expect this exception in Send: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Expected the client to still be connected");

				try {
					await client.DisconnectAsync (true);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Disconnect: {0}", ex);
				}

				Assert.IsFalse (client.IsConnected, "Failed to disconnect");
			}
		}

		[Test]
		public async void TestUnauthorizedAccessException ()
		{
			var commands = new List<SmtpReplayCommand> ();
			commands.Add (new SmtpReplayCommand ("", "comcast-greeting.txt"));
			commands.Add (new SmtpReplayCommand ("EHLO [127.0.0.1]\r\n", "comcast-ehlo.txt"));
			commands.Add (new SmtpReplayCommand ("MAIL FROM:<sender@example.com>\r\n", "auth-required.txt"));
			commands.Add (new SmtpReplayCommand ("QUIT\r\n", "comcast-quit.txt"));

			using (var client = new SmtpClient ()) {
				try {
					client.ReplayConnect ("localhost", new SmtpReplayStream (commands));
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Connect: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Client failed to connect.");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Authentication), "Failed to detect AUTH extension");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("LOGIN"), "Failed to detect the LOGIN auth mechanism");
				Assert.IsTrue (client.AuthenticationMechanisms.Contains ("PLAIN"), "Failed to detect the PLAIN auth mechanism");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EightBitMime), "Failed to detect 8BITMIME extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.EnhancedStatusCodes), "Failed to detect ENHANCEDSTATUSCODES extension");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.Size), "Failed to detect SIZE extension");
				Assert.AreEqual (36700160, client.MaxSize, "Failed to parse SIZE correctly");

				Assert.IsTrue (client.Capabilities.HasFlag (SmtpCapabilities.StartTLS), "Failed to detect STARTTLS extension");

				try {
					await client.SendAsync (CreateSimpleMessage ());
					Assert.Fail ("Expected an ServiceNotAuthenticatedException");
				} catch (ServiceNotAuthenticatedException) {
					// this is the expected exception
				} catch (Exception ex) {
					Assert.Fail ("Did not expect this exception in Send: {0}", ex);
				}

				Assert.IsTrue (client.IsConnected, "Expected the client to still be connected");

				try {
					await client.DisconnectAsync (true);
				} catch (Exception ex) {
					Assert.Fail ("Did not expect an exception in Disconnect: {0}", ex);
				}

				Assert.IsFalse (client.IsConnected, "Failed to disconnect");
			}
		}
	}
}
