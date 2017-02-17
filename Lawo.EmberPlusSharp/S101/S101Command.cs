////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;

    /// <summary>Represents a command including command-specific data but excluding payload. This is the base of all
    /// commands.</summary>
    /// <remarks>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public abstract class S101Command : IEquatable<S101Command>
    {
        /// <inheritdoc/>
        public bool Equals(S101Command other) => (other != null) && (other.commandType == this.commandType);

        /// <inheritdoc/>
        public sealed override bool Equals(object obj) => this.Equals(obj as S101Command);

        /// <inheritdoc/>
        public sealed override int GetHashCode() => (int)this.commandType;

        /// <inheritdoc/>
        public override string ToString() => this.commandType.ToString();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static async Task<S101Command> ReadFromAsync(ReadBuffer readBuffer, CancellationToken cancellationToken)
        {
            await readBuffer.FillAsync(2, cancellationToken);
            var result = GetCommandAndVersion(readBuffer);
            await result.ReadFromCoreAsync(readBuffer, cancellationToken);
            return result;
        }

        internal static S101Command Parse(string str)
        {
            var components = str.Split();
            var commandType = (CommandType)Enum.Parse(typeof(CommandType), components[0]);
            var result = CreateCommand(commandType);
            result.ParseCore(components);
            return result;
        }

        internal S101Command(CommandType commandType)
        {
            this.commandType = commandType;
        }

        internal virtual bool CanHavePayload => false;

        internal virtual bool CanHaveMultiplePackets => false;

        internal PacketFlags PacketFlags { get; set; }

        internal async Task WriteToAsync(WriteBuffer writeBuffer, CancellationToken cancellationToken)
        {
            await writeBuffer.ReserveAsync(2, cancellationToken);
            writeBuffer[writeBuffer.Count++] = (byte)this.commandType;
            writeBuffer[writeBuffer.Count++] = DefaultVersion;
            await this.WriteToCoreAsync(writeBuffer, cancellationToken);
        }

        internal virtual Task ReadFromCoreAsync(ReadBuffer readBuffer, CancellationToken cancellationToken) =>
            Completed;

        internal virtual Task WriteToCoreAsync(WriteBuffer writeBuffer, CancellationToken cancellationToken) =>
            Completed;

        internal virtual void ParseCore(string[] components)
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static S101Command GetCommandAndVersion(ReadBuffer readBuffer)
        {
            var commandType = (CommandType)readBuffer[readBuffer.Index++];

            if (readBuffer[readBuffer.Index++] != DefaultVersion)
            {
                throw new S101Exception("Unexpected Version.");
            }

            return CreateCommand(commandType);
        }

        private static S101Command CreateCommand(CommandType commandType)
        {
            switch (commandType)
            {
                case CommandType.EmberData:
                    return new EmberData();
                case CommandType.KeepAliveRequest:
                    return new KeepAliveRequest();
                case CommandType.KeepAliveResponse:
                    return new KeepAliveResponse();
                case CommandType.ProviderStatus:
                    return new ProviderStatus();
                default:
                    throw new S101Exception("Unexpected Command.");
            }
        }

        private const byte DefaultVersion = 0x01;
        private static readonly Task Completed = Task.FromResult(false);

        private readonly CommandType commandType;
    }
}
