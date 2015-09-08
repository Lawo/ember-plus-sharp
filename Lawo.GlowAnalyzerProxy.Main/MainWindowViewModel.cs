////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
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
    using Lawo.EmberPlus.S101;
    using Lawo.GlowAnalyzerProxy.Main.Properties;
    using Lawo.IO;
    using Lawo.Reflection;
    using Lawo.Threading.Tasks;

    internal sealed class MainWindowViewModel : NotifyPropertyChanged, IDataErrorInfo
    {
        private static readonly string KeepAliveRequestString = new KeepAliveRequest().ToString();
        private static readonly string KeepAliveResponseString = new KeepAliveResponse().ToString();

        private readonly TaskQueue logQueue = new TaskQueue();
        private readonly Dictionary<string, Func<string>> validationRules = new Dictionary<string, Func<string>>();
        private readonly Settings settings;
        private readonly ConnectionViewModel consumerConnection;
        private readonly ConnectionViewModel providerConnection;
        private readonly ObservableCollection<Event> events = new ObservableCollection<Event>();
        private readonly ReadOnlyObservableCollection<Event> readOnlyEvents;
        private string listeningPort;
        private string providerHostName;
        private string providerPort;
        private string logFolder;
        private bool? autoScrollToMostRecentEvent;
        private readonly CalculatedProperty<bool> canEditSettings;
        //// [CalculatedProperty1]
        private readonly CalculatedProperty<bool> canStart;
        //// [CalculatedProperty1]
        private readonly CalculatedProperty<bool> canStop;
        private Event selectedEvent;
        private FlowDocument selectedEventDetail;
        private bool isSelectedEventDetailPartial;
        private bool isStarted;
        private bool isStopped = true;
        private DateTime now;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Must be accessible from XAML.")]
        //// [ReadOnlyProperty]
        public string Title
        {
            get
            {
                return "Lawo Glow Analyzer Proxy " +
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        //// [ReadOnlyProperty]

        public string ListeningPort
        {
            get { return this.listeningPort; }
            set { this.SetValue(ref this.listeningPort, value); }
        }

        //// [ReadWriteProperty]
        public string ProviderHostName
        {
            get { return this.providerHostName; }
            set { this.SetValue(ref this.providerHostName, value); }
        }
        //// [ReadWriteProperty]

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

        public bool CanEditSettings
        {
            get { return this.canEditSettings.Value; }
        }

        //// [CalculatedProperty3]
        public bool CanStart
        {
            get { return this.canStart.Value; }
        }
        //// [CalculatedProperty3]

        public void Start()
        {
            this.events.Clear();
            this.IsStarted = true;
            this.UpdateTimeLoop();
            this.ListenLoop();
        }

        public bool CanStop
        {
            get { return this.canStop.Value; }
        }

        public void Stop()
        {
            using (this.ConsumerConnection.Client)
            using (this.ProviderConnection.Client)
            {
                this.IsStopped = true;
            }
        }

        public void SaveSettings()
        {
            this.settings.Save();
        }

        //// [CompositeProperty]
        public ConnectionViewModel ConsumerConnection
        {
            get { return this.consumerConnection; }
        }
        //// [CompositeProperty]

        public ConnectionViewModel ProviderConnection
        {
            get { return this.providerConnection; }
        }

        //// [CollectionProperty]
        public ReadOnlyObservableCollection<Event> Events
        {
            get { return this.readOnlyEvents; }
        }
        //// [CollectionProperty]

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
                        this.IsSelectedEventDetailPartial = false;
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

        public bool IsSelectedEventDetailPartial
        {
            get { return this.isSelectedEventDetailPartial; }
            set { this.SetValue(ref this.isSelectedEventDetailPartial, value); }
        }

        public void LoadFullEventDetail()
        {
            if (!this.IsSelectedEventDetailPartial)
            {
                throw new InvalidOperationException("Full event detail has already been loaded.");
            }

            this.IsSelectedEventDetailPartial = false;
            this.LoadEventDetail(int.MaxValue);
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

        public event EventHandler<ScrollEventIntoViewEventArgs> ScrollEventIntoView;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Permanent binding is intended.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        internal MainWindowViewModel(Settings settings)
        {
            this.settings = settings;
            this.consumerConnection = new ConnectionViewModel(this);
            this.providerConnection = new ConnectionViewModel(this);
            this.readOnlyEvents = new ReadOnlyObservableCollection<Event>(this.events);

            //// [TwoWayBinding]
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

            //// [TwoWayBinding]
            this.canEditSettings = CalculatedProperty.Create(
                this.GetProperty(o => o.IsStarted),
                this.GetProperty(o => o.IsStopped),
                (isStarted, isStopped) => !isStarted && isStopped,
                this.GetProperty(o => o.CanEditSettings));
            //// [CalculatedProperty2]
            this.canStart = CalculatedProperty.Create(
                this.GetProperty(o => o.IsStarted),
                this.GetProperty(o => o.IsStopped),
                this.GetProperty(o => o.ListeningPort),
                this.GetProperty(o => o.ProviderPort),
                this.GetProperty(o => o.LogFolder),
                (isStarted, isStopped, lp, pp, lf) => !isStarted && isStopped && string.IsNullOrEmpty(ValidatePort(lp) + ValidatePort(pp) + ValidateFolder(lf)),
                this.GetProperty(o => o.CanStart));
            //// [CalculatedProperty2]
            this.canStop = CalculatedProperty.Create(
                this.GetProperty(o => o.IsStopped), s => !s, this.GetProperty(o => o.CanStop));

            this.AddValidationRule(this.GetProperty(o => o.ProviderPort), p => ValidatePort(p));
            this.AddValidationRule(this.GetProperty(o => o.ListeningPort), p => ValidatePort(p));
            this.AddValidationRule(this.GetProperty(o => o.LogFolder), f => ValidateFolder(f));
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

        private async void UpdateTimeLoop()
        {
            while (this.IsStarted)
            {
                this.Now = DateTime.UtcNow;
                await Task.Delay(500);
            }
        }

        private async void ListenLoop()
        {
            this.ConsumerConnection.ConnectionCountCore = 0;
            this.ProviderConnection.ConnectionCountCore = 0;
            var listener = new TcpListener(IPAddress.Any, int.Parse(this.ListeningPort));
            listener.Start();

            try
            {
                this.IsStopped = false;
                await ForwardLoop(listener);
            }
            finally
            {
                listener.Stop();
                this.IsStarted = false;
            }
        }

        private string GetSecondsSinceLastReceived(TcpClient client, DateTime lastReceived)
        {
            return client == null ?
                string.Empty : ((long)(this.Now - lastReceived).TotalSeconds).ToString(CultureInfo.InvariantCulture);
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
                    string logPath;

                    while (File.Exists(logPath = GetLogFilename()))
                    {
                        await Task.Delay(1000);
                    }

                    var logInfo = new LogInfo(logPath);
                    Exception exception;

                    try
                    {
                        using (this.ConsumerConnection.Client = await listener.AcceptTcpClientAsync())
                        {
                            listener.Stop();

                            using (this.ProviderConnection.Client = await ConnectToProvider())
                            {
                                await Task.WhenAll(this.ForwardFromConsumer(logInfo), this.ForwardFromProvider(logInfo));
                            }
                        }

                        exception = null;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                    finally
                    {
                        this.ConsumerConnection.Client = null;
                        this.ProviderConnection.Client = null;
                        listener.Start();
                    }

                    if (exception != null)
                    {
                        await EnqueueLogOperationAsync(
                            logInfo, "Exception", null, null, i => i.Logger.LogException(exception));
                    }

                    await EnqueueLogOperationAsync(logInfo, null, null, null, i => { i.Dispose(); return new EventInfo(); });
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

        private Task ForwardFromConsumer(LogInfo logInfo)
        {
            return this.ForwardAsync(
                this.ConsumerConnection, this.ProviderConnection, logInfo, "C to P", "Consumer to Provider");
        }

        private Task ForwardFromProvider(LogInfo logInfo)
        {
            return this.ForwardAsync(
                this.ProviderConnection, this.ConsumerConnection, logInfo, "P to C", "Provider to Consumer");
        }

        private async Task ForwardAsync(
            ConnectionViewModel readConnection,
            ConnectionViewModel writeConnection,
            LogInfo logInfo,
            string shortDirection,
            string direction)
        {
            var payloadStream = new MemoryStream();
            var buffer = new byte[8192];

            try
            {
                var s101Reader = new S101Reader(
                    (b, o, c, t) => ForwardBytesAsync(logInfo, direction, readConnection, writeConnection, b, o, c, t));

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

                    await this.EnqueueLogOperationAsync(
                        logInfo, type, shortDirection, length, i => i.Logger.LogMessage(direction, message, payload));
                }

                await s101Reader.DisposeAsync(CancellationToken.None);
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
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
                        this.events.Add(evt);

                        var handler = this.ScrollEventIntoView;

                        if (this.AutoScrollToMostRecentEvent.GetValueOrDefault() && (handler != null))
                        {
                            handler(this, new ScrollEventIntoViewEventArgs(evt));
                        }
                    }
                });
        }

        private async void LoadEventDetail(int maxBytesToLoad)
        {
            var evt = this.SelectedEvent;
            SetSelectedEventDetail("Loading Event...");

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

                SetSelectedEventDetail(eventText.Replace("\r\n  ", "\r\n").Substring(2));
                this.IsSelectedEventDetailPartial = isPartial;
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

        private static string GetCount(bool isValid, long count)
        {
            return isValid ? count.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

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

        private static string ValidateFolder(string folder)
        {
            return Directory.Exists(folder) ? null : "Folder does not exist.";
        }

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
            else
            {
                return "Data";
            }
        }
    }
}
