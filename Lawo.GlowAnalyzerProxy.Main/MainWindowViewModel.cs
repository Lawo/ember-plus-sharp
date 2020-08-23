////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Documents;
    using System.Windows.Media;
    using Lawo.ComponentModel;
    using Lawo.EmberPlusSharp.S101;
    using Lawo.GlowAnalyzerProxy.Main.Properties;
    using Lawo.IO;
    using Lawo.Reflection;
    using Lawo.Threading.Tasks;

    // The following suppressions are necessary so that tested code snippets can be included in the documentation.
#pragma warning disable SA1123 // Do not place regions within elements
#pragma warning disable SA1124 // Do not use regions
    internal sealed class MainWindowViewModel : NotifyPropertyChanged, IDataErrorInfo
    {
        public event EventHandler<ScrollEventIntoViewEventArgs> ScrollEventIntoView;

        public event EventHandler<ListenFailedEventArgs> ListenFailed;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Must be accessible from XAML.")]
        #region ReadOnlyProperty
        public string Title
        {
            get
            {
                return "Lawo Glow Analyzer Proxy " +
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
        #endregion

        public string ListeningPort
        {
            get { return this.listeningPort; }
            set { this.SetValue(ref this.listeningPort, value); }
        }

        #region ReadWriteProperty
        public string ProviderHostName
        {
            get { return this.providerHostName; }
            set { this.SetValue(ref this.providerHostName, value); }
        }
        #endregion

        public string ProviderPort
        {
            get { return this.providerPort; }
            set { this.SetValue(ref this.providerPort, value); }
        }

        public string LogFolder
        {
            get { return this.logFolder; }
            set { this.SetValue(ref this.logFolder, value); }
        }

        public bool? AutoScrollToMostRecentEvent
        {
            get { return this.autoScrollToMostRecentEvent; }
            set { this.SetValue(ref this.autoScrollToMostRecentEvent, value); }
        }

        public bool CanEditSettings => this.canEditSettings.Value;

        #region CalculatedProperty3
        public bool CanStart => this.canStart.Value;

        #endregion

        public bool CanStop => this.canStop.Value;

        #region CompositeProperty
        public ConnectionViewModel ConsumerConnection => this.consumerConnection;

        #endregion

        public ConnectionViewModel ProviderConnection => this.providerConnection;

        #region CollectionProperty
        public ReadOnlyObservableCollection<Event> Events => this.readOnlyEvents;

        #endregion

        public Event SelectedEvent
        {
            get
            {
                return this.selectedEvent;
            }

            set
            {
                if (this.SetValue(ref this.selectedEvent, value))
                {
                    if (value == null)
                    {
                        this.SelectedEventDetail = null;
                        this.CanLoadFullEventDetail = false;
                    }
                    else
                    {
                        this.LoadEventDetail(1 << 16);
                    }
                }
            }
        }

        public FlowDocument SelectedEventDetail
        {
            get { return this.selectedEventDetail; }
            private set { this.SetValue(ref this.selectedEventDetail, value); }
        }

        public bool CanLoadFullEventDetail
        {
            get { return this.canLoadFullEventDetail; }
            private set { this.SetValue(ref this.canLoadFullEventDetail, value); }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                Func<string> rule;
                return this.validationRules.TryGetValue(columnName, out rule) ? rule() : null;
            }
        }

        public void Start()
        {
            this.events.Clear();
            this.IsStarted = true;
            this.UpdateLoop();
            this.ListenLoop();
        }

        public void Stop()
        {
            using (this.ConsumerConnection.Client)
            using (this.ProviderConnection.Client)
            {
                this.IsStopped = true;
            }
        }

        public void SaveSettings() => this.settings.Save();

        public void LoadFullEventDetail()
        {
            if (!this.CanLoadFullEventDetail)
            {
                throw new InvalidOperationException("Full event detail has already been loaded.");
            }

            this.CanLoadFullEventDetail = false;
            this.LoadEventDetail(int.MaxValue);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="settings"></param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Permanent binding is intended.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        internal MainWindowViewModel(Settings settings)
        {
            this.readOnlyEvents = new ReadOnlyObservableCollection<Event>(this.events);
            this.consumerConnection = new ConnectionViewModel(this);
            this.providerConnection = new ConnectionViewModel(this);
            this.canEditSettings = CalculatedProperty.Create(
                this.GetProperty(o => o.IsStarted),
                this.GetProperty(o => o.IsStopped),
                (isStarted, isStopped) => !isStarted && isStopped,
                this.GetProperty(o => o.CanEditSettings));
            #region  CalculatedProperty2
            this.canStart = CalculatedProperty.Create(
                this.GetProperty(o => o.IsStarted),
                this.GetProperty(o => o.IsStopped),
                this.GetProperty(o => o.ListeningPort),
                this.GetProperty(o => o.ProviderPort),
                this.GetProperty(o => o.LogFolder),
                (isStarted, isStopped, lp, pp, lf) => !isStarted && isStopped && string.IsNullOrEmpty(ValidatePort(lp) + ValidatePort(pp) + ValidateFolder(lf)),
                this.GetProperty(o => o.CanStart));
            #endregion
            this.canStop = CalculatedProperty.Create(
                this.GetProperty(o => o.IsStopped), s => !s, this.GetProperty(o => o.CanStop));
            this.settings = settings;

            #region TwoWayBinding
            TwoWayBinding.Create(
                this.settings.GetProperty(o => o.ListeningPort), this.GetProperty(o => o.ListeningPort));
            TwoWayBinding.Create(
                this.settings.GetProperty(o => o.ProviderHostName), this.GetProperty(o => o.ProviderHostName));
            TwoWayBinding.Create(
                this.settings.GetProperty(o => o.ProviderPort), this.GetProperty(o => o.ProviderPort));
            TwoWayBinding.Create(
                this.settings.GetProperty(o => o.LogFolder), this.GetProperty(o => o.LogFolder));
            TwoWayBinding.Create(
                this.settings.GetProperty(o => o.AutoScrollToMostRecentEvent),
                a => a,
                this.GetProperty(o => o.AutoScrollToMostRecentEvent),
                a => a.GetValueOrDefault());
            #endregion

            this.AddValidationRule(this.GetProperty(o => o.ProviderPort), ValidatePort);
            this.AddValidationRule(this.GetProperty(o => o.ListeningPort), ValidatePort);
            this.AddValidationRule(this.GetProperty(o => o.LogFolder), ValidateFolder);
        }

        internal bool IsStarted
        {
            get { return this.isStarted; }
            private set { this.SetValue(ref this.isStarted, value); }
        }

        internal bool IsStopped
        {
            get { return this.isStopped; }
            private set { this.SetValue(ref this.isStopped, value); }
        }

        internal DateTime Now
        {
            get { return this.now; }
            private set { this.SetValue(ref this.now, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string ShortConsumerToProvider = "C to P";
        private const string ConsumerToProvider = "Consumer to Provider";
        private const string ShortProviderToConsumer = "P to C";
        private const string ProviderToConsumer = "Provider to Consumer";
        private static readonly string KeepAliveRequestString = new KeepAliveRequest().ToString();
        private static readonly string KeepAliveResponseString = new KeepAliveResponse().ToString();
        private static readonly string ProviderStatusPassiveString = new ProviderStatus(false).ToString();
        private static readonly string ProviderStatusActiveString = new ProviderStatus(true).ToString();

        private static string ValidatePort(string value)
        {
            int intValue;

            if (!int.TryParse(value, out intValue))
            {
                return "Value is not an integer.";
            }

            if ((intValue < IPEndPoint.MinPort) || (intValue > IPEndPoint.MaxPort))
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Values lies outside of the interval [{0}, {1}].",
                    IPEndPoint.MinPort,
                    IPEndPoint.MaxPort);
            }
            else
            {
                return null;
            }
        }

        private static string ValidateFolder(string folder) =>
            Directory.Exists(folder) ? null : "Folder does not exist.";

        private static string GetShortType(string type)
        {
            if (type == KeepAliveRequestString)
            {
                return "KA REQ";
            }
            else if (type == KeepAliveResponseString)
            {
                return "KA RSP";
            }
            else if ((type == ProviderStatusPassiveString) || (type == ProviderStatusActiveString))
            {
                return "PS";
            }
            else
            {
                return "Data";
            }
        }

        private readonly TaskQueue logQueue = new TaskQueue();
        private readonly Dictionary<string, Func<string>> validationRules = new Dictionary<string, Func<string>>();
        private readonly List<Event> eventCache = new List<Event>();
        private readonly ObservableCollection<Event> events = new ObservableCollection<Event>();
        private readonly ReadOnlyObservableCollection<Event> readOnlyEvents;
        private readonly ConnectionViewModel consumerConnection;
        private readonly ConnectionViewModel providerConnection;
        private readonly Settings settings;
        private readonly CalculatedProperty<bool> canEditSettings;
        #region  CalculatedProperty1
        private readonly CalculatedProperty<bool> canStart;
        #endregion
        private readonly CalculatedProperty<bool> canStop;
        private string listeningPort;
        private string providerHostName;
        private string providerPort;
        private string logFolder;
        private bool? autoScrollToMostRecentEvent;
        private Event selectedEvent;
        private FlowDocument selectedEventDetail;
        private bool canLoadFullEventDetail;
        private bool isStarted;
        private bool isStopped = true;
        private DateTime now;

        private async void UpdateLoop()
        {
            while (this.IsStarted)
            {
                this.Now = DateTime.UtcNow;

                // The following is necessary because adding events one by one did only scale to roughly 100 events
                // per second, due to high CPU load, see #12.
                foreach (var evt in this.eventCache)
                {
                    this.events.Add(evt);
                }

                this.eventCache.Clear();

                if ((this.events.Count > 0) && this.AutoScrollToMostRecentEvent.GetValueOrDefault())
                {
                    this.ScrollEventIntoView?.Invoke(
                        this, new ScrollEventIntoViewEventArgs(this.events[this.events.Count - 1]));
                }

                await Task.Delay(250);
            }
        }

        private async void ListenLoop()
        {
            this.ConsumerConnection.ConnectionCountCore = 0;
            this.ProviderConnection.ConnectionCountCore = 0;
            var listener = new TcpListener(IPAddress.Any, int.Parse(this.ListeningPort));

            try
            {
                listener.Start(1);
                this.IsStopped = false;
                await this.ForwardLoop(listener);
            }
            catch (SocketException ex)
            {
                this.ListenFailed?.Invoke(this, new ListenFailedEventArgs(ex));
            }
            finally
            {
                listener.Stop();
                this.IsStarted = false;
            }
        }

        private void AddValidationRule<T>(IProperty<MainWindowViewModel, T> property, Func<T, string> rule)
        {
            this.validationRules.Add(property.PropertyInfo.Name, () => rule(property.Value));
        }

        private async Task ForwardLoop(TcpListener listener)
        {
            while (this.IsStarted && !this.IsStopped)
            {
                if (listener.Pending())
                {
                    ++this.ConsumerConnection.ConnectionCountCore;
                    string logPath;

                    while (File.Exists(logPath = this.GetLogFilename()))
                    {
                        await Task.Delay(1000);
                    }

                    var logInfo = new LogInfo(logPath);

                    try
                    {
                        ++this.ProviderConnection.ConnectionCountCore;
                        this.ProviderConnection.Client = await this.ConnectToProvider();

                        try
                        {
                            this.ConsumerConnection.Client = await listener.AcceptTcpClientAsync();
                            listener.Stop();
                            await Task.WhenAll(
                                this.ForwardFromConsumerAsync(logInfo), this.ForwardFromProviderAsync(logInfo));
                        }
                        catch (Exception ex)
                        {
                            listener.Stop();
                            await this.EnqueueLogOperationAsync(
                                logInfo, "Exception", null, null, i => i.Logger.LogException("Consumer to Proxy", ex));
                        }

                        if (this.ConsumerConnection.Client != null)
                        {
                            this.ConsumerConnection.Client.Close();
                            this.ConsumerConnection.Client = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        listener.Stop();
                        await this.EnqueueLogOperationAsync(
                            logInfo, "Exception", null, null, i => i.Logger.LogException("Proxy to Provider", ex));
                    }

                    if (this.ProviderConnection.Client != null)
                    {
                        this.ProviderConnection.Client.Close();
                        this.ProviderConnection.Client = null;
                    }

                    Func<LogInfo, EventInfo> operation =
                        i =>
                        {
                            i.Dispose();
                            return default(EventInfo);
                        };

                    await this.EnqueueLogOperationAsync(logInfo, null, null, null, operation);
                    listener.Start(1);
                }
                else
                {
                    await Task.Delay(500);
                }
            }
        }

        private string GetLogFilename()
        {
            var filename = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_UTC", CultureInfo.InvariantCulture) + ".xml";
            return Path.Combine(this.LogFolder, filename);
        }

        private async Task<TcpClient> ConnectToProvider()
        {
            var providerClient = new TcpClient();
            await providerClient.ConnectAsync(this.ProviderHostName, int.Parse(this.ProviderPort));
            return providerClient;
        }

        private Task ForwardFromConsumerAsync(LogInfo logInfo) =>
            this.ForwardAsync(
                this.ConsumerConnection, this.ProviderConnection, logInfo, ShortConsumerToProvider, ConsumerToProvider);

        private Task ForwardFromProviderAsync(LogInfo logInfo) =>
            this.ForwardAsync(
                this.ProviderConnection, this.ConsumerConnection, logInfo, ShortProviderToConsumer, ProviderToConsumer);

        private async Task ForwardAsync(
            ConnectionViewModel readConnection,
            ConnectionViewModel writeConnection,
            LogInfo logInfo,
            string shortDirection,
            string direction)
        {
            var payloadStream = new MemoryStream();
            var buffer = new byte[8192];
            var s101Reader = new S101Reader(
                (b, o, c, t) => this.ForwardBytesAsync(logInfo, direction, readConnection, writeConnection, b, o, c, t));
            Func<LogInfo, OutOfFrameByteReceivedEventArgs, EventInfo> logOutOfFrame =
                (i, e) => i.Logger.LogData("OutOfFrameByte", direction, new[] { e.Value }, 0, 1);
            EventHandler<OutOfFrameByteReceivedEventArgs> handler = async (s, e) =>
                await this.EnqueueLogOperationAsync(logInfo, "OOFB", shortDirection, null, i => logOutOfFrame(i, e));

            try
            {
                s101Reader.OutOfFrameByteReceived += handler;

                while (await s101Reader.ReadAsync(CancellationToken.None))
                {
                    payloadStream.SetLength(0);
                    int read;

                    while ((read = await s101Reader.Payload.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        payloadStream.Write(buffer, 0, read);
                    }

                    var type = GetShortType(s101Reader.Message.Command.ToString());
                    var message = s101Reader.Message;
                    var payload = payloadStream.ToArray();
                    var length = payload.Length;

                    if (length > 0)
                    {
                        await this.logQueue.Enqueue(
                            () => Task.Run(() => logInfo.Logger.LogData("DecodedPayload", direction, payload, 0, length)));
                    }

                    await this.EnqueueLogOperationAsync(
                        logInfo, type, shortDirection, length, i => i.Logger.LogMessage(direction, message, payload));
                }

                await s101Reader.DisposeAsync(CancellationToken.None);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                await this.EnqueueLogOperationAsync(
                    logInfo, "Exception", shortDirection, null, i => i.Logger.LogException(direction, ex));
            }
            finally
            {
                s101Reader.OutOfFrameByteReceived -= handler;
                payloadStream.Dispose();
                writeConnection.Client.Close();
                readConnection.Client.Close();
            }
        }

        private async Task EnqueueLogOperationAsync(
            LogInfo logInfo, string type, string direction, int? length, Func<LogInfo, EventInfo> operation)
        {
            var connection = this.ConsumerConnection.ConnectionCountCore;

            await this.logQueue.Enqueue(
                async () =>
                {
                    var logStartPos = logInfo.StartPosition;
                    var eventInfo = await Task.Run(() => operation(logInfo));
                    var logEndPos = logInfo.EndPosition;

                    if (logEndPos.HasValue)
                    {
                        var seconds = (eventInfo.TimeUtc.Value - logInfo.StartTimeUtc).TotalSeconds;
                        var logLength = logEndPos.Value - logStartPos;
                        var evt = new Event(
                            connection,
                            seconds,
                            type,
                            direction,
                            eventInfo.Number,
                            length,
                            logInfo.Path,
                            logStartPos,
                            logLength);
                        this.eventCache.Add(evt);
                    }
                });
        }

        private async void LoadEventDetail(int maxBytesToLoad)
        {
            var evt = this.SelectedEvent;
            this.SetSelectedEventDetail("Loading Event...");

            // Make sure we yield at least once, such that the GUI has a change to update to the previously set null
            // value
            await Task.Delay(1);

            using (var stream = new FileStream(evt.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.Position = evt.LogPosition;
                var buffer = new byte[Math.Min(evt.LogLength, maxBytesToLoad)];
                await StreamHelper.FillAsync(stream.ReadAsync, buffer, 0, buffer.Length, CancellationToken.None);
                var eventText = Encoding.UTF8.GetString(buffer);
                var isPartial = buffer.Length < evt.LogLength;

                if (isPartial)
                {
                    eventText += " (TRUNCATED, click the button below to load the full event.)";
                }

                this.SetSelectedEventDetail(eventText.Replace("\r\n  ", "\r\n").Substring(2));
                this.CanLoadFullEventDetail = isPartial;
            }
        }

        private async Task<int> ForwardBytesAsync(
            LogInfo logInfo,
            string direction,
            ConnectionViewModel readConnection,
            ConnectionViewModel writeConnection,
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            var result = await readConnection.Client.GetStream().ReadAsync(buffer, offset, count, cancellationToken);
            await this.logQueue.Enqueue(
                () => Task.Run(() => logInfo.Logger.LogData("RawData", direction, buffer, 0, result)));
            await writeConnection.Client.GetStream().WriteAsync(buffer, offset, result, cancellationToken);
            readConnection.AddBytesReceived(result);
            return result;
        }

        private void SetSelectedEventDetail(string text)
        {
            var paragraph = new Paragraph(new Run(text)) { FontFamily = new FontFamily("Courier"), FontSize = 12 };
            this.SelectedEventDetail = new FlowDocument() { PageWidth = 3000, Blocks = { paragraph } };
        }
    }
#pragma warning restore SA1124 // Do not use regions
#pragma warning restore SA1123 // Do not place regions within elements
}
