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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Ember;
    using Glow;

    /// <summary>This API is not intended to be used directly from your code.</summary>
    /// <remarks>Provides common implementation for all matrices in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class MatrixBase<TMostDerived> :
        ElementWithSchemas<TMostDerived>, IMatrix
        where TMostDerived : MatrixBase<TMostDerived>
    {
        /// <inheritdoc/>
        public int MaximumTotalConnects
        {
            get { return this.maximumTotalConnects; }
            private set { this.SetValue(ref this.maximumTotalConnects, value); }
        }

        /// <inheritdoc/>
        public int MaximumConnectsPerTarget
        {
            get { return this.maximumConnectsPerTarget; }
            private set { this.SetValue(ref this.maximumConnectsPerTarget, value); }
        }

        /// <inheritdoc/>
        public int? GainParameterNumber
        {
            get { return this.gainParameterNumber; }
            private set { this.SetValue(ref this.gainParameterNumber, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<KeyValuePair<string, MatrixLabels>> Labels
        {
            get { return this.labels; }
            private set { this.SetValue(ref this.labels, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<int> Targets
        {
            get { return this.targets; }
            private set { this.SetValue(ref this.targets, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<int> Sources
        {
            get { return this.sources; }
            private set { this.SetValue(ref this.sources, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<int, ObservableCollection<int>> Connections
        {
            get { return this.connections; }
            private set { this.SetValue(ref this.connections, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Required property is provided by subclasses.")]
        INode IMatrix.Parameters => this.GetParameters();

        internal MatrixBase()
            : base(RetrievalState.None)
        {
        }

        internal abstract INode GetParameters();

        internal sealed override bool WriteRequest(EmberWriter writer, IStreamedParameterCollection streamedParameters)
        {
            if (this.RetrievalState.Equals(RetrievalState.None))
            {
                if (!this.isMatrixComplete)
                {
                    writer.WriteStartApplicationDefinedType(
                        GlowElementCollection.Element.OuterId, GlowQualifiedMatrix.InnerNumber);
                    writer.WriteValue(GlowQualifiedMatrix.Path.OuterId, this.NumberPath);
                    writer.WriteStartApplicationDefinedType(
                        GlowQualifiedMatrix.Children.OuterId, GlowElementCollection.InnerNumber);
                    this.WriteCommandCollection(writer, GlowCommandNumber.GetDirectory, RetrievalState.RequestSent);
                    writer.WriteEndContainer();
                    writer.WriteEndContainer();
                }

                // TODO: Write GetDirectory request for parameters and labels, if present.
            }

            return true;
        }

        internal sealed override RetrievalState ReadContents(EmberReader reader, ElementType actualType)
        {
            this.AssertElementType(ElementType.Matrix, actualType);
            var type = MatrixType.OneToN;
            var addressingMode = MatrixAddressingMode.Linear;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowMatrixContents.Description.OuterNumber:
                        this.Description = reader.AssertAndReadContentsAsString();
                        break;
                    case GlowMatrixContents.Type.OuterNumber:
                        type = this.ReadEnum<MatrixType>(reader, GlowMatrixContents.Type.Name);
                        break;
                    case GlowMatrixContents.AddressingMode.OuterNumber:
                        addressingMode =
                            this.ReadEnum<MatrixAddressingMode>(reader, GlowMatrixContents.AddressingMode.Name);
                        break;
                    case GlowMatrixContents.TargetCount.OuterNumber:
                        this.Targets =
                            Enumerable.Range(0, this.ReadInt(reader, GlowMatrixContents.TargetCount.Name)).ToList();
                        break;
                    case GlowMatrixContents.SourceCount.OuterNumber:
                        this.Sources =
                            Enumerable.Range(0, this.ReadInt(reader, GlowMatrixContents.SourceCount.Name)).ToList();
                        break;
                    case GlowMatrixContents.MaximumTotalConnects.OuterNumber:
                        this.MaximumTotalConnects = this.ReadInt(reader, GlowMatrixContents.MaximumTotalConnects.Name);
                        break;
                    case GlowMatrixContents.MaximumConnectsPerTarget.OuterNumber:
                        this.MaximumConnectsPerTarget =
                            this.ReadInt(reader, GlowMatrixContents.MaximumConnectsPerTarget.Name);
                        break;
                    case GlowMatrixContents.ParametersLocation.OuterNumber:
                        var parametersLocation = this.ReadParametersLocation(reader);
                        break;
                    case GlowMatrixContents.GainParameterNumber.OuterNumber:
                        var gainParameterNumber = this.ReadInt(reader, GlowMatrixContents.GainParameterNumber.Name);
                        break;
                    case GlowMatrixContents.Labels.OuterNumber:
                        var labels = ReadLabels(reader);
                        break;
                    case GlowMatrixContents.SchemaIdentifiers.OuterNumber:
                        this.ReadSchemaIdentifiers(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            if ((this.Targets != null) && (this.Sources != null))
            {
                this.Connections = this.Targets.ToDictionary(i => i, i => new ObservableCollection<int>());

                if (this.MaximumTotalConnects == 0)
                {
                    this.MaximumTotalConnects =
                        GetMaximumTotalConnects(type, this.Targets.Count, this.Sources.Count);
                }

                if (this.MaximumConnectsPerTarget == 0)
                {
                    this.MaximumConnectsPerTarget = type == MatrixType.NToN ? this.Sources.Count : 1;
                }
            }

            return this.RetrievalState;
        }

        internal sealed override void ReadAdditionalFields(EmberReader reader)
        {
            switch (reader.GetContextSpecificOuterNumber())
            {
                case GlowMatrix.Targets.OuterNumber:
                    reader.AssertInnerNumber(GlowTargetCollection.InnerNumber);
                    this.Targets = this.ReadSignals(
                        reader,
                        this.Targets,
                        GlowTargetCollection.Target.OuterNumber,
                        GlowTarget.InnerNumber,
                        GlowTarget.Number.OuterNumber,
                        GlowTarget.Number.Name);
                    this.Connections = this.Targets.ToDictionary(i => i, i => new ObservableCollection<int>());
                    this.isMatrixComplete = true;
                    break;
                case GlowMatrix.Sources.OuterNumber:
                    reader.AssertInnerNumber(GlowSourceCollection.InnerNumber);
                    this.Sources = this.ReadSignals(
                        reader,
                        this.Sources,
                        GlowSourceCollection.Source.OuterNumber,
                        GlowSource.InnerNumber,
                        GlowSource.Number.OuterNumber,
                        GlowSource.Number.Name);
                    this.isMatrixComplete = true;
                    break;
                case GlowMatrix.Connections.OuterNumber:
                    this.ReadConnections(reader);
                    this.isMatrixComplete = true;
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        internal sealed override void WriteChanges(EmberWriter writer, IInvocationCollection pendingInvocations)
        {
            ////if (this.HasChanges)
            ////{
            ////    writer.WriteStartApplicationDefinedType(
            ////        GlowElementCollection.Element.OuterId, GlowQualifiedParameter.InnerNumber);

            ////    writer.WriteValue(GlowQualifiedParameter.Path.OuterId, this.NumberPath);
            ////    writer.WriteStartSet(GlowQualifiedParameter.Contents.OuterId);

            ////    if (this.theValue == null)
            ////    {
            ////        // This can only happen when the parameter happens to be a trigger.
            ////        writer.WriteValue(GlowParameterContents.Value.OuterId, 0);
            ////    }
            ////    else
            ////    {
            ////        this.WriteValue(writer, this.theValue);
            ////    }

            ////    writer.WriteEndContainer();
            ////    writer.WriteEndContainer();
            ////    this.HasChanges = false;
            ////}
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static IReadOnlyList<KeyValuePair<int[], string>> ReadLabels(EmberReader reader)
        {
            reader.AssertInnerNumber(GlowLabelCollection.InnerNumber);
            var result = new List<KeyValuePair<int[], string>>();

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowLabelCollection.Label.OuterNumber:
                        reader.AssertInnerNumber(GlowLabel.InnerNumber);

                        int[] basePath = null;
                        string description = null;

                        while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
                        {
                            switch (reader.GetContextSpecificOuterNumber())
                            {
                                case GlowLabel.BasePath.OuterNumber:
                                    basePath = reader.AssertAndReadContentsAsInt32Array();
                                    break;
                                case GlowLabel.Description.OuterNumber:
                                    description = reader.AssertAndReadContentsAsString();
                                    break;
                                default:
                                    reader.Skip();
                                    break;
                            }
                        }

                        if ((basePath != null) && (description != null))
                        {
                            result.Add(new KeyValuePair<int[], string>(basePath, description));
                        }

                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return result;
        }

        private static int GetMaximumTotalConnects(MatrixType type, int targetCount, int sourceCount)
        {
            switch (type)
            {
                case MatrixType.OneToN:
                    return targetCount;
                case MatrixType.OneToOne:
                    return Math.Min(sourceCount, targetCount);
                default:
                    return targetCount * sourceCount;
            }
        }

        private static void Insert(ObservableCollection<int> existingSources, int[] sources, bool replace)
        {
            Array.Sort(sources);
            int index = 0;

            foreach (var source in sources)
            {
                int? existingSource = null;

                while ((index < existingSources.Count) && ((existingSource = existingSources[index]) < source))
                {
                    if (replace)
                    {
                        existingSources.RemoveAt(index);
                    }
                    else
                    {
                        ++index;
                    }
                }

                if (existingSource != source)
                {
                    existingSources.Insert(index, source);
                }

                ++index;
            }
        }

        private int maximumTotalConnects;
        private int maximumConnectsPerTarget;
        private int? gainParameterNumber;
        private IReadOnlyList<KeyValuePair<string, MatrixLabels>> labels;
        private bool isMatrixComplete;
        private IReadOnlyList<int> targets;
        private IReadOnlyList<int> sources;
        private IReadOnlyDictionary<int, ObservableCollection<int>> connections;

        private enum MatrixType
        {
            OneToN,
            OneToOne,
            NToN
        }

        private enum MatrixAddressingMode
        {
            Linear,
            Nonlinear,
        }

        private enum ConnectionOperation
        {
            Absolute,
            Connect,
            Disconnect
        }

        private enum ConnectionDisposition
        {
            Tally,
            Modified,
            Pending,
            Locked
        }

        private int[] ReadParametersLocation(EmberReader reader)
        {
            if (reader.InnerNumber == InnerNumber.RelativeObjectIdentifier)
            {
                return reader.ReadContentsAsInt32Array();
            }
            else
            {
                var path = new int[this.NumberPath.Length + 1];
                Array.Copy(this.NumberPath, path, this.NumberPath.Length);
                path[this.NumberPath.Length] = this.ReadInt(reader, GlowMatrixContents.ParametersLocation.Name);
                return path;
            }
        }

        private IReadOnlyList<int> ReadSignals(
            EmberReader reader,
            IReadOnlyList<int> signals,
            int outerNumber,
            int innerNumber,
            int numberOuterNumber,
            string numberName)
        {
            List<int> result = new List<int>();

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                if (reader.GetContextSpecificOuterNumber() == outerNumber)
                {
                    reader.AssertInnerNumber(innerNumber);

                    while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
                    {
                        if (reader.GetContextSpecificOuterNumber() == numberOuterNumber)
                        {
                            result.Add(this.ReadInt(reader, numberName));
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }
                else
                {
                    reader.Skip();
                }
            }

            if (signals.Count != result.Count)
            {
                throw new ModelException("Inconsistent source or target counts in matrix.");
            }

            return result;
        }

        private void ReadConnections(EmberReader reader)
        {
            reader.AssertInnerNumber(GlowConnectionCollection.InnerNumber);

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowConnectionCollection.Connection.OuterNumber:
                        reader.AssertInnerNumber(GlowConnection.InnerNumber);
                        int? target = null;
                        int[] connectedSources = new int[0];
                        ConnectionOperation operation = ConnectionOperation.Absolute;
                        ConnectionDisposition disposition = ConnectionDisposition.Tally;

                        while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
                        {
                            switch (reader.GetContextSpecificOuterNumber())
                            {
                                case GlowConnection.Target.OuterNumber:
                                    target = this.ReadInt(reader, GlowConnection.Target.Name);
                                    break;
                                case GlowConnection.Sources.OuterNumber:
                                    connectedSources = reader.AssertAndReadContentsAsInt32Array();
                                    break;
                                case GlowConnection.Operation.OuterNumber:
                                    operation =
                                        this.ReadEnum<ConnectionOperation>(reader, GlowConnection.Operation.Name);
                                    break;
                                case GlowConnection.Disposition.OuterNumber:
                                    disposition =
                                        this.ReadEnum<ConnectionDisposition>(reader, GlowConnection.Disposition.Name);
                                    break;
                                default:
                                    reader.Skip();
                                    break;
                            }
                        }

                        if (target.HasValue && (disposition != ConnectionDisposition.Pending))
                        {
                            var existingConnectedSources = this.Connections[target.Value];

                            switch (operation)
                            {
                                case ConnectionOperation.Absolute:
                                    Insert(existingConnectedSources, connectedSources, true);
                                    break;
                                case ConnectionOperation.Connect:
                                    Insert(existingConnectedSources, connectedSources, false);
                                    break;
                                case ConnectionOperation.Disconnect:
                                    foreach (var source in connectedSources)
                                    {
                                        existingConnectedSources.Remove(source);
                                    }

                                    break;
                            }
                        }

                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }
    }
}
