////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    using Ember;
    using EmberLib;
    using EmberLib.Framing;
    using EmberLib.Glow.Framing;
    using Glow;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Reflection;
    using S101;
    using Test;
    using Test.EmberDataPayloads;
    using Threading.Tasks;

    /// <summary>Tests <see cref="Consumer{T}"/>.</summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Test code.")]
    [TestClass]
    public class ConsumerTest : CommunicationTestBase
    {
        /// <summary>Measures <see cref="Consumer{T}.CreateAsync(S101Client)"/> performance with a message containing a
        /// big tree.</summary>
        [TestMethod]
        public void BigTreeAssemblyTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    // Make sure everything is JITed.
                    await BigTreeAssemblyTestCore<BigTreeDynamicRoot>(1, true);
                    await BigTreeAssemblyTestCore<BigTreeStaticRoot>(1, false);

                    await BigTreeAssemblyTestCore<BigTreeDynamicRoot>(6, false);
                    await BigTreeAssemblyTestCore<BigTreeStaticRoot>(6, false);
                });
        }

        /// <summary>Tests the main <see cref="Consumer{T}"/> use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            AsyncPump.Run(() => TestConsumerAfterFirstRequest<MainRoot>(
                async (consumerTask, providerClient) =>
                {
                    var boolean = this.GetRandomBoolean();
                    var integer = (long)this.Random.Next(int.MinValue, int.MaxValue);
                    var factorInteger = (long)this.Random.Next(int.MinValue, int.MaxValue);
                    var formulaInteger = (long)this.Random.Next(int.MinValue, int.MaxValue);
                    var enumeration = (Enumeration)this.Random.Next(4);
                    var enumMap = (Enumeration)this.Random.Next(4);
                    var octetstring = new byte[this.Random.Next(512)];
                    this.Random.NextBytes(octetstring);
                    var real = this.Random.NextDouble();
                    var stringValue = GetRandomString();

                    var requestTask = WaitForRequest(providerClient, "MainRequest2.xml");
                    await providerClient.SendOutOfFrameByteAsync(0x01);
                    await SendResponse(
                        providerClient,
                        "MainResponse1.xml",
                        boolean.ToString().ToLowerInvariant(),
                        integer,
                        factorInteger,
                        formulaInteger,
                        (int)enumeration,
                        (int)enumMap,
                        new SoapHexBinary(octetstring).ToString(),
                        real,
                        stringValue);
                    await requestTask;

                    var children = Enumerable.Range(0, 3).Select(i => this.GetRandomBoolean()).ToArray();
                    await SendResponse(
                        providerClient,
                        "MainResponse2.xml",
                        integer,
                        children[0].ToString().ToLowerInvariant(),
                        children[1].ToString().ToLowerInvariant(),
                        children[2].ToString().ToLowerInvariant());

                    await MonitorConnection(
                        consumerTask,
                        async consumer =>
                        {
                            unchecked
                            {
                                boolean = !boolean;
                                ++factorInteger;
                                ++formulaInteger;
                                enumeration = (Enumeration)(((int)enumeration + 1) % 4);
                                enumMap = (Enumeration)(((int)enumMap + 1) % 4);
                                this.Random.NextBytes(octetstring);
                                real = this.Random.NextDouble();
                                stringValue = GetRandomString();
                                ++integer;

                                for (var index = 0; index < children.Length; ++index)
                                {
                                    children[index] = !children[index];
                                }
                            }

                            var root = consumer.Root;

                            CheckValues(root);

                            AssertThrow<ArgumentException>(() => ((IParameter)root.BooleanParameter).Value = 0);
                            AssertNotified((IParameter)root.BooleanParameter, o => o.Value, boolean);
                            AssertNotified((IParameter)root.IntegerParameter, o => o.Value, integer);
                            AssertNotified((IParameter)root.FactorIntegerParameter, o => o.Value, factorInteger);
                            AssertNotified((IParameter)root.FormulaIntegerParameter, o => o.Value, formulaInteger);
                            AssertNotified((IParameter)root.EnumerationParameter, o => o.Value, enumeration);
                            AssertNotified((IParameter)root.EnumMapParameter, o => o.Value, enumMap);
                            AssertThrow<ArgumentNullException>(() => ((IParameter)root.OctetstringParameter).Value = null);
                            AssertNotified((IParameter)root.OctetstringParameter, o => o.Value, octetstring);
                            AssertNotified((IParameter)root.RealParameter, o => o.Value, real);
                            AssertNotified((IParameter)root.StringParameter, o => o.Value, stringValue);
                            AssertNotified((IParameter)root.FieldNode.SomeParameter, o => o.Value, integer);

                            CheckCollections(root, children);

                            await WaitForRequest(
                                providerClient,
                                "MainRequest3.xml",
                                boolean.ToString().ToLowerInvariant(),
                                integer,
                                factorInteger,
                                formulaInteger,
                                (int)enumeration,
                                (int)enumMap,
                                new SoapHexBinary(octetstring).ToString(),
                                real,
                                stringValue,
                                children[0].ToString().ToLowerInvariant(),
                                children[1].ToString().ToLowerInvariant(),
                                children[2].ToString().ToLowerInvariant());

                            AssertNotified(root.StringParameter, o => o.Value, GetRandomString());

                            CheckNumbers(root);

                            await SendResponse(providerClient, "StreamEntriesResponse.xml");
                        });
                },
                null,
                new S101Logger(GlowTypes.Instance, Console.Out)));
        }

        /// <summary>Tests the dynamic use cases.</summary>
        [TestMethod]
        public void DynamicTest()
        {
            AsyncPump.Run(() => TestConsumerAfterFirstRequest<DynamicTestRoot>(
                async (consumerTask, providerClient) =>
                {
                    var boolean = this.GetRandomBoolean();
                    var integer = (long)this.Random.Next(int.MinValue, int.MaxValue);
                    var factorInteger = (long)this.Random.Next(int.MinValue, int.MaxValue);
                    var formulaInteger = (long)this.Random.Next(int.MinValue, int.MaxValue);
                    var enumeration = (long)this.Random.Next(4);
                    var enumMap = (long)this.Random.Next(4);
                    var octetstring = new byte[this.Random.Next(512)];
                    this.Random.NextBytes(octetstring);
                    var real = this.Random.NextDouble();
                    var stringValue = GetRandomString();

                    var requestTask = WaitForRequest(providerClient, "MainRequest2.xml");
                    await SendResponse(
                        providerClient,
                        "MainResponse1.xml",
                        boolean.ToString().ToLowerInvariant(),
                        integer,
                        factorInteger,
                        formulaInteger,
                        enumeration,
                        enumMap,
                        new SoapHexBinary(octetstring).ToString(),
                        real,
                        stringValue);
                    await requestTask;

                    var children = Enumerable.Range(0, 3).Select(i => this.GetRandomBoolean()).ToArray();
                    await SendResponse(
                        providerClient,
                        "MainResponse2.xml",
                        integer,
                        children[0].ToString().ToLowerInvariant(),
                        children[1].ToString().ToLowerInvariant(),
                        children[2].ToString().ToLowerInvariant());

                    await MonitorConnection(
                        consumerTask,
                        async consumer =>
                        {
                            unchecked
                            {
                                boolean = !boolean;
                                ++factorInteger;
                                ++formulaInteger;
                                enumeration = (enumeration + 1) % 4;
                                enumMap = (enumMap + 1) % 4;
                                this.Random.NextBytes(octetstring);
                                real = this.Random.NextDouble();
                                stringValue = GetRandomString();
                                ++integer;

                                for (var index = 0; index < children.Length; ++index)
                                {
                                    children[index] = !children[index];
                                }
                            }

                            var root = consumer.Root;

                            Assert.IsNull(root.Parent);
                            Assert.AreEqual(string.Empty, root.Identifier);
                            Assert.IsNull(root.Description);
                            Assert.IsTrue(root.GetPath() == "/");
                            Assert.AreEqual(true, root.IsRoot);
                            Assert.AreEqual(true, root.IsOnline);
                            Assert.IsNull(root.Tag);
                            var randomValue = new Random().Next();
                            root.Tag = randomValue;
                            Assert.AreEqual(randomValue, root.Tag);

                            Assert.AreEqual(null, ((IParameter)root.GetChild("OctetstringParameter")).Minimum);
                            Assert.AreEqual(null, ((IParameter)root.GetChild("OctetstringParameter")).Maximum);
                            Assert.AreEqual(null, ((IParameter)root.GetChild("OctetstringParameter")).Factor);
                            Assert.AreEqual(null, ((IParameter)root.GetChild("OctetstringParameter")).Formula);
                            Assert.AreEqual(null, ((IParameter)root.GetChild("OctetstringParameter")).EnumMap);
                            Assert.AreEqual(null, ((IParameter)root.GetChild("StringParameter")).Minimum);
                            Assert.AreEqual(null, ((IParameter)root.GetChild("StringParameter")).Maximum);
                            Assert.AreEqual(null, ((IParameter)root.GetChild("StringParameter")).Factor);
                            Assert.AreEqual(null, ((IParameter)root.GetChild("StringParameter")).Formula);
                            Assert.AreEqual(null, ((IParameter)root.GetChild("StringParameter")).EnumMap);

                            AssertThrow<ArgumentException>(() => ((IParameter)root.GetChild("BooleanParameter")).Value = 0);
                            AssertNotified((IParameter)root.GetChild("BooleanParameter"), o => o.Value, boolean);
                            AssertNotified((IParameter)root.GetChild("IntegerParameter"), o => o.Value, integer);
                            AssertNotified((IParameter)root.GetChild("FactorIntegerParameter"), o => o.Value, factorInteger);
                            AssertNotified((IParameter)root.GetChild("FormulaIntegerParameter"), o => o.Value, formulaInteger);
                            AssertNotified((IParameter)root.GetChild("EnumerationParameter"), o => o.Value, enumeration);
                            AssertNotified((IParameter)root.GetChild("EnumMapParameter"), o => o.Value, enumMap);
                            AssertThrow<ArgumentNullException>(() => ((IParameter)root.GetChild("OctetstringParameter")).Value = null);
                            AssertNotified((IParameter)root.GetChild("OctetstringParameter"), o => o.Value, octetstring);
                            AssertNotified((IParameter)root.GetChild("RealParameter"), o => o.Value, real);
                            AssertNotified((IParameter)root.GetChild("StringParameter"), o => o.Value, stringValue);
                            AssertNotified((IParameter)root.FieldNode.GetChild("SomeParameter"), o => o.Value, integer);

                            var original = ((Enumeration[])Enum.GetValues(typeof(Enumeration))).Select(
                                v => new KeyValuePair<string, int>(v.ToString(), (int)v)).OrderBy(p => p.Value).ToArray();
                            CollectionAssert.AreEqual(
                                original, ((IParameter)root.GetChild("EnumerationParameter")).EnumMap.OrderBy(p => p.Value).ToArray());
                            CollectionAssert.AreEqual(
                                original, ((IParameter)root.GetChild("EnumMapParameter")).EnumMap.OrderBy(p => p.Value).ToArray());

                            var collection = ((INode)root.GetChild("CollectionNode")).Children;

                            for (var index = 0; index < children.Length; ++index)
                            {
                                ((IParameter)collection[index]).Value = children[index];
                            }

                            await WaitForRequest(
                                providerClient,
                                "MainRequest3.xml",
                                boolean.ToString().ToLowerInvariant(),
                                integer,
                                factorInteger,
                                formulaInteger,
                                (int)enumeration,
                                (int)enumMap,
                                new SoapHexBinary(octetstring).ToString(),
                                real,
                                stringValue,
                                children[0].ToString().ToLowerInvariant(),
                                children[1].ToString().ToLowerInvariant(),
                                children[2].ToString().ToLowerInvariant());

                            AssertNotified((IParameter)root.GetChild("StringParameter"), o => o.Value, GetRandomString());

                            await SendResponse(providerClient, "StreamEntriesResponse.xml");
                        });
                },
                null,
                new S101Logger(GlowTypes.Instance, Console.Out)));
        }

        /// <summary>Tests whether interface-typed elements work correctly.</summary>
        [TestMethod]
        public void InterfaceElementTest()
        {
            var mainRoot = GetRoot<MainRoot>("MainLog.xml");
            AssertEqual(mainRoot, GetRoot<InterfaceElementRoot<INode>>("MainLog.xml"));
            AssertEqual(mainRoot, GetRoot<InterfaceElementRoot<CollectionNode<IParameter>>>("MainLog.xml"));
        }

        /// <summary>Tests whether <see cref="INode.GetElement"/> works correctly.</summary>
        [TestMethod]
        public void GetElementTest()
        {
            var mainRoot = GetRoot<MainRoot>("MainLog.xml");
            AssertThrow<ArgumentNullException>(() => mainRoot.GetElement(null));
            Assert.AreEqual(mainRoot, mainRoot.GetElement(string.Empty));
            Assert.AreEqual(mainRoot, mainRoot.GetElement(mainRoot.GetPath()));
            Assert.AreEqual(mainRoot.BooleanParameter, mainRoot.GetElement(mainRoot.BooleanParameter.GetPath()));
            Assert.AreEqual(mainRoot.CollectionNode, mainRoot.GetElement(mainRoot.CollectionNode.GetPath()));
            Assert.AreEqual(mainRoot.CollectionNode.Children[0], mainRoot.GetElement(mainRoot.CollectionNode.Children[0].GetPath()));
            Assert.AreEqual(mainRoot.CollectionNode.Children[0], mainRoot.CollectionNode.GetElement("_1"));
        }

        /// <summary>Tests whether property values are transmitted correctly.</summary>
        [TestMethod]
        public void PropertiesTest()
        {
            AsyncPump.Run(() => TestConsumerAfterFirstRequest<PropertiesRoot>(
                async (consumerTask, providerClient) =>
                {
                    var description = GetRandomString();
                    var value = (long)this.Random.Next(4);
                    var minimum = (long)this.Random.Next(int.MinValue, 0);
                    var maximum = (long)this.Random.Next(4, int.MaxValue);
                    var access = this.GetRandomEnum<ParameterAccess>();
                    var format = GetRandomString();
                    var factor = this.Random.Next(int.MinValue, int.MaxValue);
                    var isOnline = true;
                    var defaultValue = (long)this.Random.Next(4);
                    var streamIdentifier = this.Random.Next();
                    var streamFormat = this.GetRandomEnum<StreamFormat>();
                    var streamOffset = this.Random.Next();
                    var formula = GetRandomString();
                    var enumStrings =
                        new[] { GetRandomString(), GetRandomString(), GetRandomString(), GetRandomString() };
                    var enumValues = Enumerable.Range(0, 4).OrderBy(v => this.Random.Next()).ToArray();
                    var isRoot = this.GetRandomBoolean();
                    var schemaIdentifier1 = GetRandomString();
                    var schemaIdentifier2 = GetRandomString();

                    await SendResponse(
                        providerClient,
                        "PropertiesResponse1.xml",
                        description,
                        value,
                        minimum,
                        maximum,
                        (int)access,
                        format,
                        factor,
                        isOnline.ToString().ToLowerInvariant(),
                        defaultValue,
                        streamIdentifier,
                        (int)streamFormat,
                        streamOffset,
                        formula,
                        enumStrings[0],
                        enumStrings[1],
                        enumStrings[2],
                        enumStrings[3],
                        enumValues[0],
                        enumValues[1],
                        enumValues[2],
                        enumValues[3],
                        isRoot.ToString().ToLowerInvariant(),
                        schemaIdentifier1,
                        schemaIdentifier2);

                    await MonitorConnection(
                        consumerTask,
                        consumer =>
                        {
                            var e1 = consumer.Root.FactorIntegerParameter;
                            var e2 = consumer.Root.FormulaIntegerParameter;
                            var e3 = consumer.Root.EnumerationParameter;
                            var e4 = consumer.Root.EnumMapParameter;
                            var e5 = consumer.Root.FieldNode;

                            Assert.AreEqual("FactorIntegerParameter", e1.Identifier);
                            Assert.AreEqual("FormulaIntegerParameter", e2.Identifier);
                            Assert.AreEqual("EnumerationParameter", e3.Identifier);
                            Assert.AreEqual("EnumMapParameter", e4.Identifier);
                            Assert.AreEqual("FieldNode", e5.Identifier);
                            AssertAreEqual(consumer.Root, (IElement e) => e.Parent, e1, e2, e3, e4, e5);
                            AssertAreEqual(description, (IElement e) => e.Description, e1, e2, e3, e4, e5);

                            if ((access & ParameterAccess.Read) != 0)
                            {
                                AssertAreEqual(value, (IParameter e) => e.Value, e1, e2, e3, e4);
                            }

                            AssertAreEqual(minimum, (IParameter e) => e.Minimum, e1, e2);
                            AssertAreEqual(maximum, (IParameter e) => e.Maximum, e1, e2);
                            AssertAreEqual(access, (IParameter e) => e.Access, e1, e2, e3, e4);
                            AssertAreEqual(format, (IParameter e) => e.Format, e1, e2);
                            AssertAreEqual(factor, (IParameter e) => e.Factor, e1);
                            AssertAreEqual(isOnline, (IParameter e) => e.IsOnline, e1, e2, e3, e4);
                            AssertAreEqual(isOnline, (INode e) => e.IsOnline, e5);
                            AssertAreEqual(defaultValue, (IParameter e) => e.DefaultValue, e1, e2, e3, e4);
                            AssertAreEqual(ParameterType.Integer, (IParameter e) => e.Type, e1, e2);
                            AssertAreEqual(ParameterType.Enum, (IParameter e) => e.Type, e3, e4);
                            AssertAreEqual(formula, (IParameter e) => e.Formula, e2);
                            AssertEnumMapEquality(enumStrings, new[] { 0, 1, 3, 4 }, e3.EnumMap);
                            AssertEnumMapEquality(enumStrings, enumValues, ((IParameter)e4).EnumMap);
                            AssertAreEqual(isRoot, (INode e) => e.IsRoot, e5);
                            AssertAreEqual(schemaIdentifier1, (IElementWithSchemas e) => e.SchemaIdentifiers[0], e1, e2, e3, e4, e5);
                            AssertAreEqual(schemaIdentifier2, (IElementWithSchemas e) => e.SchemaIdentifiers[1], e1, e2, e3, e4, e5);
                            return Task.FromResult(false);
                        });
                },
                null,
                new S101Logger(GlowTypes.Instance, Console.Out)));
        }

        /// <summary>Tests whether the provider access restrictions are enforced correctly.</summary>
        [TestMethod]
        public void AccessTest()
        {
            AsyncPump.Run(() => TestWithRobot<SingleNodeRoot>(
                consumer =>
                {
                    AssertAccess(consumer.Root.FieldNode);
                    AssertAccess(consumer.Root.FieldNode.FieldNode);
                    AssertAccess(consumer.Root.FieldNode.FieldNode.FieldNode);
                    AssertAccess(consumer.Root.FieldNode.FieldNode.FieldNode.FieldNode);
                    return Task.FromResult(false);
                },
                false,
                "AccessLog.xml"));
        }

        /// <summary>Tests whether <see cref="INode.ChildrenRetrievalPolicy"/> works as expected.</summary>
        [TestMethod]
        public void ChildrenRetrievalPolicyTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    await StaticChildrenRetrievalPolicyTestAsync(
                        ChildrenRetrievalPolicy.None, "ChildrenRetrievalPolicyLog1.xml");
                    await StaticChildrenRetrievalPolicyTestAsync(
                        ChildrenRetrievalPolicy.DirectOnly, "ChildrenRetrievalPolicyLog2.xml");
                    await StaticChildrenRetrievalPolicyTestAsync(
                        ChildrenRetrievalPolicy.All, "ChildrenRetrievalPolicyLog3.xml");
                    await this.DynamicChildrenRetrievalPolicyTestAsync(false);
                    await this.DynamicChildrenRetrievalPolicyTestAsync(true);
                });
        }

        /// <summary>Tests nullable parameter variants.</summary>
        [TestMethod]
        public void NullableTest()
        {
            AsyncPump.Run(() => TestConsumerAfterFirstRequest<NullableRoot>(
                async (consumerTask, providerClient) =>
                {
                    var integerMin = (long)this.Random.Next(int.MinValue, 0);
                    var integerMax = (long)this.Random.Next();
                    var realMin = -this.Random.NextDouble();
                    var realMax = this.Random.NextDouble();

                    await SendResponse(
                        providerClient, "NullableResponse1.xml", integerMin, integerMax, realMin, realMax);

                    await MonitorConnection(
                        consumerTask,
                        async consumer =>
                        {
                            var root = consumer.Root;
                            Assert.IsNull(root.BooleanParameter.Value);
                            Assert.IsNull(root.IntegerParameter.Value);
                            Assert.AreEqual(integerMin, ((IParameter)root.IntegerParameter).Minimum);
                            Assert.AreEqual(integerMax, ((IParameter)root.IntegerParameter).Maximum);

                            Assert.IsNull(root.EnumParameter.Value);
                            var original = ((Enumeration[])Enum.GetValues(typeof(Enumeration))).Select(
                                v => new KeyValuePair<string, int>(v.ToString(), (int)v)).OrderBy(p => p.Value).ToArray();
                            CollectionAssert.AreEqual(
                                original, ((IParameter)root.EnumParameter).EnumMap.OrderBy(p => p.Value).ToArray());

                            Assert.IsNull(consumer.Root.OctetstringParameter.Value);
                            Assert.IsNull(consumer.Root.RealParameter.Value);
                            Assert.AreEqual(realMin, ((IParameter)root.RealParameter).Minimum);
                            Assert.AreEqual(realMax, ((IParameter)root.RealParameter).Maximum);
                            Assert.IsNull(consumer.Root.StringParameter.Value);

                            AssertNotified(root.BooleanParameter, o => o.Value, false);
                            AssertNotified(root.IntegerParameter, o => o.Value, 0);
                            AssertNotified(root.EnumParameter, o => o.Value, Enumeration.Two);
                            AssertNotified(root.OctetstringParameter, o => o.Value, new byte[0]);
                            AssertNotified(root.RealParameter, o => o.Value, 0.0);
                            AssertNotified(root.StringParameter, o => o.Value, string.Empty);

                            var changed = new TaskCompletionSource<string>();
                            root.StringParameter.PropertyChanged += (s, e) => changed.SetResult(e.PropertyName);

                            await WaitForRequest(providerClient, "NullableRequest2.xml");
                            await SendResponse(providerClient, "NullableResponse2.xml");
                            await changed.Task;
                        });
                },
                null,
                new S101Logger(GlowTypes.Instance, Console.Out)));
        }

        /// <summary>Tests trigger use cases.</summary>
        [TestMethod]
        public void TriggerTest()
        {
            AsyncPump.Run(() => TestWithRobot<EmptyDynamicRoot>(
                async consumer =>
                {
                    foreach (IParameter param in consumer.Root.DynamicChildren)
                    {
                        param.Value = null;
                    }

                    await Task.Delay(300);
                },
                false,
                "TriggerLog.xml"));
        }

        /// <summary>Tests sending/receiving with a broken connection.</summary>
        [TestMethod]
        public void AutoSendWithBrokenS101ConnectionTest()
        {
            AsyncPump.Run(() => TestWithRobot<ModelPayloads>(
                async client =>
                {
                    using (var consumer = await Consumer<EmptyDynamicRoot>.CreateAsync(client))
                    {
                        AssertThrow<ArgumentOutOfRangeException>(() => consumer.AutoSendInterval = -2);
                        Assert.AreEqual(100, consumer.AutoSendInterval);

                        foreach (IParameter param in consumer.Root.DynamicChildren)
                        {
                            param.Value = null;
                        }

                        client.Dispose();
                        await Task.Delay(300);
                    }
                },
                null,
                null,
                GlowTypes.Instance,
                false,
                "SendReceiveWithBrokenConnectionLog.xml"));
        }

        /// <summary>Tests whether exceptions are reported appropriately after
        /// <see cref="Consumer{T}.CreateAsync(S101Client)"/> has returned.</summary>
        [TestMethod]
        public void ThrowAfterCreateTest()
        {
            AsyncPump.Run(() => AssertThrowAsync<S101Exception>(
                async () =>
                {
                    var providerTcpClientTask = WaitForConnectionAsync();
                    using (var consumerClient = await ConnectAsync(-1, null))
                    using (var providerTcpClient = await providerTcpClientTask)
                    using (var providerStream = providerTcpClient.GetStream())
                    using (var providerClient = new S101Client(providerTcpClient, providerStream.ReadAsync, providerStream.WriteAsync))
                    using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        typeof(ModelPayloads), "ThrowAfterCreateLog.xml"))
                    using (var reader = XmlReader.Create(resourceStream))
                    {
                        var robotTask = S101Robot.RunAsync(providerClient, GlowTypes.Instance, reader, false);
                        TaskCompletionSource<bool> connectionLost = new TaskCompletionSource<bool>();

                        using (var consumer = await Consumer<SingleNodeRoot>.CreateAsync(consumerClient))
                        {
                            consumer.ConnectionLost += (s, e) => connectionLost.SetException(e.Exception);
                            await robotTask;

                            // The following bytes represent a valid value update message with the last CRC digit
                            // incremented, such that the CRC check fails.
                            var data = SoapHexBinary.Parse("FE000E00018001020A0260806B80A0806380A003020101A2806480A0806180A003020101A1803180A20302012B0000000000000000000000000000000000000000CE7EFFFE000E00016001020A021354FF").Value;
                            await providerStream.WriteAsync(data, 0, data.Length);
                            await connectionLost.Task;
                        }
                    }
                }));
        }

        /// <summary>Tests whether messages with a different slot are ignored.</summary>
        [TestMethod]
        public void SlotTest()
        {
            AsyncPump.Run(() => TestWithRobot<SingleNodeRoot>(
                async consumer =>
                {
                    Assert.AreEqual(42, consumer.Root.FieldNode.SomeParameter.Value);
                    await consumer.SendAsync();
                },
                false,
                "SlotLog.xml"));
        }

        /// <summary>Tests the various change notifications.</summary>
        [TestMethod]
        public void ChangeNotificationTest()
        {
            AsyncPump.Run(() => TestWithRobot<BooleanRoot>(
                async consumer =>
                {
                    Assert.AreEqual(1, ((INode)consumer.Root).Children.Count);
                    Assert.IsNull(consumer.Root.BooleanParameter);
                    Assert.AreEqual(0, consumer.Root.CollectionNode.Children.Count);

                    TaskCompletionSource<bool> rootChildrenChanged = new TaskCompletionSource<bool>();
                    ((INotifyCollectionChanged)((INode)consumer.Root).Children).CollectionChanged +=
                        (s, e) => rootChildrenChanged.TrySetResult(true);
                    TaskCompletionSource<bool> dynamicRootChildrenChanged = new TaskCompletionSource<bool>();
                    ((INotifyCollectionChanged)consumer.Root.DynamicChildren).CollectionChanged +=
                        (s, e) => dynamicRootChildrenChanged.SetResult(true);

                    TaskCompletionSource<bool> booleanSet = new TaskCompletionSource<bool>();
                    consumer.Root.PropertyChanged +=
                        (s, e) =>
                        {
                            if (e.PropertyName == "BooleanParameter")
                            {
                                booleanSet.SetResult(true);
                            }
                        };

                    TaskCompletionSource<bool> collectionNodeChanged = new TaskCompletionSource<bool>();
                    ((INotifyCollectionChanged)consumer.Root.CollectionNode.Children).CollectionChanged +=
                        (s, e) => collectionNodeChanged.SetResult(true);

                    await rootChildrenChanged.Task;
                    Assert.AreEqual(2, ((INode)consumer.Root).Children.Count);
                    await booleanSet.Task;
                    Assert.IsNotNull(consumer.Root.BooleanParameter);
                    Assert.IsTrue(consumer.Root.BooleanParameter.Value);
                    await collectionNodeChanged.Task;
                    Assert.AreEqual(1, consumer.Root.CollectionNode.Children.Count);
                    Assert.IsTrue(consumer.Root.CollectionNode.Children[0].Value);
                    await dynamicRootChildrenChanged.Task;
                    Assert.AreEqual(3, ((INode)consumer.Root).Children.Count);
                    Assert.AreEqual(1, consumer.Root.DynamicChildren.Count);

                    var dynamicNode = (INode)consumer.Root.DynamicChildren[0];
                    var dynamicParameter = (IParameter)dynamicNode.Children[0];
                    Assert.AreEqual(true, dynamicParameter.Value);
                },
                true,
                "ChangeNotificationLog.xml"));
        }

        /// <summary>Tests Ember+ functions.</summary>
        [TestMethod]
        public void FunctionTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    await TestWithRobot<FunctionRoot>(
                        async consumer =>
                        {
                            var allInteger = Enumerable.Range(1, 6).Select(i => ParameterType.Integer).ToArray();

                            AssertSignature(allInteger.Take(0), allInteger.Take(0), consumer.Root.Function0);
                            AssertSignature(allInteger.Take(1), allInteger.Take(1), consumer.Root.Function1);
                            AssertSignature(
                                new[] { ParameterType.Real, ParameterType.String },
                                new[] { ParameterType.Real, ParameterType.String },
                                consumer.Root.Function2);
                            AssertSignature(
                                new[] { ParameterType.Boolean, ParameterType.Octets, ParameterType.Integer },
                                new[] { ParameterType.Boolean, ParameterType.Octets, ParameterType.Integer },
                                consumer.Root.Function3);
                            AssertSignature(allInteger.Take(4), allInteger.Take(4), consumer.Root.Node.Function4);
                            AssertSignature(allInteger.Take(5), allInteger.Take(5), consumer.Root.Node.Function5);
                            AssertSignature(allInteger.Take(6), allInteger.Take(6), consumer.Root.Node.Function6);

                            consumer.AutoSendInterval = 10;
                            await consumer.Root.Function0.InvokeAsync();
                            Assert.AreEqual(42, (await consumer.Root.Function1.InvokeAsync(42)).Item1);
                            var result2 = await consumer.Root.Function2.InvokeAsync(3.14159265359, "Hello there.");
                            Assert.AreEqual(3.14159265359, result2.Item1);
                            Assert.AreEqual("Hello there.", result2.Item2);
                            var result3 = await consumer.Root.Function3.InvokeAsync(true, new byte[] { 42, 42 }, 13);
                            Assert.AreEqual(true, result3.Item1);
                            CollectionAssert.AreEqual(new byte[] { 42, 42 }, result3.Item2);
                            Assert.AreEqual(13, result3.Item3);
                            var result4 = await consumer.Root.Node.Function4.InvokeAsync(1, 2, 3, 4);
                            Assert.AreEqual(1, result4.Item1);
                            Assert.AreEqual(2, result4.Item2);
                            Assert.AreEqual(3, result4.Item3);
                            Assert.AreEqual(4, result4.Item4);
                            var result5 = await consumer.Root.Node.Function5.InvokeAsync(1, 2, 3, 4, 5);
                            Assert.AreEqual(1, result5.Item1);
                            Assert.AreEqual(2, result5.Item2);
                            Assert.AreEqual(3, result5.Item3);
                            Assert.AreEqual(4, result5.Item4);
                            Assert.AreEqual(5, result5.Item5);
                            var result6 = await consumer.Root.Node.Function6.InvokeAsync(1, 2, 3, 4, 5, 6);
                            Assert.AreEqual(1, result6.Item1);
                            Assert.AreEqual(2, result6.Item2);
                            Assert.AreEqual(3, result6.Item3);
                            Assert.AreEqual(4, result6.Item4);
                            Assert.AreEqual(5, result6.Item5);
                            Assert.AreEqual(6, result6.Item6);

                            try
                            {
                                await consumer.Root.Function1.InvokeAsync(42);
                            }
                            catch (InvocationFailedException ex)
                            {
                                Assert.AreEqual(42L, ex.Result.Items.First());
                                Assert.AreEqual(1, ex.Result.Items.Count());
                            }
                        },
                        true,
                        "FunctionLog.xml");
                });
        }

        /// <summary>Tests dynamic Ember+ functions.</summary>
        [TestMethod]
        public void DynamicFunctionTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    await TestWithRobot<EmptyDynamicRoot>(
                        async consumer =>
                        {
                            INode root = consumer.Root;
                            var function0 = (IFunction)root.Children.FirstOrDefault(e => e.Identifier == "Function0");
                            var function1 = (IFunction)root.Children.FirstOrDefault(e => e.Identifier == "Function1");
                            var function2 = (IFunction)root.Children.FirstOrDefault(e => e.Identifier == "Function2");
                            var function3 = (IFunction)root.Children.FirstOrDefault(e => e.Identifier == "Function3");
                            var node = (INode)root.Children.FirstOrDefault(e => e.Identifier == "Node");
                            var function4 = (IFunction)node.Children.FirstOrDefault(e => e.Identifier == "Function4");
                            var function5 = (IFunction)node.Children.FirstOrDefault(e => e.Identifier == "Function5");
                            var function6 = (IFunction)node.Children.FirstOrDefault(e => e.Identifier == "Function6");

                            var allInteger = Enumerable.Range(1, 6).Select(i => ParameterType.Integer).ToArray();

                            AssertSignature(allInteger.Take(0), allInteger.Take(0), function0);
                            AssertSignature(allInteger.Take(1), allInteger.Take(1), function1);
                            AssertSignature(
                                new[] { ParameterType.Real, ParameterType.String },
                                new[] { ParameterType.Real, ParameterType.String },
                                function2);
                            AssertSignature(
                                new[] { ParameterType.Boolean, ParameterType.Octets, ParameterType.Integer },
                                new[] { ParameterType.Boolean, ParameterType.Octets, ParameterType.Integer },
                                function3);
                            AssertSignature(allInteger.Take(4), allInteger.Take(4), function4);
                            AssertSignature(allInteger.Take(5), allInteger.Take(5), function5);
                            AssertSignature(allInteger.Take(6), allInteger.Take(6), function6);

                            await AssertThrowAsync<ArgumentException>(
                                () => function0.InvokeAsync(0),
                                "The number of actual arguments is not equal to the number of expected arguments.");
                            await AssertThrowAsync<ArgumentException>(
                                () => function1.InvokeAsync(),
                                "The number of actual arguments is not equal to the number of expected arguments.");
                            await AssertThrowAsync<ArgumentException>(
                                () => function1.InvokeAsync(13),
                                "The type of at least one actual argument is not equal to the expected type.");

                            consumer.AutoSendInterval = 10;
                            var result0 = await function0.InvokeAsync();
                            Assert.AreEqual(0, result0.Items.Count());
                            var result1 = await function1.InvokeAsync(42L);
                            Assert.AreEqual(1, result1.Items.Count());
                            Assert.AreEqual(42L, result1.Items.First());
                            var result2 = await function2.InvokeAsync(3.14159265359, "Hello there.");
                            Assert.AreEqual(2, result2.Items.Count());
                            Assert.AreEqual(3.14159265359, result2.Items.First());
                            Assert.AreEqual("Hello there.", result2.Items.Skip(1).First());
                            var result3 = await function3.InvokeAsync(true, new byte[] { 42, 42 }, 13L);
                            Assert.AreEqual(3, result3.Items.Count());
                            Assert.AreEqual(true, result3.Items.First());
                            CollectionAssert.AreEqual(new byte[] { 42, 42 }, (byte[])result3.Items.Skip(1).First());
                            Assert.AreEqual(13L, result3.Items.Skip(2).First());
                            var expected = Enumerable.Range(1, 6).Select(i => (long)i);

                            CollectionAssert.AreEqual(
                                expected.Take(4).ToArray(),
                                (await function4.InvokeAsync(1L, 2L, 3L, 4L)).Items.Select(o => (long)o).ToArray());

                            CollectionAssert.AreEqual(
                                expected.Take(5).ToArray(),
                                (await function5.InvokeAsync(1L, 2L, 3L, 4L, 5L)).Items.Select(o => (long)o).ToArray());

                            CollectionAssert.AreEqual(
                                expected.Take(6).ToArray(),
                                (await function6.InvokeAsync(1L, 2L, 3L, 4L, 5L, 6L)).Items.Select(o => (long)o).ToArray());

                            try
                            {
                                await function1.InvokeAsync(42L);
                            }
                            catch (InvocationFailedException ex)
                            {
                                Assert.AreEqual(42L, ex.Result.Items.First());
                                Assert.AreEqual(1, ex.Result.Items.Count());
                            }
                        },
                        false,
                        "FunctionLog.xml");
                });
        }

        /// <summary>Tests the main use cases of Ember+ matrices.</summary>
        [TestMethod]
        public void MatrixMainTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    await TestWithRobot<MatrixRoot>(
                        async consumer =>
                        {
                            var matrix = consumer.Root.Sdn.Switching.Matrix0.Matrix;
                            Assert.AreEqual("Matrix-0", matrix.Identifier);
                            Assert.AreEqual("Matrix 0", matrix.Description);
                            Assert.AreEqual(4, matrix.MaximumTotalConnects);
                            Assert.AreEqual(1, matrix.MaximumConnectsPerTarget);
                            CollectionAssert.AreEqual(new[] { 1, 1, 0, 2000 }, matrix.ParametersLocation?.ToArray());
                            Assert.AreEqual(17, matrix.GainParameterNumber);
                            Assert.AreEqual(1, matrix.Labels?.Count);
                            Assert.AreEqual("Primary", matrix.Labels[0].Description);
                            CollectionAssert.AreEqual(new[] { 1, 1, 0, 1000, 1 }, matrix.Labels[0].BasePath.ToArray());
                            Assert.AreEqual(1, matrix.SchemaIdentifiers?.Count);
                            Assert.AreEqual("com.company", matrix.SchemaIdentifiers[0]);

                            CollectionAssert.AreEqual(new[] { 3001, 3002, 3003, 3004 }, matrix.Targets.ToArray());
                            CollectionAssert.AreEqual(new[] { 0, 2711, 2712, 2713, 2714 }, matrix.Sources.ToArray());
                            CollectionAssert.AreEqual(matrix.Targets.ToArray(), matrix.Connections.Keys.ToArray());

                            foreach (var connection in matrix.Connections)
                            {
                                Assert.AreEqual(0, connection.Value.Single());
                            }

                            var labels = consumer.Root.Sdn.Switching.Matrix0.Labels.Children.Single();
                            Assert.AreEqual("Primary", labels.Identifier);
                            Assert.AreEqual(0, labels.Targets.Children.Count);
                            Assert.AreEqual("Disconnected", labels.Sources.Children.Single().Value);

                            var parameters = consumer.Root.Sdn.Switching.Matrix0.Parameters;
                            Assert.AreEqual(matrix.Targets.Count, parameters.Targets.Children.Count);

                            CollectionAssert.AreEqual(
                                matrix.Targets.ToArray(), parameters.Targets.Children.Select(t => t.Number).ToArray());

                            foreach (var target in parameters.Targets.Children)
                            {
                                Assert.AreEqual(0, target.Children.Count);
                            }

                            CollectionAssert.AreEqual(
                                matrix.Sources.ToArray(), parameters.Sources.Children.Select(t => t.Number).ToArray());

                            foreach (var source in parameters.Sources.Children)
                            {
                                Assert.AreEqual(0, source.Children.Count);
                            }

                            CollectionAssert.AreEqual(
                                matrix.Connections.Keys.ToArray(),
                                parameters.Connections.Children.Select(t => t.Number).ToArray());

                            foreach (var target in matrix.Targets)
                            {
                                var targetParameters = parameters.Connections[target];

                                CollectionAssert.AreEqual(
                                    matrix.Sources.ToArray(),
                                    targetParameters.Children.Select(t => t.Number).ToArray());

                                foreach (var source in matrix.Sources)
                                {
                                    Assert.AreEqual(0, targetParameters[source].Children.Count);
                                }
                            }

                            var targets = matrix.Targets;
                            var sources = matrix.Sources;
                            var connections = matrix.Connections;
                            connections[targets[0]].Clear();
                            connections[targets[0]].Add(sources[1]);
                            await WaitAndAssertStableAsync(connections[targets[1]], new[] { sources[1] });
                            connections[targets[0]].Clear();
                            connections[targets[1]].Clear();
                            await WaitAndAssertStableAsync(connections[targets[0]], new[] { sources[2], sources[4] });
                            Assert.AreEqual(0, connections[targets[1]].Count);
                            connections[targets[1]].Add(sources[0]);
                            await WaitAndAssertStableAsync(
                                connections[targets[0]], new[] { sources[1], sources[2], sources[3], sources[4] });
                            connections[targets[0]].Remove(sources[1]);
                            await WaitAndAssertStableAsync(connections[targets[0]], new[] { sources[2], sources[4] });
                        },
                        true,
                        "MatrixMainLog.xml");
                });
        }

        /// <summary>Tests default values for Ember+ matrices.</summary>
        [TestMethod]
        public void MatrixMinimalTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    await TestWithRobot<MatrixRoot>(
                        consumer =>
                        {
                            var matrix = consumer.Root.Sdn.Switching.Matrix0.Matrix;
                            Assert.AreEqual("Matrix-0", matrix.Identifier);
                            Assert.AreEqual(4, matrix.MaximumTotalConnects);
                            Assert.AreEqual(1, matrix.MaximumConnectsPerTarget);
                            Assert.AreEqual(null, matrix.ParametersLocation);
                            Assert.AreEqual(null, matrix.GainParameterNumber);
                            Assert.AreEqual(null, matrix.Labels);
                            Assert.AreEqual(null, matrix.SchemaIdentifiers);

                            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, matrix.Targets.ToArray());
                            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, matrix.Sources.ToArray());
                            CollectionAssert.AreEqual(matrix.Targets.ToArray(), matrix.Connections.Keys.ToArray());

                            foreach (var connection in matrix.Connections)
                            {
                                Assert.AreEqual(0, connection.Value.Count);
                            }

                            return Task.FromResult(false);
                        },
                        true,
                        "MatrixMinimalLog.xml");
                });
        }

        /// <summary>Tests the behavior when <see cref="Element.IsOnline"/> changes.</summary>
        [TestMethod]
        public void IsOnlineTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    await TestWithRobot<ZoneNodeRoot>(
                        async consumer =>
                        {
                            var state = consumer.Root.ZoneNode.Ravenna.MediaSessions.Children[0].State;
                            Assert.IsNull(consumer.Root.ZoneNode.Ravenna.MediaSessionReceivers);
                            state.Value = 1;
                            await WaitForChangeAsync(
                                consumer.Root.ZoneNode.Ravenna.GetProperty(r => r.MediaSessionReceivers));
                            var receivers = consumer.Root.ZoneNode.Ravenna.MediaSessionReceivers;
                            Assert.IsNotNull(receivers);
                            Assert.AreEqual(1, receivers.Children.Count);
                            Assert.AreEqual("text42", receivers.Children[0].Uri.Value);
                            Assert.AreEqual("text43", receivers.Children[0].Sdp.Value);
                            Assert.AreEqual(0, receivers.Children[0].State.Value);
                            state.Value = 2;
                            await WaitForChangeAsync(receivers.Children.GetProperty(c => c.Count), 0);
                            await WaitForChangeAsync(((INode)receivers).Children.GetProperty(c => c.Count), 0);
                            state.Value = 3;
                            await WaitForChangeAsync(receivers.Children.GetProperty(c => c.Count), 1);
                            await WaitForChangeAsync(((INode)receivers).Children.GetProperty(c => c.Count), 1);
                            state.Value = 4;
                            await WaitForChangeAsync(receivers.Children[0].GetProperty(r => r.Sdp), null);
                            state.Value = 5;
                            await WaitForChangeAsync(receivers.Children[0].GetProperty(r => r.Sdp));
                        },
                        true,
                        "IsOnlineLog.xml");
                });
        }

        /// <summary>Tests various streaming scenarios.</summary>
        [TestMethod]
        public void StreamTest()
        {
            var enumValues = (Enumeration[])Enum.GetValues(typeof(Enumeration));
            var enumValue = (int)enumValues[this.Random.Next(enumValues.Length)];
            var realValue = this.Random.NextDouble();

            AsyncPump.Run(
                async () =>
                {
                    await AssertThrowAsync<ModelException>(
                        () => this.StreamTestCore((byte)this.Random.Next(byte.MinValue, byte.MaxValue + 1), (byte)enumValue, (float)realValue, Invalid, Genuine, false),
                        "The field format has an unexpected value for the element with the path /IntegerParameter.");
                    await AssertThrowAsync<ModelException>(
                        () => this.StreamTestCore((byte)this.Random.Next(byte.MinValue, byte.MaxValue + 1), (byte)enumValue, (float)realValue, Genuine, OneByteMissing, false),
                        "A stream entry for the parameter with the path /EnumerationParameter is incompatible, see inner exception for more information.");
                    await AssertThrowAsync<ModelException>(
                        () => this.StreamTestCore((long)this.Random.Next(int.MinValue, int.MaxValue), (long)enumValue, realValue, Genuine, OneByteMissing, false),
                        "A stream entry for the parameter with the path /EnumerationParameter is incompatible, see inner exception for more information.");
                    await AssertThrowAsync<ModelException>(
                        () => this.StreamTestCore(BitConverter.DoubleToInt64Bits(3.1415925359), (long)enumValue, realValue, Mismatch, Genuine, false),
                        "Read unexpected stream value 3.1415925359 for the parameter with the path /IntegerParameter.");
                    await AssertThrowAsync<ModelException>(
                        () => this.StreamTestCore((byte)this.Random.Next(byte.MinValue, byte.MaxValue + 1), (byte)enumValue, 3.1415925359, Genuine, Genuine, true),
                        "Read stream value 3.1415925359 while expecting to read an octetstring for the parameter with the path /RealParameter.");
                    await this.StreamTestCore(
                        (byte)this.Random.Next(byte.MinValue, byte.MaxValue + 1), (byte)enumValue, (float)realValue);
                    await this.StreamTestCore(
                        (ushort)this.Random.Next(ushort.MinValue, ushort.MaxValue + 1), (ushort)enumValue, realValue);
                    await this.StreamTestCore(
                        (uint)this.Random.Next((int)uint.MinValue, int.MaxValue), (uint)enumValue, (float)realValue);
                    await this.StreamTestCore(
                        (ulong)this.Random.Next((int)ulong.MinValue, int.MaxValue), (ulong)enumValue, realValue);
                    await this.StreamTestCore(
                        (sbyte)this.Random.Next(sbyte.MinValue, sbyte.MaxValue + 1), (sbyte)enumValue, (float)realValue);
                    await this.StreamTestCore(
                        (short)this.Random.Next(short.MinValue, short.MaxValue + 1), (short)enumValue, realValue);
                    await this.StreamTestCore(
                        this.Random.Next(int.MinValue, int.MaxValue), enumValue, (float)realValue);
                    await this.StreamTestCore(
                        (long)this.Random.Next(int.MinValue, int.MaxValue), (long)enumValue, realValue);
                });
        }

        /// <summary>Tests <see cref="CollectionNode{TMostDerived, TElement}"/>.</summary>
        [TestMethod]
        public void CollectionNodeTest()
        {
            AsyncPump.Run(() => TestWithRobot<CollectionNodeRoot>(
                consumer =>
                {
                    var node = consumer.Root.Node;
                    Assert.AreEqual(1, node.Children.Count);
                    Assert.AreEqual(42, node.Children[0].Value);
                    Assert.AreEqual(43, node.Parameter2.Value);
                    Assert.AreEqual(44, node.Parameter3.Value);
                    Assert.AreEqual(3, ((INode)node).Children.Count);
                    return Task.FromResult(false);
                },
                true,
                "CollectionNodeLog.xml"));
        }

        /// <summary>Tests various exceptional conditions.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    await AssertThrowAsync<ArgumentNullException>(() => Consumer<SingleNodeRoot>.CreateAsync(null));

                    using (var dummy = new S101Client(Stream.Null, Stream.Null.ReadAsync, Stream.Null.WriteAsync))
                    {
                        await AssertThrowAsync<ArgumentOutOfRangeException>(
                            () => Consumer<SingleNodeRoot>.CreateAsync(dummy, -2),
                            () => Consumer<SingleNodeRoot>.CreateAsync(dummy, 10000, ChildrenRetrievalPolicy.None - 1),
                            () => Consumer<SingleNodeRoot>.CreateAsync(dummy, 10000, ChildrenRetrievalPolicy.All + 1));
                    }

                    TestStandardExceptionConstructors<ModelException>();
                    await AssertThrowInCreateAsync<TimeoutException, SingleNodeRoot>(
                        "IncompleteLog1.xml",
                        "The provider failed to send the children for the element with the path /FieldNode.");
                    await AssertThrowInCreateAsync<TimeoutException, SingleNodeRoot>(
                        "IncompleteLog2.xml",
                        "The provider failed to send the children for the element with the path /FieldNode.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "InvalidEmberResponse.xml",
                        "Encountered invalid EmBER data while expecting outer identifier A-0.",
                        false);
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "UnexpectedOuterIdResponse.xml",
                        "Found actual outer identifier A-5000 while expecting A-0.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "EndOfStreamResponse.xml",
                        "Encountered end of stream while expecting outer identifier A-0.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "NumberOutOfRangeResponse.xml",
                        "Found actual integer 2147483648 while expecting to read a 32-bit integer.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "UnexpectedInnerNumberResponse1.xml",
                        "Found actual inner number 16 while expecting 17 on a container with the outer identifier C-1.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "UnexpectedInnerNumberResponse2.xml",
                        "Found actual inner number 2 while expecting to read a System.Boolean data value with outer identifier C-3.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "EndOfContainerResponse.xml",
                        "Found end of container while expecting outer identifier C-0.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "UnexpectedNonContextSpecificResponse.xml",
                        "Found actual outer identifier A-100 while expecting a context-specific one.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "NoDataValueAvailableReponse1.xml",
                        "No data value available for the required property Lawo.EmberPlusSharp.Model.Test.SingleNodeRoot.FieldNode in the node with the path /.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "NoDataValueAvailableReponse2.xml",
                        "No data value available for the required property Lawo.EmberPlusSharp.Model.Test.RecursiveFieldNode.SomeParameter in the node with the path /FieldNode/FieldNode.");
                    await AssertThrowAfterFirstRequest<ModelException, DuplicateElementRoot>(
                        "DuplicateElementResponse1.xml",
                        "Duplicate identifier found in Lawo.EmberPlusSharp.Model.Test.DuplicateElementRoot.");
                    await AssertThrowAsync<ModelException>(() => TestNoExceptionsAsync(
                        (c, p) => Consumer<NoDefaultConstructorRoot>.CreateAsync(c),
                        () => ConnectAsync(-1, null),
                        () => WaitForConnectionAsync(new S101Logger(GlowTypes.Instance, Console.Out))));
                    await AssertThrowAfterFirstRequest<ModelException, UnsupportedPropertyTypeRoot>(
                        "UnsupportedPropertyTypeResponse.xml",
                        "The property Whatever in the type Lawo.EmberPlusSharp.Model.Test.UnsupportedPropertyTypeRoot has an unsupported type.");
                    await AssertThrowAfterFirstRequest<ModelException, PropertiesRoot>(
                        "InvalidPathResponse.xml",
                        "Invalid path for a qualified element.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "NoValueResponse.xml",
                        "No value field is available for the non-nullable parameter with the path /FieldNode/SomeParameter.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "ValueOutOfRangeResponse.xml",
                        "The value of the field factor is out of the allowed range for the element with the path /FieldNode/SomeParameter.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "UnexpectedValueResponse.xml",
                        "The field access has an unexpected value for the element with the path /FieldNode/SomeParameter.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "MissingTypeResponse.xml",
                        "No enumeration, enumMap, value or type field is available for the parameter with the path /FieldNode/SomeParameter.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "TypeMismatchResponse1.xml",
                        "Found a Node data value while expecting a Parameter for the element with the path /FieldNode/SomeParameter.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "TypeMismatchResponse2.xml",
                        "Found a Parameter data value while expecting a Node for the element with the path /FieldNode.");
                    await AssertThrowAfterFirstRequest<ModelException, SingleNodeRoot>(
                        "QualifiedParameterChildResponse.xml",
                        "The path of a qualified element attempts to address a direct or indirect child of the element with the path /FieldNode/SomeParameter.");
                    await AssertThrowAfterFirstRequest<ModelException, EnumParameterRoot>(
                        "MissingEnumerationResponse.xml",
                        "No enumeration or enumMap field is available for the enum parameter with the path /Enum.");
                    await AssertThrowAfterFirstRequest<ModelException, EnumParameterRoot>(
                        "EntryCountMismatchResponse.xml",
                        "The number of named constants of the enum specified for the parameter with the path /Enum does not match the number of entries sent by the provider.");
                    await AssertThrowAfterFirstRequest<ModelException, EnumParameterRoot>(
                        "MissingNamedConstantResponse.xml",
                        "The enum specified for the parameter with the path /Enum does not have a named constant for the value 4.");
                    await AssertThrowAfterFirstRequest<ModelException, NoEnumParameterRoot>(
                        "EnumParameterResponse.xml",
                        "The type argument passed to the enum parameter with the path /Enum is not an enum.");
                    await AssertThrowInCreateAsync<ModelException, EmptyDynamicRoot>(
                        "UnexpectedDtdLog.xml",
                        "Unexpected DTD: 02.");
                    await AssertThrowInCreateAsync<ModelException, EmptyDynamicRoot>(
                        "UnsupportedGlowVersionLog1.xml",
                        "Encountered actual Glow DTD Version 1.10 while expecting >= 2.10.");
                    await AssertThrowInCreateAsync<ModelException, EmptyDynamicRoot>(
                        "UnsupportedGlowVersionLog2.xml",
                        "Encountered actual Glow DTD Version 2.9 while expecting >= 2.10.");
                    await AssertThrowInCreateAsync<ModelException, EmptyDynamicRoot>(
                        "UnsupportedGlowVersionLog3.xml",
                        "Encountered actual Glow DTD Version 2 while expecting >= 2.10.");
                    await AssertThrowInCreateAsync<ModelException, EmptyDynamicRoot>(
                        "UnsupportedGlowVersionLog4.xml",
                        "Encountered actual Glow DTD Version 2.10.1 while expecting >= 2.10.");
                    await AssertThrowInCreateAsync<ModelException, SingleNodeRoot>(
                        "InvalidParameterType.xml",
                        "The field type has an unexpected value for the element with the path /FieldNode/SomeParameter.");
                    await AssertThrowInCreateAsync<ModelException, UnsupportedTypeInFunctionRoot1>(
                        "UnaryFunctionLog.xml",
                        "Unsupported type in function signature Lawo.EmberPlusSharp.Model.Function`2[System.Char,Lawo.EmberPlusSharp.Model.Result`1[System.Int64]].");
                    await AssertThrowInCreateAsync<ModelException, UnsupportedTypeInFunctionRoot2>(
                        "UnaryFunctionLog.xml",
                        "Unsupported type in function signature Lawo.EmberPlusSharp.Model.Function`2[System.Int32,Lawo.EmberPlusSharp.Model.Result`1[System.Char]].",
                        false);
                    await AssertThrowInCreateAsync<ModelException, FunctionSignatureMismatchRoot1>(
                        "UnaryFunctionLog.xml",
                        "The actual signature for the function with the path /Function does not match the expected signature.",
                        false);
                    await AssertThrowInCreateAsync<ModelException, FunctionSignatureMismatchRoot2>(
                        "UnaryFunctionLog.xml",
                        "The actual signature for the function with the path /Function does not match the expected signature.",
                        false);
                    await AssertThrowInCreateAsync<ModelException, FunctionSignatureMismatchRoot3>(
                        "UnaryFunctionLog.xml",
                        "The actual signature for the function with the path /Function does not match the expected signature.",
                        false);
                    await AssertThrowInCreateAsync<ModelException, FunctionSignatureMismatchRoot4>(
                        "UnaryFunctionLog.xml",
                        "The actual signature for the function with the path /Function does not match the expected signature.",
                        false);
                    await AssertThrowInCreateAsync<ModelException, FunctionSignatureMismatchRoot5>(
                        "UnaryFunctionLog.xml",
                        "The actual signature for the function with the path /Function does not match the expected signature.",
                        false);
                    await AssertThrowInCreateAsync<ModelException, FunctionSignatureMismatchRoot6>(
                        "UnaryFunctionLog.xml",
                        "The actual signature for the function with the path /Function does not match the expected signature.",
                        false);
                    await AssertThrowInCreateAsync<ModelException, FunctionSignatureMismatchRoot1>(
                        "NullaryFunctionLog.xml",
                        "The actual signature for the function with the path /Function does not match the expected signature.",
                        false);
                    await AssertThrowInCreateAsync<ModelException, FunctionSignatureMismatchRoot4>(
                        "NullaryFunctionLog.xml",
                        "The actual signature for the function with the path /Function does not match the expected signature.",
                        false);
                    await AssertThrowAsync<ModelException>(
                        () => TestWithRobot<FunctionResultMismatchRoot>(
                            c =>
                            {
                                c.AutoSendInterval = 10;
                                return c.Root.Function.InvokeAsync(42);
                            },
                            false,
                            "FunctionResultValueMismatchLog1.xml"),
                        "The received tuple length does not match the tuple description length of 1.");
                    await AssertThrowAsync<ModelException>(
                        () => TestWithRobot<FunctionResultMismatchRoot>(
                            c =>
                            {
                                c.AutoSendInterval = 10;
                                return c.Root.Function.InvokeAsync(42);
                            },
                            false,
                            "FunctionResultValueMismatchLog2.xml"),
                        "The received tuple length does not match the tuple description length of 1.");
                    await AssertThrowInCreateAsync<ModelException, ZoneNodeRoot>(
                        "OfflineRequiredElementLog.xml",
                        "The required property Lawo.EmberPlusSharp.Model.Test.ZoneNode.Ravenna in the node with the path /ZoneNode has been set offline by the provider.");
                    await AssertThrowInCreateAsync<ModelException, InterfaceElementRoot<CollectionNode<IElement>>>(
                        "MainLog.xml",
                        "The type argument passed to CollectionNode<TElement> with the path /CollectionNode is neither an Element<TMostDerived> subclass nor IParameter nor INode nor IFunction.",
                        false);
                    await AssertThrowInCreateAsync<ModelException, InterfaceElementRoot<CollectionNode<INode>>>(
                        "MainLog.xml",
                        "Found a Parameter data value while expecting a Node for the element with the path /CollectionNode/_1.",
                        false);
                    await AssertThrowInCreateAsync<ModelException, InterfaceElementRoot<CollectionNode<IFunction>>>(
                        "MainLog.xml",
                        "Found a Parameter data value while expecting a Function for the element with the path /CollectionNode/_1.",
                        false);
                });
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/1620">Bug 1620</see>.</summary>
        [TestMethod]
        public void Bug1620Test()
        {
            AsyncPump.Run(() => TestWithRobot<SingleNodeRoot>(
                consumer =>
                {
                    Assert.AreEqual(1, consumer.Root.FieldNode.SomeParameter.Value);
                    return Task.FromResult(false);
                },
                false,
                "Bug1620Log.xml"));
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/1766">Bug 1766</see>.</summary>
        [TestMethod]
        public void Bug1766Test()
        {
            AsyncPump.Run(
                () => TestWithRobot<EmptyNodeRoot>(consumer => Task.FromResult(false), false, "Bug1766Log.xml"));
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/1834">Bug 1834</see>.</summary>
        [TestMethod]
        public void Bug1834Test()
        {
            AsyncPump.Run(() => TestWithRobot<EmptyRoot>(consumer => Task.FromResult(false), false, "Bug1834Log.xml"));
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/1836">Bug 1836</see>.</summary>
        [TestMethod]
        public void Bug1836Test()
        {
            AsyncPump.Run(
                async () =>
                {
                    var providerResponse = await ConcatMessagesAsync(
                        await GetS101MessageStreamAsync(GetPayload("UninterestingParameterResponse.xml")),
                        await GetS101MessageStreamAsync(GetPayload("UninterestingParameterResponse2.xml")),
                        await GetS101MessageStreamAsync(GetPayload("InterestingParameterResponse.xml")));

                    var providerClientTask = WaitForConnectionAsync();

                    using (var consumerClient = await ConnectAsync(-1, null))
                    using (var providerClient = await providerClientTask)
                    using (var providerStream = providerClient.GetStream())
                    {
                        var providerReader = new S101Reader(providerStream.ReadAsync);
                        var consumerTask = Consumer<SingleParameterRoot>.CreateAsync(consumerClient);

                        if (await providerReader.ReadAsync(CancellationToken.None))
                        {
                            await providerStream.WriteAsync(providerResponse, 0, providerResponse.Length);
                            await MonitorConnection(consumerTask, c => Task.FromResult(false));
                        }

                        await providerReader.DisposeAsync(CancellationToken.None);
                    }
                });
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/1866">Bug 1866</see>.</summary>
        [TestMethod]
        public void Bug1866Test()
        {
            AsyncPump.Run(() => TestWithRobot<EnumParameterRoot>(
                consumer =>
                {
                    Assert.AreEqual(Enumeration.Two, consumer.Root.Enum.Value);
                    return Task.FromResult(false);
                },
                false,
                "Bug1866Log.xml"));
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/2755">Bug 2755</see>.</summary>
        [TestMethod]
        public void Bug2755Test()
        {
            AsyncPump.Run(() => TestConsumerAfterFirstRequest<SingleNodeRoot>(
                (consumerTask, providerClient) =>
                {
                    providerClient.Dispose();
                    return AssertThrowAsync<OperationCanceledException>(
                        () => MonitorConnection(consumerTask, c => Task.FromResult(false)));
                },
                null,
                new S101Logger(GlowTypes.Instance, Console.Out)));
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/3345">Bug 3345</see>.</summary>
        [TestMethod]
        public void Bug3345Test()
        {
            AsyncPump.Run(() => TestWithRobot<TwoParameterRoot>(
                async consumer =>
                {
                    await Task.Delay(1000);
                    consumer.Root.Parameter1.Value = true;
                    consumer.Root.Parameter2.Value = true;
                    await Task.Delay(1000);
                },
                false,
                "Bug3345Log.xml"));
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/4424">Bug 4424</see>.</summary>
        [TestMethod]
        public void Bug4424Test()
        {
            AsyncPump.Run(() => AssertThrowInCreateAsync<ModelException, TwoParameterRoot>(
                "Bug4424Log.xml",
                "No data value available for the required property Lawo.EmberPlusSharp.Model.Test.TwoParameterRoot.Parameter2 in the node with the path /."));
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/4463">Bug 4463</see>.</summary>
        [TestMethod]
        public void Bug4463Test()
        {
            AsyncPump.Run(
                () => TestWithRobot<EmptyZoneNodeRoot>(c => Task.FromResult(false), true, "Bug4463Log.xml"));
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/4815">Bug 4815</see>.</summary>
        [TestMethod]
        public void Bug4815Test()
        {
            AsyncPump.Run(
                async () =>
                {
                    var providerClientTask = WaitForConnectionAsync();

                    using (var logger = new S101Logger(GlowTypes.Instance, Console.Out))
                    using (var consumerClient = await ConnectAsync(-1, logger))
                    using (var providerClient = await providerClientTask)
                    using (var providerStream = providerClient.GetStream())
                    {
                        var providerReader = new S101Reader(providerStream.ReadAsync);
                        var consumerTask = Consumer<EmptyZoneNodeRoot>.CreateAsync(consumerClient);
                        await ReadAndAssertEqualAsync(providerReader, "Request1.xml");

                        var providerResponse1 = await ConcatMessagesAsync(
                            await GetS101MessageStreamAsync(GetPayload("Bug4815Response1.xml")));
                        await providerStream.WriteAsync(providerResponse1, 0, providerResponse1.Length);

                        await MonitorConnection(
                            consumerTask,
                            async consumer =>
                            {
                                Assert.AreEqual(consumer.Root.ZoneNode, null);
                                var providerResponse2 = await ConcatMessagesAsync(
                                    await GetS101MessageStreamAsync(GetPayload("Bug4815Response2.xml")));

                                await providerStream.WriteAsync(providerResponse2, 0, providerResponse2.Length);
                                await ReadAndAssertEqualAsync(providerReader, "Bug4815Request2.xml");

                                var providerResponse3 = await ConcatMessagesAsync(
                                    await GetS101MessageStreamAsync(GetPayload("Bug4815Response3.xml")));
                                await providerStream.WriteAsync(providerResponse3, 0, providerResponse3.Length);
                                var readTask = providerReader.ReadAsync(CancellationToken.None);

                                Func<Task> delayedAbort =
                                    async () =>
                                    {
                                        await Task.Delay(2000);
                                        providerClient.Close();
                                    };

                                var delayedAbortTask = delayedAbort();
                                Assert.AreNotEqual(await Task.WhenAny(readTask, delayedAbortTask), readTask);

                                try
                                {
                                    await Task.WhenAll(readTask, delayedAbortTask);
                                }
                                catch (ObjectDisposedException)
                                {
                                    // Intentionally swallowed
                                }
                            });

                        await providerReader.DisposeAsync(CancellationToken.None);
                    }
                });
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/5201">Bug 5201</see>.</summary>
        [TestMethod]
        public void Bug5201Test()
        {
            AsyncPump.Run(
                async () =>
                {
                    var providerClientTask = WaitForConnectionAsync();

                    using (var logger = new S101Logger(GlowTypes.Instance, Console.Out))
                    using (var consumerClient = await ConnectAsync(-1, logger))
                    using (var providerClient = await providerClientTask)
                    using (var providerStream = providerClient.GetStream())
                    {
                        var providerReader = new S101Reader(providerStream.ReadAsync);
                        var consumerTask = Consumer<EmptyZoneNodeRoot>.CreateAsync(consumerClient);
                        await ReadAndAssertEqualAsync(providerReader, "Request1.xml");

                        var providerResponse1 = await ConcatMessagesAsync(
                            await GetS101MessageStreamAsync(GetPayload("Bug5201Response1.xml")),
                            await GetS101MessageStreamAsync(GetPayload("Bug5201Response2.xml")));
                        await providerStream.WriteAsync(providerResponse1, 0, providerResponse1.Length);
                        await ReadAndAssertEqualAsync(providerReader, "Bug5201Request2.xml");

                        var providerResponse3 = await ConcatMessagesAsync(
                            await GetS101MessageStreamAsync(GetPayload("Bug5201Response3.xml")));
                        await providerStream.WriteAsync(providerResponse3, 0, providerResponse3.Length);

                        await MonitorConnection(
                            consumerTask,
                            consumer =>
                            {
                                Assert.AreNotEqual(consumer.Root.ZoneNode, null);
                                return Task.FromResult(false);
                            });

                        await providerReader.DisposeAsync(CancellationToken.None);
                    }
                });
        }

        /// <summary>Exposes <see href="https://redmine.lawo.de/redmine/issues/5639">Bug 5639</see>.</summary>
        [TestMethod]
        public void Bug5639Test()
        {
            AsyncPump.Run(() => TestWithRobot<BooleanRoot>(c => Task.FromResult(false), true, "Bug5639Log.xml"));
        }

        /// <summary>Exposes <see href="https://github.com/Lawo/ember-plus-sharp/issues/27">Bug 27</see>.</summary>
        [TestMethod]
        public void Bug27Test()
        {
            AsyncPump.Run(() => TestWithRobot<GrassRoot>(
                async c =>
                {
                    var result = await c.Root.Production.LoadSnapshot.InvokeAsync("snapshot0004");
                    Assert.AreEqual(0, result.Items.Count());
                },
                true,
                "Bug27Log.xml",
                "true"));

            AsyncPump.Run(() => TestWithRobot<GrassRoot>(
                async c =>
                {
                    try
                    {
                        var result = await c.Root.Production.LoadSnapshot.InvokeAsync("snapshot0004");
                        Assert.Fail("Unexpected success.");
                    }
                    catch (InvocationFailedException ex)
                    {
                        Assert.AreEqual(0, ex.Result.Items.Count());
                    }
                },
                true,
                "Bug27Log.xml",
                "false"));
        }

        /// <summary>Tests that Ember+ trees received with different <see cref="ChildrenRetrievalPolicy"/> have an equal
        /// structure.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void Bug40Test()
        {
            AsyncPump.Run(
                async () =>
                {
                    var automaticRoot = await GetTreeAsync(ChildrenRetrievalPolicy.All);
                    var manualRoot = await GetTreeAsync(ChildrenRetrievalPolicy.DirectOnly);
                    Compare(automaticRoot, manualRoot);
                });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Task TestConsumerAfterFirstRequest<TRoot>(
            Func<Task<Consumer<TRoot>>, S101Client, Task> testCallback,
            IS101Logger consumerLogger,
            IS101Logger providerLogger)
            where TRoot : Root<TRoot>
        {
            return TestNoExceptionsAsync(
                async (consumerClient, providerClient) =>
                {
                    var request1Task = WaitForRequest(providerClient, "Request1.xml");
                    var consumerTask = Consumer<TRoot>.CreateAsync(consumerClient);
                    await request1Task;
                    await testCallback(consumerTask, providerClient);
                },
                () => ConnectAsync(-1, consumerLogger),
                () => WaitForConnectionAsync(providerLogger));
        }

        private static Task SendResponse(S101Client provider, string payloadXmlName, params object[] args)
        {
            if (payloadXmlName == null)
            {
                return Task.FromResult(false);
            }
            else
            {
                return provider.SendMessageAsync(EmberDataMessage, GetPayload(payloadXmlName, args));
            }
        }

        private static async Task WaitForRequest(
            S101Client provider, string expectedPayloadXmlName, params object[] args)
        {
            var payloadReceived = new TaskCompletionSource<byte[]>();
            EventHandler<MessageReceivedEventArgs> handler = (s, e) => payloadReceived.SetResult(e.GetPayload());
            provider.EmberDataReceived += handler;

            try
            {
                var actualPayloadBytes = await payloadReceived.Task;
                Assert.IsTrue(GetPayload(expectedPayloadXmlName, args).SequenceEqual(actualPayloadBytes));
            }
            finally
            {
                provider.EmberDataReceived -= handler;
            }
        }

        private static byte[] GetPayload(string payloadXmlName, params object[] args)
        {
            var xml = GetXml(payloadXmlName);
            return ToPayload(args.Length > 0 ? string.Format(CultureInfo.InvariantCulture, xml, args) : xml);
        }

        private static string GetXml(string payloadXmlName) => GetContent<ModelPayloads>(payloadXmlName);

        private static byte[] ToPayload(string xml)
        {
            using (var textReader = new StringReader(xml))
            using (var reader =
                XmlReader.Create(textReader, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
            {
                return new EmberConverter(GlowTypes.Instance).FromXml(reader);
            }
        }

        private static async Task ReadAndAssertEqualAsync(S101Reader providerReader, string requestXml)
        {
            Assert.IsTrue(await providerReader.ReadAsync(CancellationToken.None));
            Assert.IsInstanceOfType(providerReader.Message.Command, typeof(EmberData));
            Assert.IsTrue(GetPayload(requestXml).SequenceEqual(await ReadToEndAsync(providerReader.Payload)));
        }

        private static async Task<byte[]> ConcatMessagesAsync(params Stream[] messageStreams)
        {
            using (var result = new MemoryStream())
            {
                foreach (var messageStream in messageStreams)
                {
                    using (messageStream)
                    {
                        await messageStream.CopyToAsync(result);
                    }
                }

                return result.ToArray();
            }
        }

        private static async Task<byte[]> ReadToEndAsync(Stream stream)
        {
            using (var result = new MemoryStream())
            {
                await stream.CopyToAsync(result);
                return result.ToArray();
            }
        }

        private static void AssertAreEqual<TValue, TElement>(
            TValue expected, Func<TElement, TValue> getProperty, params TElement[] elements)
        {
            foreach (var element in elements)
            {
                Assert.AreEqual(expected, getProperty(element));
            }
        }

        private static void AssertNotified<TType, TProperty>(
            TType obj, Expression<Func<TType, TProperty>> getValue, TProperty value)
            where TType : INotifyPropertyChanged
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)getValue.Body).Member;
            Assert.AreNotEqual(value, propertyInfo.GetValue(obj));

            var expectedPropertyName = propertyInfo.Name;
            string actualPropertyName = null;
            PropertyChangedEventHandler handler = (s, e) => actualPropertyName = e.PropertyName;
            obj.PropertyChanged += handler;

            try
            {
                propertyInfo.SetValue(obj, value);
                Assert.AreEqual(expectedPropertyName, actualPropertyName);
            }
            finally
            {
                obj.PropertyChanged -= handler;
            }
        }

        private static void AssertEnumMapEquality(
            string[] entryStrings, int[] entryIntegers, IReadOnlyList<KeyValuePair<string, int>> enumMap)
        {
            Assert.AreEqual(entryStrings.Length, enumMap.Count);

            for (int index = 0; index < entryStrings.Length; ++index)
            {
                Assert.AreEqual(entryIntegers[index], enumMap.First(p => p.Key == entryStrings[index]).Value);
            }
        }

        private static void AssertAccess(RecursiveFieldNode node)
        {
            var parameter = node.SomeParameter;

            if ((parameter.Access & ParameterAccess.Read) == 0)
            {
                AssertThrow<InvalidOperationException>(() => parameter.Value.Ignore());
            }
            else
            {
                parameter.Value.Ignore();
            }

            if ((parameter.Access & ParameterAccess.Write) == 0)
            {
                AssertThrow<InvalidOperationException>(() => parameter.Value = 42);
            }
            else
            {
                parameter.Value = 42;
            }
        }

        private static Task StaticChildrenRetrievalPolicyTestAsync(ChildrenRetrievalPolicy policy, string logName) =>
            TestWithRobot<ModelPayloads>(
                async client =>
                {
                    using (var consumer = await Consumer<EmptyNodeRoot>.CreateAsync(client, 4000, policy))
                    {
                        var root = consumer.Root;
                        AssertThrow<ArgumentOutOfRangeException>(
                            () => root.ChildrenRetrievalPolicy = (ChildrenRetrievalPolicy)(-1),
                            () => root.ChildrenRetrievalPolicy = ChildrenRetrievalPolicy.All + 1);
                        Assert.AreEqual(policy, root.ChildrenRetrievalPolicy);

                        const string ExpectedMessage =
                            "A new value cannot be set if the current value is not equal to None.";

                        if (root.ChildrenRetrievalPolicy == ChildrenRetrievalPolicy.None)
                        {
                            root.ChildrenRetrievalPolicy = ChildrenRetrievalPolicy.DirectOnly;
                            AssertThrow<InvalidOperationException>(
                                () => root.ChildrenRetrievalPolicy = ChildrenRetrievalPolicy.All, ExpectedMessage);
                        }
                        else
                        {
                            AssertThrow<InvalidOperationException>(() => root.ChildrenRetrievalPolicy -= 1, ExpectedMessage);
                        }

                        if (policy == ChildrenRetrievalPolicy.None)
                        {
                            Assert.IsNull(root.Node);
                        }
                        else
                        {
                            var childPolicy = policy == ChildrenRetrievalPolicy.All ?
                                ChildrenRetrievalPolicy.All : ChildrenRetrievalPolicy.None;
                            Assert.AreEqual(childPolicy, root.Node.ChildrenRetrievalPolicy);
                        }
                    }
                },
                null,
                null,
                GlowTypes.Instance,
                false,
                logName);

        private static Task WaitForCompletion(Consumer<EmptyNodeRoot> consumer, bool delay) =>
            delay ? Task.Delay(consumer.AutoSendInterval + 500) : consumer.SendAsync();

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Objects are disposed within the called method.")]
        private static Task TestWithRobot<TRoot>(
            Func<Consumer<TRoot>, Task> testCallback, bool log, string logXmlName, params object[] args)
            where TRoot : Root<TRoot>
        {
            return TestWithRobot<ModelPayloads>(
                client => MonitorConnection(Consumer<TRoot>.CreateAsync(client, 4000), testCallback),
                CreateLogger(log, logXmlName, "consumer.xml"),
                CreateLogger(log, logXmlName, "provider.xml"),
                GlowTypes.Instance,
                false,
                logXmlName,
                args);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Objects are disposed within the called method.")]
        private static Task AssertThrowInCreateAsync<TException, TRoot>(string logXmlName, string expectedMessage, bool log = true)
            where TException : Exception
            where TRoot : Root<TRoot>
        {
            return AssertThrowAsync<TException>(
                () => TestWithRobot<TRoot>(c => Task.FromResult(false), log, logXmlName), expectedMessage);
        }

        private static async Task AssertThrowAfterFirstRequest<TException, TRoot>(
            string payloadXmlName, string expectedMessage, bool log = true)
            where TException : Exception
            where TRoot : Root<TRoot>
        {
            await TestConsumerAfterFirstRequest<TRoot>(
                async (consumerTask, providerClient) =>
                {
                    await SendResponse(providerClient, payloadXmlName);
                    await AssertThrowAsync<TException>(
                        () => MonitorConnection(consumerTask, c => Task.FromResult(false)), expectedMessage);
                },
                CreateLogger(log, payloadXmlName, "consumer.xml"),
                CreateLogger(log, payloadXmlName, "provider.xml"));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Objects are disposed in called method.")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Test code.")]
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Test code.")]
        private static async Task BigTreeAssemblyTestCore<TRoot>(int magnitude, bool writeLog)
            where TRoot : Root<TRoot>
        {
            var encodedPayloadStream = await GetS101MessageStreamAsync(GetBigTreePayload(magnitude));
            var encodedPayload = encodedPayloadStream.ToArray();

            using (File.CreateText(".\\BigTreeOutput.txt"))
            {
                var stopwatch = new Stopwatch();

                AsyncPump.Run(
                    async () =>
                    {
                        using (var client = GetFakeClient(
                            encodedPayloadStream, writeLog ? new S101Logger(GlowTypes.Instance, Console.Out) : null))
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            stopwatch.Start();

                            await MonitorConnection(
                                Consumer<TRoot>.CreateAsync(client),
                                consumer =>
                                {
                                    GC.Collect();
                                    GC.WaitForPendingFinalizers();
                                    stopwatch.Stop();

                                    var message = "Assembled a tree with the root {0} containing {1} elements from a message " +
                                        "of {2} bytes in {3} milliseconds.";
                                    var elapsed = stopwatch.ElapsedMilliseconds;
                                    var elementCount = CountChildren(((INode)consumer.Root).Children);
                                    Console.WriteLine(
                                        message, typeof(TRoot).Name, elementCount, encodedPayload.Length, elapsed);
                                    return Task.FromResult(false);
                                });
                        }
                    });
            }

            var emberLibStopwatch = new Stopwatch();
            EventHandler<AsyncDomReader.RootReadyArgs> rootReady =
                (s, e) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    emberLibStopwatch.Stop();
                };

            EventHandler<FramingReader.KeepAliveRequestReceivedArgs> keepAliveRequestReceived = (s, e) => { };
            GC.Collect();
            GC.WaitForPendingFinalizers();
            emberLibStopwatch.Start();

            using (var glowReader = new GlowReader(rootReady, keepAliveRequestReceived))
            {
                try
                {
                    glowReader.ReadBytes(encodedPayload, 0, encodedPayload.Length);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // This is necessary due to a suspected bug in EmberLib.net
                }
            }

            Assert.IsFalse(emberLibStopwatch.IsRunning);
            Console.WriteLine("EmberLib.net: Processed in {0} milliseconds.", emberLibStopwatch.ElapsedMilliseconds);
        }

        private static byte[] GetBigTreePayload(int magnitude)
        {
            var payloadTemplateXml = GetXml("BigTreeRootTemplate.xml");
            var nodeChildrenTemplateXml = GetXml("BigTreeNodeChildrenTemplate.xml");

            for (int iteration = 0; iteration < magnitude; ++iteration)
            {
                payloadTemplateXml =
                    string.Format(CultureInfo.InvariantCulture, payloadTemplateXml, nodeChildrenTemplateXml);
            }

            var payload = ToPayload(string.Format(CultureInfo.InvariantCulture, payloadTemplateXml, string.Empty));
            return payload;
        }

        private static int CountChildren(IEnumerable<IElement> children)
        {
            var count = 0;

            foreach (var child in children)
            {
                ++count;
                var node = child as INode;

                if (node != null)
                {
                    count += CountChildren(node.Children);
                }
            }

            return count;
        }

        private static void CheckValues(MainRoot root)
        {
            Assert.IsNull(root.Parent);
            Assert.AreEqual(0, root.Number);
            Assert.AreEqual(string.Empty, root.Identifier);
            Assert.IsNull(root.Description);
            Assert.IsTrue(root.GetPath() == "/");
            Assert.AreEqual(true, root.IsRoot);
            Assert.AreEqual(true, root.IsOnline);

            Assert.AreEqual(null, ((IParameter)root.BooleanParameter).Minimum);
            Assert.AreEqual(null, ((IParameter)root.BooleanParameter).Maximum);
            Assert.AreEqual(null, ((IParameter)root.BooleanParameter).Factor);
            Assert.AreEqual(null, ((IParameter)root.BooleanParameter).Formula);
            Assert.AreEqual(null, ((IParameter)root.BooleanParameter).EnumMap);
            Assert.AreEqual(null, ((IParameter)root.OctetstringParameter).Minimum);
            Assert.AreEqual(null, ((IParameter)root.OctetstringParameter).Maximum);
            Assert.AreEqual(null, ((IParameter)root.OctetstringParameter).Factor);
            Assert.AreEqual(null, ((IParameter)root.OctetstringParameter).Formula);
            Assert.AreEqual(null, ((IParameter)root.OctetstringParameter).EnumMap);
            Assert.AreEqual(null, ((IParameter)root.StringParameter).Minimum);
            Assert.AreEqual(null, ((IParameter)root.StringParameter).Maximum);
            Assert.AreEqual(null, ((IParameter)root.StringParameter).Factor);
            Assert.AreEqual(null, ((IParameter)root.StringParameter).Formula);
            Assert.AreEqual(null, ((IParameter)root.StringParameter).EnumMap);
        }

        private static void CheckCollections(MainRoot root, bool[] children)
        {
            var original = ((Enumeration[])Enum.GetValues(typeof(Enumeration))).Select(
                v => new KeyValuePair<string, int>(v.ToString(), (int)v)).OrderBy(p => p.Value).ToArray();
            CollectionAssert.AreEqual(
                original, ((IParameter)root.EnumerationParameter).EnumMap.OrderBy(p => p.Value).ToArray());
            CollectionAssert.AreEqual(
                original, ((IParameter)root.EnumMapParameter).EnumMap.OrderBy(p => p.Value).ToArray());

            var collection = root.CollectionNode.Children;
            CollectionAssert.AreEqual(collection, ((INode)root.CollectionNode).Children);

            for (var index = 0; index < children.Length; ++index)
            {
                collection[index].Value = children[index];
            }
        }

        private static void CheckNumbers(MainRoot root)
        {
            var childrenMap = ((INode)root).Children.ToDictionary(c => c.Number);
            var children = childrenMap.Keys.Select(k => ((INode)root)[k]).ToArray();
            CollectionAssert.AreEqual(childrenMap.Values, children);
            Assert.AreEqual(
                root.CollectionNode[root.CollectionNode.Children[0].Number], root.CollectionNode.Children[0]);
            Assert.AreEqual(
                root.CollectionNode[root.CollectionNode.Children[1].Number], root.CollectionNode.Children[1]);
            Assert.AreEqual(
                root.CollectionNode[root.CollectionNode.Children[2].Number], root.CollectionNode.Children[2]);
        }

        private static TRoot GetRoot<TRoot>(string logXmlName)
            where TRoot : Root<TRoot>
        {
            TRoot root = null;
            AsyncPump.Run(
                () => TestWithRobot<TRoot>(consumer => Task.FromResult(root = consumer.Root), false, logXmlName));
            return root;
        }

        private static void AssertEqual<TCollectionNode>(
            MainRoot mainRoot, InterfaceElementRoot<TCollectionNode> interfaceRoot)
            where TCollectionNode : INode
        {
            Assert.AreEqual(mainRoot.BooleanParameter.Value, interfaceRoot.BooleanParameter.Value);
            Assert.AreEqual(mainRoot.IntegerParameter.Value, interfaceRoot.IntegerParameter.Value);
            Assert.AreEqual(mainRoot.FactorIntegerParameter.Value, interfaceRoot.FactorIntegerParameter.Value);
            Assert.AreEqual(mainRoot.FormulaIntegerParameter.Value, interfaceRoot.FormulaIntegerParameter.Value);
            Assert.AreEqual((long)mainRoot.EnumerationParameter.Value, interfaceRoot.EnumerationParameter.Value);
            Assert.AreEqual((long)mainRoot.EnumMapParameter.Value, interfaceRoot.EnumMapParameter.Value);
            CollectionAssert.AreEqual(mainRoot.OctetstringParameter.Value, (byte[])interfaceRoot.OctetstringParameter.Value);
            Assert.AreEqual(mainRoot.RealParameter.Value, interfaceRoot.RealParameter.Value);
            Assert.AreEqual(mainRoot.StringParameter.Value, interfaceRoot.StringParameter.Value);
            Assert.AreEqual(mainRoot.FieldNode.SomeParameter.Value, ((IParameter)interfaceRoot.FieldNode.Children[0]).Value);
            Assert.AreEqual(mainRoot.CollectionNode.Children.Count, interfaceRoot.CollectionNode.Children.Count);
        }

        private static void AssertSignature(
            IEnumerable<ParameterType> arguments, IEnumerable<ParameterType> result, IFunction function)
        {
            CollectionAssert.AreEqual(arguments.ToArray(), function.Arguments.Select(a => a.Value).ToArray());
            CollectionAssert.AreEqual(result.ToArray(), function.Result.Select(a => a.Value).ToArray());
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the XmlWriter it is passed to.")]
        private static S101Logger CreateLogger(bool log, string payloadXmlName, string extension)
        {
            if (!log)
            {
                return null;
            }

            return new S101Logger(
                GlowTypes.Instance,
                File.CreateText(Path.ChangeExtension(payloadXmlName, extension)),
                new XmlWriterSettings { Indent = true, CloseOutput = true });
        }

        private static byte[] Genuine(byte[] bytes) => bytes;

        private static byte[] OneByteMissing(byte[] bytes)
        {
            var result = new byte[bytes.Length > 0 ? bytes.Length - 1 : 0];
            Array.Copy(bytes, result, result.Length);
            return result;
        }

        private static int Genuine(int format) => format;

        private static int Invalid(int format) => 4242;

        private static int Mismatch(int format)
        {
            var endianBit = format & 1;

            switch ((StreamFormat)(format & ~1))
            {
                case StreamFormat.UInt32BigEndian:
                case StreamFormat.Int32BigEndian:
                    return (int)(StreamFormat.Float32BigEndian + endianBit);
                case StreamFormat.UInt64BigEndian:
                case StreamFormat.Int64BigEndian:
                    return (int)(StreamFormat.Float64BigEndian + endianBit);
                case StreamFormat.Float32BigEndian:
                    return (int)(StreamFormat.Int32BigEndian + endianBit);
                case StreamFormat.Float64BigEndian:
                    return (int)(StreamFormat.Int64BigEndian + endianBit);
                default:
                    throw new ArgumentException("No mismatch format available.");
            }
        }

        private static int GetFormat(object value, bool isLittleEndian)
        {
            var bigEndianFormat = GetFormat(value);

            switch (bigEndianFormat)
            {
                case StreamFormat.Byte:
                case StreamFormat.SByte:
                    return (int)bigEndianFormat;
                default:
                    return (int)bigEndianFormat + (isLittleEndian ? 1 : 0);
            }
        }

        private static StreamFormat GetFormat(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    return StreamFormat.Byte;
                case TypeCode.UInt16:
                    return StreamFormat.UInt16BigEndian;
                case TypeCode.UInt32:
                    return StreamFormat.UInt32BigEndian;
                case TypeCode.UInt64:
                    return StreamFormat.UInt64BigEndian;
                case TypeCode.SByte:
                    return StreamFormat.SByte;
                case TypeCode.Int16:
                    return StreamFormat.Int16BigEndian;
                case TypeCode.Int32:
                    return StreamFormat.Int32BigEndian;
                case TypeCode.Int64:
                    return StreamFormat.Int64BigEndian;
                case TypeCode.Single:
                    return StreamFormat.Float32BigEndian;
                case TypeCode.Double:
                    return StreamFormat.Float64BigEndian;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unexpected type: {0}", value));
            }
        }

        private static byte[] GetBytes(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    return new[] { (byte)value };
                case TypeCode.UInt16:
                    return BitConverter.GetBytes((ushort)value);
                case TypeCode.UInt32:
                    return BitConverter.GetBytes((uint)value);
                case TypeCode.UInt64:
                    return BitConverter.GetBytes((ulong)value);
                case TypeCode.SByte:
                    return new[] { unchecked((byte)(sbyte)value) };
                case TypeCode.Int16:
                    return BitConverter.GetBytes((short)value);
                case TypeCode.Int32:
                    return BitConverter.GetBytes((int)value);
                case TypeCode.Int64:
                    return BitConverter.GetBytes((long)value);
                case TypeCode.Single:
                    return BitConverter.GetBytes((float)value);
                case TypeCode.Double:
                    return BitConverter.GetBytes((double)value);
                default:
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture, "Unexpected type: {0}", value));
            }
        }

        private static async Task WaitAndAssertStableAsync(ObservableCollection<int> collection, int[] expected)
        {
            var source = new TaskCompletionSource<bool>();
            NotifyCollectionChangedEventHandler collectionChanged = null;

            collectionChanged =
                (s, e) =>
                {
                    if (collection.SequenceEqual(expected))
                    {
                        collection.CollectionChanged -= collectionChanged;
                        source.SetResult(false);
                    }
                };

            collection.CollectionChanged += collectionChanged;
            await source.Task;

            // The following two lines verify that the change we've detected above is not just an intermediate state.
            await Task.Delay(250);
            CollectionAssert.AreEqual(collection, expected);
        }

        private static async Task<EmptyDynamicRoot> GetTreeAsync(ChildrenRetrievalPolicy policy)
        {
            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync("localhost", 8999);

                using (var stream = tcpClient.GetStream())
                using (var client = new S101Client(tcpClient, stream.ReadAsync, stream.WriteAsync))
                using (var consumer = await Consumer<EmptyDynamicRoot>.CreateAsync(client, 10000, policy))
                {
                    if (policy != ChildrenRetrievalPolicy.All)
                    {
                        await RetrieveChildrenAsync(consumer, consumer.Root);
                    }

                    return consumer.Root;
                }
            }
        }

        private static async Task RetrieveChildrenAsync(Consumer<EmptyDynamicRoot> consumer, INode node)
        {
            foreach (IElement child in node.Children)
            {
                var childNode = child as INode;

                if (childNode != null)
                {
                    childNode.ChildrenRetrievalPolicy = ChildrenRetrievalPolicy.DirectOnly;
                    await consumer.SendAsync();
                    await RetrieveChildrenAsync(consumer, childNode);
                }
            }
        }

        private static void Compare(INode expected, INode actual)
        {
            foreach (IElement expectedChild in expected.Children)
            {
                var actualChild = actual[expectedChild.Number];
                Assert.IsNotNull(actualChild);
                Assert.AreEqual(expectedChild.GetType(), actualChild.GetType());

                var expectedChildNode = expectedChild as INode;

                if (expectedChildNode != null)
                {
                    var actualChildNode = actualChild as INode;
                    Assert.IsNotNull(actualChildNode);
                    Compare(expectedChildNode, actualChildNode);
                }
            }
        }

        private Task DynamicChildrenRetrievalPolicyTestAsync(bool delay)
        {
            return TestWithRobot<ModelPayloads>(
                async client =>
                {
                    using (var consumer = await Consumer<EmptyNodeRoot>.CreateAsync(
                        client, Timeout.Infinite, ChildrenRetrievalPolicy.None))
                    {
                        consumer.AutoSendInterval = this.Random.Next(100, 5000);
                        var root = consumer.Root;
                        Assert.IsNull(root.Node);
                        Assert.AreEqual(0, ((INode)root).Children.Count);
                        Assert.AreEqual(ChildrenRetrievalPolicy.None, root.ChildrenRetrievalPolicy);
                        root.ChildrenRetrievalPolicy = ChildrenRetrievalPolicy.DirectOnly;
                        await WaitForCompletion(consumer, delay);
                        Assert.IsNotNull(root.Node);
                        Assert.AreEqual(1, ((INode)root).Children.Count);
                        Assert.AreEqual(ChildrenRetrievalPolicy.None, root.Node.ChildrenRetrievalPolicy);
                        root.Node.ChildrenRetrievalPolicy = ChildrenRetrievalPolicy.All;
                        await WaitForCompletion(consumer, delay);
                        Assert.AreEqual(1, ((INode)root).Children.Count);
                    }
                },
                null,
                null,
                GlowTypes.Instance,
                false,
                "ChildrenRetrievalPolicyLog3.xml");
        }

        private Task StreamTestCore(object intValue, object enumValue, object realValue) =>
            this.StreamTestCore(intValue, enumValue, realValue, Genuine, Genuine, false);

        private async Task StreamTestCore(
            object intValue,
            object enumValue,
            object realValue,
            Func<int, int> failFormat,
            Func<byte[], byte[]> failEncoding,
            bool failType)
        {
            await this.StreamTestCore(intValue, enumValue, realValue, false, failFormat, failEncoding, failType);
            await this.StreamTestCore(intValue, enumValue, realValue, true, failFormat, failEncoding, failType);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "We need lowercase.")]
        private Task StreamTestCore(
            object intValue,
            object enumValue,
            object realValue,
            bool isLittleEndian,
            Func<int, int> failFormat,
            Func<byte[], byte[]> failEncoding,
            bool failType)
        {
            var boolValue = this.GetRandomBoolean();
            GetFormat(intValue).Ignore();
            GetFormat(enumValue).Ignore();
            var octetStringValue = new byte[this.Random.Next(0, 5)];
            this.Random.NextBytes(octetStringValue);
            var stringValue = GetRandomString();

            var intBytes = GetBytes(intValue);
            var enumBytes = GetBytes(enumValue);
            var realBytes = GetBytes(realValue);

            if (isLittleEndian != BitConverter.IsLittleEndian)
            {
                intBytes = intBytes.Reverse().ToArray();
                enumBytes = enumBytes.Reverse().ToArray();
                realBytes = realBytes.Reverse().ToArray();
            }

            var args =
                new[]
                {
                    failFormat(GetFormat(intValue, isLittleEndian)),
                    0,
                    failFormat(GetFormat(enumValue, isLittleEndian)),
                    intBytes.Length,
                    failFormat(GetFormat(realValue, isLittleEndian)),
                    0,
                    boolValue.ToString().ToLowerInvariant(),
                    new SoapHexBinary(failEncoding(intBytes.Concat(enumBytes).ToArray())),
                    new SoapHexBinary(failEncoding(octetStringValue)),
                    failType ? "Real" : "Octetstring",
                    failType ? realValue : new SoapHexBinary(failEncoding(realBytes)),
                    stringValue
                };

            return TestWithRobot<StreamRoot>(
                async consumer =>
                {
                    await Task.Delay(1000);
                    var root = consumer.Root;
                    Assert.AreEqual(boolValue, root.BooleanParameter.Value);

                    // Assert.AreEqual fails if types are different. At this point we don't know the original type of
                    // intValue or enumValue and we cannot cast object to int if the object happens to be a byte, for
                    // example. We therefore need to use Convert rather than a cast.
                    Assert.AreEqual(Convert.ToInt64(intValue), root.IntegerParameter.Value);
                    Assert.AreEqual((Enumeration)Convert.ToInt32(enumValue.ToString()), root.EnumerationParameter.Value);
                    CollectionAssert.AreEqual(octetStringValue, root.OctetstringParameter.Value);
                    Assert.AreEqual(Convert.ToDouble(realValue), root.RealParameter.Value);
                    Assert.AreEqual(stringValue, root.StringParameter.Value);
                },
                false,
                "StreamLog.xml",
                args);
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
        private sealed class MatrixRoot : Root<MatrixRoot>
        {
            [Element(Identifier = "SDN")]
            public Sdn Sdn { get; private set; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
        private sealed class Sdn : FieldNode<Sdn>
        {
            public Switching Switching { get; private set; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
        private sealed class Switching : FieldNode<Switching>
        {
            [Element(Identifier = "Matrix-0")]
            public Matrix0 Matrix0 { get; private set; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
        private sealed class Matrix0 : FieldNode<Matrix0>
        {
            [Element(Identifier = "Matrix-0")]
            public IMatrix Matrix { get; private set; }

            [Element(Identifier = "labels", IsOptional = true)]
            public CollectionNode<MatrixLabels> Labels { get; private set; }

            [Element(Identifier = "parameters", IsOptional = true)]
            public MatrixParameters<INode, INode, INode> Parameters { get; private set; }
        }
    }
}
