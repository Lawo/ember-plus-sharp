////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    // The following suppressions are necessary so that tested code snippets can be included in the documentation.
#pragma warning disable SA1123 // Do not place regions within elements
#pragma warning disable SA1124 // Do not use regions
#pragma warning disable SA1515 // Single-line comment must be preceded by blank line
    #region Using Declarations
    using System;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using S101;
    using Threading.Tasks;
    #endregion

    /// <summary>Tests the <see cref="Consumer{T}"/>-based code shown in the tutorial.</summary>
    [TestClass]
    public class TutorialTest
    {
        /// <summary>Establishes a connection to a provider and creates a dynamic local copy of the database.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void DynamicConnectTest() => MainTutorial();

        /// <summary>Iterates over the dynamic local database.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void DynamicIterateTest()
        {
            #region Dynamic Iterate
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<MyRoot>.CreateAsync(client))
                    {
                        WriteChildren(consumer.Root, 0);
                    }
                }, cancelToken);
            #endregion
        }

        /// <summary>Demonstrates how to react to changes with the dynamic interface.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void DynamicReactToChangesTest()
        {
            #region Dynamic React to Changes
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<MyRoot>.CreateAsync(client))
                    {
                        INode root = consumer.Root;

                        // Navigate to the parameter we're interested in.
                        var sapphire = (INode)root.Children.First(c => c.Identifier == "Sapphire");
                        var sources = (INode)sapphire.Children.First(c => c.Identifier == "Sources");
                        var fpgm1 = (INode)sources.Children.First(c => c.Identifier == "FPGM 1");
                        var fader = (INode)fpgm1.Children.First(c => c.Identifier == "Fader");
                        var positionParameter = fader.Children.First(c => c.Identifier == "Position");

                        var valueChanged = new TaskCompletionSource<string>();
                        positionParameter.PropertyChanged += (s, e) => valueChanged.SetResult(((IElement)s).GetPath());
                        Console.WriteLine("Waiting for the parameter to change...");
                        Console.WriteLine("A value of the element with the path {0} has been changed.", await valueChanged.Task);
                    }
                }, cancelToken);
            #endregion
        }

        /// <summary>Modifies parameters in the dynamic local database.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void DynamicModifyTest()
        {
            #region Dynamic Modify
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<MyRoot>.CreateAsync(client))
                    {
                        INode root = consumer.Root;

                        // Navigate to the parameters we're interested in.
                        var sapphire = (INode)root.Children.First(c => c.Identifier == "Sapphire");
                        var sources = (INode)sapphire.Children.First(c => c.Identifier == "Sources");
                        var fpgm1 = (INode)sources.Children.First(c => c.Identifier == "FPGM 1");
                        var fader = (INode)fpgm1.Children.First(c => c.Identifier == "Fader");
                        var level = (IParameter)fader.Children.First(c => c.Identifier == "dB Value");
                        var position = (IParameter)fader.Children.First(c => c.Identifier == "Position");

                        // Set parameters to the desired values.
                        level.Value = -67.0;
                        position.Value = 128L;

                        // We send the changes back to the provider with the call below. Here, this is necessary so that
                        // the changes are sent before Dispose is called on the consumer. In a real-world application
                        // however, SendAsync often does not need to be called explicitly because it is automatically
                        // called every 100ms as long as there are pending changes. See AutoSendInterval for more
                        // information.
                        await consumer.SendAsync();
                    }
                }, cancelToken);
            #endregion
        }

        /// <summary>Waits for the connection to be lost.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void ConnectionLostTest()
        {
            #region Connection Lost
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<MyRoot>.CreateAsync(client))
                    {
                        var connectionLost = new TaskCompletionSource<Exception>();
                        consumer.ConnectionLost += (s, e) => connectionLost.SetResult(e.Exception);

                        Console.WriteLine("Waiting for the provider to disconnect...");
                        var exception = await connectionLost.Task;
                        Console.WriteLine("Connection Lost!");
                        Console.WriteLine("Exception:{0}{1}", exception, Environment.NewLine);
                    }
                }, cancelToken);
            #endregion
        }

        /// <summary>Iterates over the static local database.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void StaticIterateTest()
        {
            #region Static Iterate
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<SapphireRoot>.CreateAsync(client))
                    {
                        WriteChildren(consumer.Root, 0);
                    }
                }, cancelToken);
            #endregion
        }

        /// <summary>Demonstrates how to react to changes with the static interface.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void StaticReactToChangesTest()
        {
            #region Static React to Changes
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<SapphireRoot>.CreateAsync(client))
                    {
                        var valueChanged = new TaskCompletionSource<string>();
                        var positionParameter = consumer.Root.Sapphire.Sources.Fpgm1.Fader.Position;
                        positionParameter.PropertyChanged += (s, e) => valueChanged.SetResult(((IElement)s).GetPath());
                        Console.WriteLine("Waiting for the parameter to change...");
                        Console.WriteLine("A value of the element with the path {0} has been changed.", await valueChanged.Task);
                    }
                }, cancelToken);
            #endregion
        }

        /// <summary>Modifies parameters in the dynamic local database.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void StaticModifyTest()
        {
            #region Static Modify
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<SapphireRoot>.CreateAsync(client))
                    {
                        var fader = consumer.Root.Sapphire.Sources.Fpgm1.Fader;
                        fader.DBValue.Value = -67.0;
                        fader.Position.Value = 128;
                        await consumer.SendAsync();
                    }
                }, cancelToken);
            #endregion
        }

        /// <summary>Tests <see cref="CollectionNode{T}"/>.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void CollectionNodeTest()
        {
            #region Collection Node
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<UnboundedSapphireRoot>.CreateAsync(client))
                    {
                        foreach (var source in consumer.Root.Sapphire.Sources.Children)
                        {
                            Console.WriteLine(source.Fader.Position.Value);
                        }
                    }
                }, cancelToken);
            #endregion
        }

        /// <summary>Iterates over the mixed local database.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void MixedIterateTest()
        {
            #region Mixed Iterate
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<MixedSapphireRoot>.CreateAsync(client))
                    {
                        WriteChildren(consumer.Root, 0);
                    }
                }, cancelToken);
            #endregion
        }

        /// <summary>Modifies parameters in the mixed local database.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void MixedModifyTest()
        {
            #region Mixed Modify
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = await ConnectAsync("localhost", 9000))
                    using (var consumer = await Consumer<MixedSapphireRoot>.CreateAsync(client))
                    {
                        foreach (var source in consumer.Root.Sapphire.Sources.Children)
                        {
                            source.Fader.DBValue.Value = -67.0;
                            source.Fader.Position.Value = 128;
                            source.Dsp.Input.LRMode.Value = LRMode.Mono;
                            source.Dsp.Input.Phase.Value = false;
                        }

                        await consumer.SendAsync();
                    }
                }, cancelToken);
            #endregion
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region MainTutorial Method
        private static void MainTutorial()
        {
            // This is necessary so that we can execute async code in a console application.
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    // Establish S101 protocol
                    using (S101Client client = await ConnectAsync("localhost", 9000))

                    // Retrieve *all* elements in the provider database and store them in a local copy
                    using (Consumer<MyRoot> consumer = await Consumer<MyRoot>.CreateAsync(client))
                    {
                        // Get the root of the local database.
                        INode root = consumer.Root;

                        // For now just output the number of direct children under the root node.
                        Console.WriteLine(root.Children.Count);
                    }
                }, cancelToken);
        }
        #endregion

        #region S101 Connect Method
        private static async Task<S101Client> ConnectAsync(string host, int port)
        {
            // Create TCP connection
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);

            // Establish S101 protocol
            // S101 provides message packaging, CRC integrity checks and a keep-alive mechanism.
            var stream = tcpClient.GetStream();
            return new S101Client(tcpClient, stream.ReadAsync, stream.WriteAsync);
        }
        #endregion

        #region Write Children
        private static void WriteChildren(INode node, int depth)
        {
            var indent = new string(' ', 2 * depth);

            foreach (var child in node.Children)
            {
                var childNode = child as INode;

                if (childNode != null)
                {
                    Console.WriteLine("{0}Node {1}", indent, child.Identifier);
                    WriteChildren(childNode, depth + 1);
                }
                else
                {
                    var childParameter = child as IParameter;

                    if (childParameter != null)
                    {
                        Console.WriteLine("{0}Parameter {1}: {2}", indent, child.Identifier, childParameter.Value);
                    }
                }
            }
        }
        #endregion

        #region Dynamic Root Class
        // Note that the most-derived subtype MyRoot needs to be passed to the generic base class.
        private sealed class MyRoot : DynamicRoot<MyRoot>
        {
        }
        #endregion

