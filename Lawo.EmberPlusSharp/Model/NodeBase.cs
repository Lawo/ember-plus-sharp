////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Ember;
    using Glow;

    /// <summary>This API is not intended to be used directly from your code.</summary>
    /// <remarks>Provides common implementation for all nodes in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class NodeBase<TMostDerived> : ElementWithSchemas<TMostDerived>, IParent
        where TMostDerived : NodeBase<TMostDerived>
    {
        private readonly SortedDictionary<int, Element> children = new SortedDictionary<int, Element>();
        private ChildrenRetrievalPolicy childrenRetrievalPolicy;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <inheritdoc/>
        public ChildrenRetrievalPolicy ChildrenRetrievalPolicy
        {
            get
            {
                return this.childrenRetrievalPolicy;
            }

            set
            {
                if ((value < ChildrenRetrievalPolicy.None) || (value > ChildrenRetrievalPolicy.All))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value != this.childrenRetrievalPolicy)
                {
                    if (this.childrenRetrievalPolicy != ChildrenRetrievalPolicy.None)
                    {
                        throw new ArgumentException(
                            "A new value cannot be set if the current value is not equal to ChildrenRetrievalPolicy.None.",
                            "value");
                    }

                    this.SetValue(ref this.childrenRetrievalPolicy, value);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        int[] IParent.NumberPath
        {
            get { return this.NumberPath; }
        }

        void IParent.SetHasChanges()
        {
            this.HasChanges = true;
        }

        void IParent.AppendPath(StringBuilder builder)
        {
            this.AppendPath(builder);
        }

        void IParent.OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e);
        }

        internal NodeBase() : base(RetrievalState.None)
        {
        }

        internal sealed override void SetContext(Context context)
        {
            base.SetContext(context);
            this.childrenRetrievalPolicy = context.ChildrenRetrievalPolicy;
        }

        internal IElement GetChild(int number)
        {
            return this.children[number];
        }

        internal void ReadChild(EmberReader reader, ElementType actualType)
        {
            reader.ReadAndAssertOuter(GlowNode.Number.OuterId);
            this.ReadChild(reader, actualType, reader.AssertAndReadContentsAsInt32());
        }

        internal bool WriteCommandCollection(EmberWriter writer, IStreamedParameterCollection streamedParameters)
        {
            if (this.children.Count == 0)
            {
                this.WriteCommandCollection(writer, GlowCommandNumber.GetDirectory, RetrievalState.RequestSent);
                return true;
            }
            else
            {
                var result = false;

                foreach (var child in this.children.Values)
                {
                    // We want to avoid short-circuit logic, which is why we use | rather than ||.
                    result |= child.WriteRequest(writer, streamedParameters);
                }

                return result;
            }
        }

        internal void WriteChangesCollection(EmberWriter writer, IInvocationCollection pendingInvocations)
        {
            foreach (var child in this.children.Values)
            {
                child.WriteChanges(writer, pendingInvocations);
            }
        }

        internal virtual Element ReadNewChildContents(
            EmberReader reader, ElementType actualType, Context context, out RetrievalState childRetrievalState)
        {
            reader.SkipToEndContainer();
            childRetrievalState = RetrievalState.Complete;
            return null;
        }

        internal virtual bool ReadChildrenCore(EmberReader reader)
        {
            var isEmpty = true;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.InnerNumber)
                {
                    case GlowParameter.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Parameter);
                        break;
                    case GlowNode.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Node);
                        break;
                    case GlowFunction.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Function);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return isEmpty;
        }

        /// <summary>Changes the visibility of <paramref name="child"/>.</summary>
        /// <param name="child">The child to change the visibility for.</param>
        /// <returns><c>true</c> if the visibility has been changed; otherwise, <c>false</c>.</returns>
        internal virtual bool ChangeVisibility(Element child)
        {
            return false;
        }

        internal sealed override RetrievalState ReadChildren(EmberReader reader)
        {
            if (this.ReadChildrenCore(reader))
            {
                this.RetrievalState = RetrievalState.Complete;
            }

            return this.RetrievalState;
        }

        internal sealed override RetrievalState ReadQualifiedChild(
            EmberReader reader, ElementType actualType, int[] path, int index)
        {
            if (index == path.Length - 1)
            {
                this.ReadChild(reader, actualType, path[index]);
            }
            else
            {
                Element child;

                if (!this.children.TryGetValue(path[index], out child))
                {
                    reader.SkipToEndContainer();
                }
                else
                {
                    this.RetrievalState &= child.ReadQualifiedChild(reader, actualType, path, index + 1);
                }
            }

            return this.RetrievalState;
        }

        internal sealed override RetrievalState UpdateRetrievalState(bool throwForMissingRequiredChildren)
        {
            if (!this.RetrieveDetails || (this.children.Count == 0))
            {
                return base.UpdateRetrievalState(throwForMissingRequiredChildren);
            }
            else
            {
                if (!this.RetrievalState.Equals(RetrievalState.Verified))
                {
                    var accumulatedChildRetrievalState = RetrievalState.Verified;

                    foreach (var child in this.children.Values)
                    {
                        var childRetrievalState = child.UpdateRetrievalState(throwForMissingRequiredChildren);
                        accumulatedChildRetrievalState &= childRetrievalState;

                        if (child.RetrieveDetailsChangeStatus != RetrieveDetailsChangeStatus.Unchanged)
                        {
                            if ((child.RetrieveDetails && childRetrievalState.Equals(RetrievalState.Verified)) ||
                                !child.RetrieveDetails)
                            {
                                child.RetrieveDetailsChangeStatus = RetrieveDetailsChangeStatus.Unchanged;
                                this.ChangeVisibility(child);
                            }
                        }
                    }

                    this.RetrievalState =
                        this.GetRetrievalState(throwForMissingRequiredChildren, accumulatedChildRetrievalState);
                }

                return this.RetrievalState;
            }
        }

        internal override bool WriteRequest(EmberWriter writer, IStreamedParameterCollection streamedParameters)
        {
            if (this.RetrievalState.Equals(RetrievalState.None))
            {
                var isEmpty = this.children.Count == 0;

                if (isEmpty)
                {
                    writer.WriteStartApplicationDefinedType(
                        GlowElementCollection.Element.OuterId, GlowQualifiedNode.InnerNumber);
                    writer.WriteValue(GlowQualifiedNode.Path.OuterId, this.NumberPath);
                    writer.WriteStartApplicationDefinedType(
                        GlowQualifiedNode.Children.OuterId, GlowElementCollection.InnerNumber);
                }

                var result = this.WriteCommandCollection(writer, streamedParameters);

                if (isEmpty)
                {
                    writer.WriteEndContainer();
                    writer.WriteEndContainer();
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        internal sealed override void SetComplete()
        {
            foreach (var child in this.children.Values)
            {
                child.SetComplete();
            }

            base.SetComplete();
        }

        internal sealed override IParent GetFirstIncompleteChild()
        {
            if (this.RetrievalState.Equals(RetrievalState.RequestSent))
            {
                return this.children.Count == 0 ? this :
                    this.children.Values.Select(c => c.GetFirstIncompleteChild()).FirstOrDefault(c => c != null);
            }
            else
            {
                return base.GetFirstIncompleteChild();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void ReadChild(EmberReader reader, ElementType actualType, int number)
        {
            var childRetrievalState = RetrievalState.Complete;
            Element child;
            this.children.TryGetValue(number, out child);

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                var contextSpecificOuterNumber = reader.GetContextSpecificOuterNumber();

                if (contextSpecificOuterNumber == GlowNode.Contents.OuterNumber)
                {
                    this.ReadChildContents(reader, actualType, number, ref child, out childRetrievalState);
                }
                else
                {
                    if (child == null)
                    {
                        reader.Skip();
                    }
                    else
                    {
                        if (contextSpecificOuterNumber == GlowNode.Children.OuterNumber)
                        {
                            reader.AssertInnerNumber(GlowElementCollection.InnerNumber);
                            childRetrievalState = child.ReadChildren(reader);
                        }
                        else
                        {
                            child.ReadAdditionalFields(reader);
                        }
                    }
                }
            }

            if (child != null)
            {
                child.RetrievalState = childRetrievalState;
            }

            this.RetrievalState =
                (this.children.Count == 0 ? RetrievalState.Complete : this.RetrievalState) & childRetrievalState;
        }

        private void ReadChildContents(
            EmberReader reader,
            ElementType actualType,
            int number,
            ref Element child,
            out RetrievalState childRetrievalState)
        {
            reader.AssertInnerNumber(InnerNumber.Set);

            if (child != null)
            {
                childRetrievalState = child.ReadContents(reader, actualType);
            }
            else
            {
                using (var stream = new MemoryStream())
                using (var writer = new EmberWriter(stream))
                {
                    // Since EmberReader checks that every end of a container is matched by a start, we need to write
                    // this dummy here.
                    writer.WriteStartSet(GlowNode.Contents.OuterId);
                    var identifier = reader.CopyToEndContainer(writer, GlowNodeContents.Identifier.OuterId) as string;

                    if (identifier != null)
                    {
                        writer.Flush();
                        stream.Position = 0;

                        using (var contentsReader = new EmberReader(stream))
                        {
                            contentsReader.Read(); // Read what we have written with WriteStartSet above

                            var newPolicy = this.childrenRetrievalPolicy == ChildrenRetrievalPolicy.All ?
                                ChildrenRetrievalPolicy.All : ChildrenRetrievalPolicy.None;
                            var context = new Context(this, number, identifier, newPolicy);
                            child = this.ReadNewChildContents(contentsReader, actualType, context, out childRetrievalState);

                            if (child != null)
                            {
                                this.children.Add(number, child);
                            }
                        }
                    }
                    else
                    {
                        childRetrievalState = RetrievalState.Complete;
                        child = null;
                    }
                }
            }
        }

        private RetrievalState GetRetrievalState(
            bool throwForMissingRequiredChildren, RetrievalState accumulatedChildRetrievalState)
        {
            if (accumulatedChildRetrievalState.Equals(RetrievalState.Verified))
            {
                return this.AreRequiredChildrenAvailable(throwForMissingRequiredChildren) ?
                    RetrievalState.Verified : RetrievalState.Complete;
            }
            else
            {
                return accumulatedChildRetrievalState;
            }
        }
    }
}
