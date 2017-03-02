////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml;

    using Ember;
    using Model;
    using S101;

    using static System.Globalization.CultureInfo;

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
                    case nameof(element.Description):
                        return element.Description;
                    case nameof(element.IsOnline):
                        return element.IsOnline;
                    case nameof(IElementWithSchemas.SchemaIdentifiers):
                        return Join(Environment.NewLine, ((IElementWithSchemas)element).SchemaIdentifiers);
                    case nameof(IParameter.Value):
                        var parameter = (IParameter)element;
                        return (parameter.Access & ParameterAccess.Read) == 0 ? null : parameter.Value;
                    case nameof(IParameter.Minimum):
                        return ((IParameter)element).Minimum;
                    case nameof(IParameter.Maximum):
                        return ((IParameter)element).Maximum;
                    case nameof(IParameter.Access):
                        return LowerFirst(((IParameter)element).Access.ToString());
                    case nameof(IParameter.Format):
                        return ((IParameter)element).Format;
                    case nameof(IParameter.Factor):
                        return ((IParameter)element).Factor;
                    case nameof(IParameter.Formula):
                        return ((IParameter)element).Formula;
                    case nameof(IParameter.DefaultValue):
                        return ((IParameter)element).DefaultValue;
                    case nameof(IParameter.Type):
                        return LowerFirst(((IParameter)element).Type.ToString());
                    case nameof(IStreamedParameter.StreamIdentifier):
                        return ((IStreamedParameter)element).StreamIdentifier;
                    case nameof(IParameter.EnumMap):
                        return ((IParameter)element).EnumMap;
                    case nameof(IStreamedParameter.StreamDescriptor):
                        return ((IStreamedParameter)element).StreamDescriptor;
                    case nameof(INode.IsRoot):
                        return ((INode)element).IsRoot;
                    case nameof(IFunction.Arguments):
                        return ((IFunction)element).Arguments;
                    case nameof(IFunction.Result):
                        return ((IFunction)element).Result;
                    case nameof(IMatrix.MaximumTotalConnects):
                        return ((IMatrix)element).MaximumTotalConnects;
                    case nameof(IMatrix.MaximumConnectsPerTarget):
                        return ((IMatrix)element).MaximumConnectsPerTarget;
                    case nameof(IMatrix.ParametersLocation):
                        return Join(".", ((IMatrix)element).ParametersLocation);
                    case nameof(IMatrix.GainParameterNumber):
                        return ((IMatrix)element).GainParameterNumber;
                    case nameof(IMatrix.Labels):
                        return ((IMatrix)element).Labels;
                    case nameof(IMatrix.Targets):
                        return ((IMatrix)element).Targets;
                    case nameof(IMatrix.Sources):
                        return ((IMatrix)element).Sources;
                    case nameof(IMatrix.Connections):
                        return ((IMatrix)element).Connections;
                    default:
                        throw new ArgumentException("Unknown element or field name.");
                }
            }

            private static string Join<T>(string separator, IEnumerable<T> values)
                where T : IFormattable
            {
                return values == null ? null : Join(separator, values.Select(l => l.ToString(null, InvariantCulture)));
            }

            private static string Join(string separator, IEnumerable<string> values) =>
                values == null ? null : string.Join(separator, values);

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
                                var function = element as IFunction;

                                if (function != null)
                                {
                                    this.WriteInitSequence(function);
                                }
                                else
                                {
                                    this.WriteInitSequence((IMatrix)element);
                                }
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

            private void WriteInitSequence(INode node)
            {
                this.WriteOperation("Init", node, nameof(node.Description));
                this.WriteOperation("Init", node, nameof(node.IsRoot));
                this.WriteOperation("Init", node, nameof(node.IsOnline));
                this.WriteOperation("Init", node, nameof(node.SchemaIdentifiers));
            }

            private void WriteInitSequence(IParameter parameter)
            {
                this.WriteOperation("Init", parameter, nameof(parameter.Description));
                this.WriteOperation("Init", parameter, nameof(parameter.Value));
                this.WriteOperation("Init", parameter, nameof(parameter.Minimum));
                this.WriteOperation("Init", parameter, nameof(parameter.Maximum));
                this.WriteOperation("Init", parameter, nameof(parameter.Access));
                this.WriteOperation("Init", parameter, nameof(parameter.Format));
                this.WriteOperation("Init", parameter, nameof(parameter.Factor));
                this.WriteOperation("Init", parameter, nameof(parameter.IsOnline));
                this.WriteOperation("Init", parameter, nameof(parameter.Formula));
                this.WriteOperation("Init", parameter, nameof(parameter.DefaultValue));
                this.WriteOperation("Init", parameter, nameof(parameter.Type));
                this.WriteOperation("Init", parameter, nameof(IStreamedParameter.StreamIdentifier));
                this.WriteOperation("Init", parameter, nameof(parameter.EnumMap));
                this.WriteOperation("Init", parameter, nameof(IStreamedParameter.StreamDescriptor));
                this.WriteOperation("Init", parameter, nameof(parameter.SchemaIdentifiers));
            }

            private void WriteInitSequence(IFunction function)
            {
                this.WriteOperation("Init", function, nameof(function.Description));
                this.WriteOperation("Init", function, nameof(function.Arguments));
                this.WriteOperation("Init", function, nameof(function.Result));
            }

            private void WriteInitSequence(IMatrix matrix)
            {
                this.WriteOperation("Init", matrix, nameof(matrix.Description));
                this.WriteOperation("Init", matrix, nameof(matrix.MaximumTotalConnects));
                this.WriteOperation("Init", matrix, nameof(matrix.MaximumConnectsPerTarget));
                this.WriteOperation("Init", matrix, nameof(matrix.ParametersLocation));
                this.WriteOperation("Init", matrix, nameof(matrix.GainParameterNumber));
                this.WriteOperation("Init", matrix, nameof(matrix.Labels));
                this.WriteOperation("Init", matrix, nameof(matrix.SchemaIdentifiers));
                this.WriteOperation("Init", matrix, nameof(matrix.Targets));
                this.WriteOperation("Init", matrix, nameof(matrix.Sources));
                this.WriteOperation("Init", matrix, nameof(matrix.Connections));
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
                        case nameof(IParameter.EnumMap):
                            this.WriteEnumMap(value);
                            break;
                        case nameof(IStreamedParameter.StreamDescriptor):
                            this.WriteStreamDescriptor(value);
                            break;
                        case nameof(IFunction.Arguments):
                        case nameof(IFunction.Result):
                            this.WriteTuple(value);
                            break;
                        case nameof(IMatrix.Labels):
                            this.WriteLabels(value);
                            break;
                        case nameof(IMatrix.Targets):
                            this.WriteSignals(value, "TargetCollection", "Target");
                            break;
                        case nameof(IMatrix.Sources):
                            this.WriteSignals(value, "SourceCollection", "Source");
                            break;
                        case nameof(IMatrix.Connections):
                            this.WriteConnections(value);
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

            private void WriteEnumMap(object value)
            {
                this.writer.WriteStartElement("StringIntegerCollection");

                foreach (var pair in (IReadOnlyList<KeyValuePair<string, int>>)value)
                {
                    this.writer.WriteStartElement("StringIntegerPair");
                    this.writer.WriteAttributeString("entryString", pair.Key);
                    this.writer.WriteAttributeString("entryInteger", pair.Value.ToString(InvariantCulture));
                    this.writer.WriteEndElement();
                }

                this.writer.WriteEndElement();
            }

            private void WriteStreamDescriptor(object value)
            {
                this.writer.WriteStartElement("StreamDescription");
                var description = (StreamDescription)value;
                this.writer.WriteAttributeString("format", description.Format.ToString());
                this.writer.WriteAttributeString("offset", description.Offset.ToString(InvariantCulture));
                this.writer.WriteEndElement();
            }

            private void WriteTuple(object value)
            {
                this.writer.WriteStartElement("TupleDescription");

                foreach (var pair in (IReadOnlyList<KeyValuePair<string, ParameterType>>)value)
                {
                    this.writer.WriteStartElement("TupleItemDescription");
                    this.writer.WriteAttributeString("type", pair.Value.ToString());
                    this.writer.WriteAttributeString("name", pair.Key);
                    this.writer.WriteEndElement();
                }

                this.writer.WriteEndElement();
            }

            private void WriteLabels(object value)
            {
                this.writer.WriteStartElement("LabelCollection");

                foreach (var label in (IReadOnlyList<MatrixLabel>)value)
                {
                    this.writer.WriteStartElement("Label");
                    this.writer.WriteAttributeString("basePath", Join(".", label.BasePath));
                    this.writer.WriteAttributeString("description", label.Description);
                    this.writer.WriteEndElement();
                }

                this.writer.WriteEndElement();
            }

            private void WriteSignals(object value, string collectionName, string itemName)
            {
                this.writer.WriteStartElement(collectionName);

                foreach (var signal in (IReadOnlyList<int>)value)
                {
                    this.writer.WriteStartElement(itemName);
                    this.writer.WriteAttributeString("number", signal.ToString(InvariantCulture));
                    this.writer.WriteEndElement();
                }

                this.writer.WriteEndElement();
            }

            private void WriteConnections(object value)
            {
                this.writer.WriteStartElement("ConnectionCollection");

                foreach (var connection in (IReadOnlyDictionary<int, ObservableCollection<int>>)value)
                {
                    this.writer.WriteStartElement("Connection");
                    this.writer.WriteAttributeString("target", connection.Key.ToString(InvariantCulture));
                    this.writer.WriteAttributeString("sources", Join(" ", connection.Value));
                    this.writer.WriteEndElement();
                }

                this.writer.WriteEndElement();
            }
        }
    }
}
