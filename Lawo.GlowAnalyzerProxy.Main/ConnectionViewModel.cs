////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System;
    using System.Globalization;
    using System.Net.Sockets;

    using Lawo.ComponentModel;
    using Lawo.Reflection;

    internal sealed class ConnectionViewModel : NotifyPropertyChanged
    {
        public string ConnectionCount => this.connectionCount.Value;

        public string BytesReceived => this.bytesReceived.Value;

        public string SecondsSinceLastReceived => this.secondsSinceLastReceived.Value;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ConnectionViewModel(MainWindowViewModel parent)
        {
            this.parent = parent;
            this.connectionCount = CalculatedProperty.Create(
                this.parent.GetProperty(o => o.IsStopped),
                this.GetProperty(o => o.ConnectionCountCore),
                (s, c) => GetCount(!s, c),
                this.GetProperty(o => o.ConnectionCount));
            this.bytesReceived = CalculatedProperty.Create(
                this.GetProperty(o => o.Client),
                this.GetProperty(o => o.BytesReceivedCore),
                (c, r) => GetCount(c != null, r),
                this.GetProperty(o => o.BytesReceived));
            this.secondsSinceLastReceived = CalculatedProperty.Create(
                this.GetProperty(o => o.Client),
                this.parent.GetProperty(o => o.Now),
                this.GetProperty(o => o.LastReceived),
                (c, n, r) => c == null ? string.Empty : ((long)(n - r).TotalSeconds).ToString(CultureInfo.InvariantCulture),
                this.GetProperty(o => o.SecondsSinceLastReceived));
        }

        internal TcpClient Client
        {
            get
            {
                return this.client;
            }

            set
            {
                this.SetValue(ref this.client, value);

                if (this.client != null)
                {
                    this.BytesReceivedCore = 0;
                    this.LastReceived = DateTime.UtcNow;
                }
            }
        }

        internal int ConnectionCountCore
        {
            get { return this.connectionCountCore; }
            set { this.SetValue(ref this.connectionCountCore, value); }
        }

        internal void AddBytesReceived(int count)
        {
            this.BytesReceivedCore += count;
            this.LastReceived = DateTime.UtcNow;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static string GetCount(bool isValid, long count) =>
            isValid ? count.ToString(CultureInfo.InvariantCulture) : string.Empty;

        private readonly MainWindowViewModel parent;
        private readonly CalculatedProperty<string> connectionCount;
        private readonly CalculatedProperty<string> bytesReceived;
        private readonly CalculatedProperty<string> secondsSinceLastReceived;
        private TcpClient client;
        private int connectionCountCore;
        private long bytesReceivedCore;
        private DateTime lastReceived;

        private long BytesReceivedCore
        {
            get { return this.bytesReceivedCore; }
            set { this.SetValue(ref this.bytesReceivedCore, value); }
        }

        private DateTime LastReceived
        {
            get { return this.lastReceived; }
            set { this.SetValue(ref this.lastReceived, value); }
        }
    }
}