#pragma warning disable SA1201 // Elements must appear in the correct order. Enumeration must appear last to be consistent with order of other types.
        #region Static Database Types
        private sealed class SapphireRoot : Root<SapphireRoot>
        {
            internal Sapphire Sapphire { get; private set; }
        }

        private sealed class Sapphire : FieldNode<Sapphire>
        {
            internal Sources Sources { get; private set; }
        }

        private sealed class Sources : FieldNode<Sources>
        {
            [Element(Identifier = "FPGM 1")]
            internal Source Fpgm1 { get; private set; }

            [Element(Identifier = "FPGM 2")]
            internal Source Fpgm2 { get; private set; }
        }

        private sealed class Source : FieldNode<Source>
        {
            internal Fader Fader { get; private set; }

            [Element(Identifier = "DSP")]
            internal Dsp Dsp { get; private set; }
        }

        private sealed class Fader : FieldNode<Fader>
        {
            [Element(Identifier = "dB Value")]
            internal RealParameter DBValue { get; private set; }

            internal IntegerParameter Position { get; private set; }
        }

        private sealed class Dsp : FieldNode<Dsp>
        {
            internal Input Input { get; private set; }
        }

        private sealed class Input : FieldNode<Input>
        {
            internal BooleanParameter Phase { get; private set; }

            [Element(Identifier = "LR Mode")]
            internal EnumParameter<LRMode> LRMode { get; private set; }
        }

        private enum LRMode
        {
            Stereo,

            RightToBoth,

            Side,

            LeftToBoth,

            Mono,

            MidSideToXY
        }
        #endregion
