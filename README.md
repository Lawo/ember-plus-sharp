EmBER+ Sharp Project
=============================================================================

Check out the [documentation](http://lawo.github.io/ember-plus-sharp) for the EmBER+. This is the C#, .net library for a EmBER+ Consumer.

### Soluton contains:
- Lawo - Common shared classes (Required by main library) - .NET Standard 2.1, .NET Framework 4.5, .NET Framework 4.8
- Lawo.EmberPlusSharp - The EmBER+ Consumer main library - .NET Standard 2.1, .NET Framework 4.5, .NET Framework 4.8

- Lawo.GlowAnalyzerProxy.Main - An EmBER+ analyzer tool for Glow debugging - WPF, .NET Framework 4.5
- Lawo.GlowLogConverter.Main - Converts the log format received from the Glow Analyzer Proxy - WPF, .NET Framework 4.5

### Example to get started

```csharp
// Note that the most-derived subtype MyRoot needs to be passed to the generic base class.
// Represents the root containing dynamic and optional static elements in the object tree accessible through Consumer<TRoot>.Root
private class MyRoot : DynamicRoot<MyRoot> { }

// Create TCP connection
var tcpClient = new TcpClient();
await tcpClient.ConnectAsync("localhost", "9001");

// Establish S101 protocol
// S101 provides message packaging, CRC integrity checks and a keep-alive mechanism.
var stream = tcpClient.GetStream();
var s101Client = new S101Client(tcpClient, stream.ReadAsync, stream.WriteAsync);

// Create consumer
var consumer = await Consumer<MyRoot>.CreateAsync(s101Client));

// Navigate down tree until IParameter is reached or desired INode
var mixer = (INode)root.Children.First(c => c.Identifier == "MixerEmberIdentifier");
var mute = (IParameter)mixer.Children.First(c => c.Identifier == "Mute");

mute.Value = true;

```
