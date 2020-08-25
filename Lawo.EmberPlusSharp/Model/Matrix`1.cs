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
    using System.Runtime.CompilerServices;
    using Ember;
    using Glow;

    /// <summary>Represents a matrix with optional static children in the object tree accessible
    /// through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>
    /// <para><typeparamref name="TMostDerived"/> must contain a property with a getter and a setter for each
    /// child of the represented matrix. The property getters and setters can have any accessibility. The name of each
    /// property must be equal to the identifier of the corresponding child, or carry an
    /// <see cref="ElementAttribute"/> to which the identifier is passed.</para>
    /// <para>The type of each <typeparamref name="TMostDerived"/> property must be of one of the following:
    /// <list type="bullet">
    /// <item><see cref="IParameter"/>.</item>
    /// <item><see cref="INode"/>.</item>
    /// <item><see cref="IFunction"/>.</item>
    /// <item><see cref="IMatrix"/>.</item>
    /// <item><see cref="BooleanParameter"/>.</item>
    /// <item><see cref="EnumParameter{TEnum}"/>.</item>
    /// <item><see cref="IntegerParameter"/>.</item>
    /// <item><see cref="OctetstringParameter"/>.</item>
    /// <item><see cref="RealParameter"/>.</item>
    /// <item><see cref="StringParameter"/>.</item>
    /// <item><see cref="NullableBooleanParameter"/>.</item>
    /// <item><see cref="NullableEnumParameter{TEnum}"/>.</item>
    /// <item><see cref="NullableIntegerParameter"/>.</item>
    /// <item><see cref="NullableOctetstringParameter"/>.</item>
    /// <item><see cref="NullableRealParameter"/>.</item>
    /// <item><see cref="NullableStringParameter"/>.</item>
    /// <item><see cref="CollectionNode{TElement}"/>.</item>
    /// <item>A <see cref="FieldNode{TMostDerived}"/> subtype.</item>
    /// <item>A <see cref="DynamicFieldNode{TMostDerived}"/> subtype.</item>
    /// <item>A <see cref="Matrix{TMostDerived}"/> subtype.</item>
    /// <item>A <see cref="DynamicMatrix{TMostDerived}"/> subtype.</item>
    /// <item><see cref="Function{T}"/>, <see cref="Function{T, U}"/>, <see cref="Function{T, U, V}"/>,
    /// <see cref="Function{T, U, V, W}"/>, <see cref="Function{T, U, V, W, X}"/>,
    /// <see cref="Function{T, U, V, W, X, Y}"/>, or <see cref="Function{T, U, V, W, X, Y, Z}"/>.</item>
    /// </list></para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class Matrix<TMostDerived> : FieldNode<TMostDerived>, IMatrix
        where TMostDerived : Matrix<TMostDerived>
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
            private set { this.SetSignals(ref this.targets, value, this.sources); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<int> Sources
        {
            get { return this.sources; }
            private set { this.SetSignals(ref this.sources, value, this.targets); }
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
                this.SetValue(ref this.connections, value);

                foreach (var connection in this.connections)
                {
                    connection.Value.CollectionChanged += (s, e) => this.OnSourcesChanged(connection.Key);
                }

                if (this.MaximumTotalConnects == 0)
                {
                    this.MaximumTotalConnects =
                        GetMaximumTotalConnects(this.type, this.Targets.Count, this.Sources.Count);
                }

                if (this.MaximumConnectsPerTarget == 0)
                {
                    this.MaximumConnectsPerTarget = this.type == MatrixType.NToN ? this.Sources.Count : 1;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal sealed override int FinalElementType => GlowQualifiedMatrix.InnerNumber;

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override RetrievalState ReadContents(EmberReader reader, ElementType actualType)
        {
            this.AssertElementType(ElementType.Matrix, actualType);
            var addressingMode = MatrixAddressingMode.Linear;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowMatrixContents.Description.OuterNumber:
                        this.Description = reader.AssertAndReadContentsAsString();
                        break;
                    case GlowMatrixContents.Type.OuterNumber:
                        this.type = this.ReadEnum<MatrixType>(reader, GlowMatrixContents.Type.Name);
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
                        this.connectionsRead = true;
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

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
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

        internal sealed override RetrievalState UpdateRetrievalState(bool throwForMissingRequiredChildren)
        {
            base.UpdateRetrievalState(throwForMissingRequiredChildren);

            // According to the specification, the provider is not obligated to send an empty children collection when
            // a matrix does not have any children. Since is must respond with at least the connections to a
            // GetDirectory command, a consumer has no easy way to determine whether all children (if any) of a matrix
            // have been received or not (for nodes without children the provider responds with just number or path and
            // nothing else). We cannot assume the matrix is complete when its connections have been received, as we
            // might still be missing direct or indirect children. The following code implements a heuristic, which
            // assumes that a provider would send the direct children (if any) before sending the connections.
            if (this.connectionsRead)
            {
                if (this.RetrievalState.Equals(RetrievalState.RequestSent) && (this.GetFirstIncompleteChild() == this))
                {
                    this.RetrievalState = this.AreRequiredChildrenAvailable(throwForMissingRequiredChildren) ?
                        RetrievalState.Verified : RetrievalState.Complete;
                }
            }
            else
            {
                this.RetrievalState &= RetrievalState.RequestSent;
            }

            return this.RetrievalState;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="Matrix{TMostDerived}"/> class.</summary>
        /// <remarks>
        /// <para>Objects of subtypes are not created by client code directly but indirectly when a
        /// <see cref="Consumer{T}"/> object is created.</para>
        /// </remarks>
        protected Matrix()
        {
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
        private MatrixType type = MatrixType.OneToN;
        private int maximumTotalConnects;
        private int maximumConnectsPerTarget;
        private IReadOnlyList<int> parametersLocation;
        private int? gainParameterNumber;
        private IReadOnlyList<MatrixLabel> labels;
        private IReadOnlyList<int> targets;
        private IReadOnlyList<int> sources;
        private IReadOnlyDictionary<int, ObservableCollection<int>> connections;
        private bool connectionsRead;
        private bool isProviderChangeInProgress;

        /// <summary>
        /// Defines the possible values of the "type" field of GlowMatrix
        /// </summary>
        private enum MatrixType
        {
            OneToN,
            OneToOne,
            NToN
        }

        /// <summary>
        /// Defines the possible values of the "addressingMode" field of GlowMatrix
        /// </summary>
        private enum MatrixAddressingMode
        {
            Linear,
            Nonlinear,
        }

        /// <summary>
        /// Defines the possible values of the "operation" field of GlowConnection
        /// </summary>
        private enum ConnectionOperation
        {
            Absolute,
            Connect,
            Disconnect
        }

        /// <summary>
        /// Defines the possible values of the "disposition" field of GlowConnection
        /// </summary>
        private enum ConnectionDisposition
        {
            Tally,
            Modified,
            Pending,
            Locked
        }

        private void SetSignals(
            ref IReadOnlyList<int> field,
            IReadOnlyList<int> signals,
            IReadOnlyList<int> other,
            [CallerMemberName] string propertyName = null)
        {
            if (field?.SequenceEqual(signals) != true)
            {
                this.SetValue(ref field, signals, propertyName);

                if (other != null)
                {
                    this.Connections = this.targets.ToDictionary(i => i, i => new ObservableCollection<int>());
                }
            }
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