#pragma warning restore SA1201 // Elements must appear in the correct order

        #region Unbounded Database Types
        private sealed class UnboundedSapphireRoot : Root<UnboundedSapphireRoot>
        {
            internal UnboundedSapphire Sapphire { get; private set; }
        }

        private sealed class UnboundedSapphire : FieldNode<UnboundedSapphire>
        {
            internal CollectionNode<Source> Sources { get; private set; }
        }
        #endregion

        #region Optional Fader Source
        private sealed class OptionalFaderSource : FieldNode<OptionalFaderSource>
        {
            [Element(IsOptional = true)]
            internal Fader Fader { get; private set; }

            [Element(Identifier = "DSP")]
            internal Dsp Dsp { get; private set; }
        }
        #endregion

        #region Mixed Database Types
        // Subclassing Root means that the Children collection of this node will only contain the elements declared
        // with properties, in this case a single node with the identifier Sapphire, which is also accessible through
        // the property.
        private sealed class MixedSapphireRoot : Root<MixedSapphireRoot>
        {
            internal MixedSapphire Sapphire { get; private set; }
        }

        // Subclassing DynamicFieldNode means that the Children collection of this node will contain *all* elements
        // reported by the provider. Additionally, the node with the identifier Sources is also accessible through the
        // property.
        private sealed class MixedSapphire : DynamicFieldNode<MixedSapphire>
        {
            internal CollectionNode<MixedSource> Sources { get; private set; }
        }

        // Subclassing DynamicFieldNode means that the Children collection of this node will contain *all* elements
        // reported by the provider. Additionally, the nodes Fader and Dsp are also accessible through their
        // respective properties. The Fader and Dsp types themselves derive from FieldNode, so their Children
        // collections will only contain the parameters declared as properties.
        private sealed class MixedSource : DynamicFieldNode<MixedSource>
        {
            internal Fader Fader { get; private set; }

            [Element(Identifier = "DSP")]
            internal Dsp Dsp { get; private set; }
        }
        #endregion
    }
#pragma warning restore SA1515 // Single-line comment must be preceded by blank line
#pragma warning restore SA1124 // Do not use regions
#pragma warning restore SA1123 // Do not place regions within elements
}
