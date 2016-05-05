////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml;

    using Ember;
    using Model;
    using S101;

    /// <summary>Converts the log written by an <see cref="S101Logger"/> instantiated with
    /// <see cref="GlowTypes.Instance"/> into an equivalent log the payloads of which can be much more easily read
    /// by humans.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class GlowLogConverter
    {
        /// <summary>Converts the XML read with <paramref name="logReader"/> and writes it into
        /// <paramref name="logWriter"/>.</summary>
        public static void Convert(XmlReader logReader, XmlWriter logWriter)
        {
            if (logWriter == null)
            {
                throw new ArgumentNullException(nameof(logWriter));
            }

            var interpreter = new GlowLogInterpreter(GlowTypes.Instance, logReader);
            var converter = new Converter(interpreter, logWriter);

            using (var logger = new S101Logger(converter, logWriter))
            {
                var dummy = new byte[0];

                while (interpreter.Read())
                {
                    logger.LogMessage(interpreter.TimeUtc, interpreter.Direction, interpreter.Message, dummy);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private sealed class Converter : IEmberConverter
        {
            public void ToXml(byte[] dummyBuffer, XmlWriter dummyWriter) => this.interpreter.ApplyPayload();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal Converter(GlowLogInterpreter interpreter, XmlWriter writer)
            {
                this.interpreter = interpreter;
                this.writer = writer;
                this.Add(this.interpreter.Root);
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private static string LowerFirst(string str) => char.ToLowerInvariant(str[0]) + str.Substring(1);

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "There's no meaningful way to reduce the complexity.")]
            [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "There is only one cast per method call, CA bug?")]
            private static object GetValue(IElement element, string name)
            {
                switch (name)
                {
                    case "Description":
                        return element.Description;
                    case "IsOnline":
                        return element.IsOnline;
                    case "SchemaIdentifiers":
                        return ((IElementWithSchemas)element).SchemaIdentifiers;
                    case "Value":
                        var parameter = (IParameter)element;
                        return (parameter.Access & ParameterAccess.Read) == 0 ? null : parameter.Value;
                    case "Minimum":
                        return ((IParameter)element).Minimum;
                    case "Maximum":
                        return ((IParameter)element).Maximum;
                    case "Access":
                        return LowerFirst(((IParameter)element).Access.ToString());
                    case "Format":
                        return ((IParameter)element).Format;
                    case "Factor":
                        return ((IParameter)element).Factor;
                    case "Formula":
                        return ((IParameter)element).Formula;
                    case "DefaultValue":
                        return ((IParameter)element).DefaultValue;
                    case "Type":
                        return LowerFirst(((IParameter)element).Type.ToString());
                    case "StreamIdentifier":
                        return ((IStreamedParameter)element).StreamIdentifier;
                    case "EnumMap":
                        return ((IParameter)element).EnumMap;
                    case "StreamDescriptor":
                        return ((IStreamedParameter)element).StreamDescriptor;
                    case "Arguments":
                        return ((IFunction)element).Arguments;
                    case "Result":
                        return ((IFunction)element).Result;
                    case "IsRoot":
                        return ((INode)element).IsRoot;
                    default:
                        throw new ArgumentException("Unknown element or field name.");
                }
            }

            private readonly HashSet<IElement> subscribedElements = new HashSet<IElement>();
            private readonly GlowLogInterpreter interpreter;
            private readonly XmlWriter writer;

            private void Add(IElement element)
            {
                if (this.subscribedElements.Add(element))
                {
                    var node = element as INode;

                    if (element != this.interpreter.Root)
                    {
                        if (node != null)
                        {
                            this.WriteInitSequence(node);
                        }
                        else
                        {
                            var parameter = element as IParameter;

                            if (parameter != null)
                            {
                                this.WriteInitSequence(parameter);
                            }
                            else
                            {
                                this.WriteInitSequence((IFunction)element);
                            }
                        }

                        element.PropertyChanged += this.OnPropertyChanged;
                    }

                    if (node != null)
                    {
                        this.AddChildren(node.Children);
                        ((INotifyCollectionChanged)node.Children).CollectionChanged += this.OnCollectionChanged;
                    }
                }
            }

            private void WriteInitSequence(IParameter parameter)
            {
                this.WriteOperation("Init", parameter, "Description");
                this.WriteOperation("Init", parameter, "Value");
                this.WriteOperation("Init", parameter, "Minimum");
                this.WriteOperation("Init", parameter, "Maximum");
                this.WriteOperation("Init", parameter, "Access");
                this.WriteOperation("Init", parameter, "Format");
                this.WriteOperation("Init", parameter, "Factor");
                this.WriteOperation("Init", parameter, "IsOnline");
                this.WriteOperation("Init", parameter, "Formula");
                this.WriteOperation("Init", parameter, "DefaultValue");
                this.WriteOperation("Init", parameter, "Type");
                this.WriteOperation("Init", parameter, "StreamIdentifier");
                this.WriteOperation("Init", parameter, "EnumMap");
                this.WriteOperation("Init", parameter, "StreamDescriptor");
                this.WriteOperation("Init", parameter, "SchemaIdentifiers");
            }

            private void WriteInitSequence(IFunction function)
            {
                this.WriteOperation("Init", function, "Description");
                this.WriteOperation("Init", function, "Arguments");
                this.WriteOperation("Init", function, "Result");
            }

            private void WriteInitSequence(INode node)
            {
                this.WriteOperation("Init", node, "Description");
                this.WriteOperation("Init", node, "IsRoot");
                this.WriteOperation("Init", node, "IsOnline");
                this.WriteOperation("Init", node, "SchemaIdentifiers");
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                this.WriteOperation("Set", (IElement)sender, e.PropertyName);

            private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    this.AddChildren(e.NewItems);
                }

                // We intentionally do nothing for all other cases.
            }

            private void WriteOperation(string operation, IElement element, string propertyName)
            {
                this.writer.WriteStartElement(operation);
                var fieldName = LowerFirst(propertyName);
                this.writer.WriteAttributeString("path", element.GetPath() + "." + fieldName);
                var value = GetValue(element, propertyName);

                if (value != null)
                {
                    switch (propertyName)
                    {
                        case "EnumMap":
                            var map = (IReadOnlyList<KeyValuePair<string, int>>)value;
                            this.writer.WriteStartElement("StringIntegerCollection");

                            foreach (var pair in map)
                            {
                                this.writer.WriteStartElement("StringIntegerPair");
                                this.writer.WriteAttributeString("entryString", pair.Key);
                                this.writer.WriteAttributeString(
                                    "entryInteger", pair.Value.ToString(CultureInfo.InvariantCulture));

                                this.writer.WriteEndElement();
                            }

                            this.writer.WriteEndElement();
                            break;
                        case "StreamDescriptor":
                            var description = (StreamDescription)value;
                            this.writer.WriteStartElement("StreamDescription");
                            this.writer.WriteAttributeString("format", description.Format.ToString());
                            this.writer.WriteAttributeString(
                                "offset", description.Offset.ToString(CultureInfo.InvariantCulture));

                            this.writer.WriteEndElement();
                            break;
                        case "Arguments":
                        case "Result":
                            this.WriteTuple(value);
                            break;
                        default:
                            this.writer.WriteValue(value);
                            break;
                    }
                }

                this.writer.WriteEndElement();
            }

            private void AddChildren(IList list)
            {
                foreach (IElement element in list)
                {
                    this.Add(element);
                }
            }

            private void WriteTuple(object value)
            {
                var arguments = (IReadOnlyList<KeyValuePair<string, ParameterType>>)value;
                this.writer.WriteStartElement("TupleDescription");

                foreach (var pair in arguments)
                {
                    this.writer.WriteStartElement("TupleItemDescription");
                    this.writer.WriteAttributeString("type", pair.Value.ToString());
                    this.writer.WriteAttributeString("name", pair.Key);
                    this.writer.WriteEndElement();
                }

                this.writer.WriteEndElement();
            }
        }
    }
}
