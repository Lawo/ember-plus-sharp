////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;

    using Ember;
    using IO;
    using Threading.Tasks;

    /// <summary>Simulates S101 communication.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class S101Robot
    {
        /// <summary>Asynchronously simulates S101 communication.</summary>
        /// <param name="client">The <see cref="S101Client"/> to use.</param>
        /// <param name="types">The types to pass to the internal <see cref="EmberConverter"/>, which is used to convert
        /// between XML payload and EmBER payload.</param>
        /// <param name="logReader">The <see cref="XmlReader"/> to read the messages from. The messages that are
        /// expected to be received from the remote party as well as the ones that will be sent are read with this
        /// reader. The format needs to match the one written by <see cref="S101Logger"/>.</param>
        /// <param name="sendFirstMessage"><c>true</c> to send the first <see cref="EmberData"/> message read with
        /// <paramref name="logReader"/>; <c>false</c> to wait for the first message from the remote party and match it
        /// to the first <see cref="EmberData"/> message read with <paramref name="logReader"/>.</param>
        /// <returns>A <see cref="Task"/> object representing the communication. This task completes when one of the
        /// following events occurs:
        /// <list type="bullet">
        /// <item>The last message in the log has been sent/received.</item>
        /// <item>The <see cref="S101Client.ConnectionLost"/> event occurred on the client passed to
        /// <see cref="RunAsync"/>.</item>
        /// </list></returns>
        /// <exception cref="ArgumentNullException"><paramref name="client"/>, <paramref name="types"/> and/or
        /// <paramref name="logReader"/> equal <c>null</c>.</exception>
        /// <exception cref="S101Exception"><list type="bullet">
        /// <item>There was a mismatch between an incoming message and one read from the log.</item>
        /// <item>The <see cref="S101Client.ConnectionLost"/> event occurred on the client passed to
        /// <see cref="RunAsync"/>.</item>
        /// </list></exception>
        /// <exception cref="XmlException">The XML read with <paramref name="logReader"/> is invalid, see
        /// <see cref="Exception.Message"/> for details.</exception>
        /// <remarks>
        /// <para>Reads messages with <paramref name="logReader"/> and depending on the direction either sends them to
        /// the remote party or matches them to messages received from the remote party. If a message received from the
        /// remote party does not match the one in the log then an appropriate exception is thrown.</para>
        /// <para>Subsequent messages read with <paramref name="logReader"/> that match the direction of the first
        /// message read with <paramref name="logReader"/> are sent if <paramref name="sendFirstMessage"/> equals
        /// <c>true</c>; otherwise such messages are matched to the ones received from the remote party. The opposite
        /// happens with log messages of opposite direction.</para>
        /// </remarks>
        public static async Task RunAsync(
            S101Client client, EmberTypeBag types, XmlReader logReader, bool sendFirstMessage)
        {
            var robot = new S101Robot(client, types, logReader, sendFirstMessage);

            try
            {
                await robot.WaitAsync();
            }
            finally
            {
                robot.Dispose();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly TaskQueue taskQueue = new TaskQueue();
        private readonly TaskCompletionSource<bool> done = new TaskCompletionSource<bool>();
        private readonly S101Client client;
        private readonly S101LogReader logReader;
        private readonly bool sendFirstMessage;
        private string firstMessageDirection;

        private S101Robot(S101Client client, EmberTypeBag types, XmlReader logReader, bool sendFirstMessage)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.logReader = new S101LogReader(types, logReader);
            this.sendFirstMessage = sendFirstMessage;

            this.client.OutOfFrameByteReceived += this.OnOutOfFrameByteReceived;
            this.client.EmberDataReceived += this.OnClientEmberDataReceived;
            this.client.ConnectionLost += this.OnClientConnectionLost;
            this.SendMessages();
        }

        private Task WaitAsync() => this.done.Task;

        private void Dispose()
        {
            this.taskQueue.Enqueue(
                () =>
                {
                    this.client.ConnectionLost -= this.OnClientConnectionLost;
                    this.client.EmberDataReceived -= this.OnClientEmberDataReceived;
                    this.client.OutOfFrameByteReceived -= this.OnOutOfFrameByteReceived;
                    return Task.FromResult(false);
                }).Ignore();
        }

        private async void SendMessages()
        {
            await this.taskQueue.Enqueue(this.SendMessagesAsync);
        }

        private async void OnOutOfFrameByteReceived(object sender, OutOfFrameByteReceivedEventArgs e)
        {
            await this.taskQueue.Enqueue(() => this.ProcessIncomingOutOfFrameByte(e));
        }

        private async void OnClientEmberDataReceived(object sender, MessageReceivedEventArgs e)
        {
            await this.taskQueue.Enqueue(() => this.ProcessIncomingMessage(e));
        }

        private void OnClientConnectionLost(object sender, ConnectionLostEventArgs e) =>
            this.done.TrySetException(
                new S101Exception("The connection was lost before all messages could be processed.", e.Exception));

        private async Task SendMessagesAsync()
        {
            try
            {
                while (this.logReader.Read())
                {
                    switch (this.logReader.EventType)
                    {
                        case "OutOfFrameByte":
                            if (!await this.SendEvent(() => this.client.SendOutOfFrameByteAsync(
                                this.logReader.GetPayload()[0])))
                            {
                                return;
                            }

                            break;
                        case "Message":
                            if (this.logReader.Message.Command is EmberData)
                            {
                                if (!await this.SendEvent(() => this.client.SendMessageAsync(
                                    this.logReader.Message, this.logReader.GetPayload())))
                                {
                                    return;
                                }
                            }

                            break;
                        default:
                            // All other event types are intentionally ignored
                            break;
                    }
                }

                this.done.TrySetResult(true);
            }
            catch (Exception ex)
            {
                this.done.TrySetException(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is forwarded.")]
        private Task ProcessIncomingOutOfFrameByte(OutOfFrameByteReceivedEventArgs e)
        {
            try
            {
                var expected = this.logReader.GetPayload()[0];

                if (expected != e.Value)
                {
                    var msg = "The expected payload does not match the actual received payload, see Data for details.";
                    throw new S101Exception(msg) { Data = { { "Expected", expected }, { "Actual", e.Value } } };
                }
            }
            catch (Exception ex)
            {
                this.done.TrySetException(ex);
                return Task.FromResult(false);
            }

            return this.SendMessagesAsync();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is forwarded.")]
        private Task ProcessIncomingMessage(MessageReceivedEventArgs e)
        {
            try
            {
                var expected = this.logReader.GetPayload();

                // We're converting to XML and back again to normalize the payload. This could be done much more
                // efficiently by a dedicated method, which has yet to be implemented.
                var actualXml = this.ToXml(e.GetPayload());
                var actual = this.FromXml(actualXml);

                if (!expected.SequenceEqual(actual))
                {
                    var msg = "The expected payload does not match the actual received payload, see Data for details.";
                    var expectedXml = this.ToXml(expected);
                    throw new S101Exception(msg) { Data = { { "Expected", expectedXml }, { "Actual", actualXml } } };
                }
            }
            catch (Exception ex)
            {
                this.done.TrySetException(ex);
                return Task.FromResult(false);
            }

            return this.SendMessagesAsync();
        }

        private async Task<bool> SendEvent(Func<Task> sendOperation)
        {
            if (this.firstMessageDirection == null)
            {
                this.firstMessageDirection = this.logReader.Direction;
            }

            if (this.sendFirstMessage == (this.firstMessageDirection == this.logReader.Direction))
            {
                await sendOperation();
                return true;
            }
            else
            {
                return false;
            }
        }

        private string ToXml(byte[] payload)
        {
            var xml = new StringBuilder(payload.Length * 10);

            using (var writer = XmlWriter.Create(xml, new XmlWriterSettings { Indent = true }))
            {
                this.logReader.Converter.ToXml(payload, writer);
            }

            return xml.ToString();
        }

        private byte[] FromXml(string xml)
        {
            using (var reader = new StringReader(xml))
            {
                return this.logReader.Converter.FromXml(XmlReader.Create(reader));
            }
        }
    }
}
