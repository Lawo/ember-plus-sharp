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
        public IReadOnlyList<int> ParametersLocation
        {
            get { return this.parametersLocation; }
            private set { this.SetValue(ref this.parametersLocation, value); }
        }

        /// <inheritdoc/>
        public int? GainParameterNumber
        {
            get { return this.gainParameterNumber; }
            private set { this.SetValue(ref this.gainParameterNumber, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<MatrixLabel> Labels
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
            get
            {
                return this.connections;
            }

            private set
            {
                if (this.SetValue(ref this.connections, value))
                {
                    foreach (var connection in this.connections)
                    {
                        connection.Value.CollectionChanged += (s, e) => this.OnSourcesChanged(connection.Key);
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal MatrixBase()
            : base(RetrievalState.None)
        {
        }

        internal sealed override bool WriteRequest(EmberWriter writer, IStreamedParameterCollection streamedParameters)
        {
            if (this.RetrievalState.Equals(RetrievalState.None))
            {
                writer.WriteStartApplicationDefinedType(
                    GlowElementCollection.Element.OuterId, GlowQualifiedMatrix.InnerNumber);
                writer.WriteValue(GlowQualifiedMatrix.Path.OuterId, this.NumberPath);
                writer.WriteStartApplicationDefinedType(
                    GlowQualifiedMatrix.Children.OuterId, GlowElementCollection.InnerNumber);
                this.WriteCommandCollection(writer, GlowCommandNumber.GetDirectory, RetrievalState.RequestSent);
                writer.WriteEndContainer();
                writer.WriteEndContainer();
                return true;
            }

            return false;
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
                        this.ParametersLocation = this.ReadParametersLocation(reader);
                        break;
                    case GlowMatrixContents.GainParameterNumber.OuterNumber:
                        this.GainParameterNumber = this.ReadInt(reader, GlowMatrixContents.GainParameterNumber.Name);
                        break;
                    case GlowMatrixContents.Labels.OuterNumber:
                        this.Labels = ReadLabels(reader);
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

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override RetrievalState ReadAdditionalField(EmberReader reader, int contextSpecificOuterNumber)
        {
            this.isProviderChangeInProgress = true;

            try
            {
                switch (contextSpecificOuterNumber)
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
                        break;
                    case GlowMatrix.Connections.OuterNumber:
                        this.ReadConnections(reader);
                        this.RetrievalState = RetrievalState.Complete;
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
            finally
            {
                this.isProviderChangeInProgress = false;
            }

            return this.RetrievalState;
        }

        internal sealed override void WriteChanges(EmberWriter writer, IInvocationCollection pendingInvocations)
        {
            if (this.HasChanges)
            {
                writer.WriteStartApplicationDefinedType(
                    GlowElementCollection.Element.OuterId, GlowQualifiedMatrix.InnerNumber);

                writer.WriteValue(GlowQualifiedMatrix.Path.OuterId, this.NumberPath);
                writer.WriteStartSequence(GlowQualifiedMatrix.Connections.OuterId);

                foreach (var target in this.targetsWithChangedConnections)
                {
                    writer.WriteStartApplicationDefinedType(
                        GlowConnectionCollection.Connection.OuterId, GlowConnection.InnerNumber);
                    writer.WriteValue(GlowConnection.Target.OuterId, target);
                    writer.WriteValue(GlowConnection.Sources.OuterId, this.connections[target].ToArray());
                    writer.WriteEndContainer();
                }

                writer.WriteEndContainer();
                writer.WriteEndContainer();
                this.targetsWithChangedConnections.Clear();
                this.HasChanges = false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static IReadOnlyList<MatrixLabel> ReadLabels(EmberReader reader)
        {
            reader.AssertInnerNumber(GlowLabelCollection.InnerNumber);
            var result = new List<MatrixLabel>();

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
                            result.Add(new MatrixLabel(basePath, description));
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

            if (replace)
            {
                // Remove all elements in existingSources not contained in sources
                var index = 0;

                while (index < existingSources.Count)
                {
                    if (Array.BinarySearch(sources, existingSources[index]) < 0)
                    {
                        existingSources.RemoveAt(index);
                    }
                    else
                    {
                        ++index;
                    }
                }
            }

            // Insert sources elements into existingSources, but skip elements already present in existingSources
            var insertIndex = 0;

            foreach (var source in sources)
            {
                int? existingSource = null;

                while ((insertIndex < existingSources.Count) && ((existingSource = existingSources[insertIndex]) < source))
                {
                    ++insertIndex;
                }

                if (source != existingSource)
                {
                    existingSources.Insert(insertIndex, source);
                }
            }
        }

        private readonly HashSet<int> targetsWithChangedConnections = new HashSet<int>();
        private int maximumTotalConnects;
        private int maximumConnectsPerTarget;
        private IReadOnlyList<int> parametersLocation;
        private int? gainParameterNumber;
        private IReadOnlyList<MatrixLabel> labels;
        private IReadOnlyList<int> targets;
        private IReadOnlyList<int> sources;
        private IReadOnlyDictionary<int, ObservableCollection<int>> connections;
        private bool isProviderChangeInProgress;

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

        private void OnSourcesChanged(int target)
        {
            if (!this.isProviderChangeInProgress)
            {
                this.HasChanges = true;
                this.targetsWithChangedConnections.Add(target);
            }
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

                        if (target.HasValue && (disposition != ConnectionDisposition.Pending) && !this.HasChanges)
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
