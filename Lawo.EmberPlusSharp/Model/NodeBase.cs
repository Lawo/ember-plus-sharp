////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;
    using System.ComponentModel;
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

        internal NodeBase() : base(RequestState.None)
        {
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

        internal void WriteCommandCollection(EmberWriter writer)
        {
            if (this.children.Count == 0)
            {
                this.WriteCommandCollection(writer, GlowCommandNumber.GetDirectory, RequestState.RequestSent);
            }
            else
            {
                foreach (var child in this.children.Values)
                {
                    child.WriteRequest(writer);
                }
            }
        }

        internal void WriteChangesCollection(EmberWriter writer, IInvocationCollection invocationCollection)
        {
            foreach (var child in this.children.Values)
            {
                child.WriteChanges(writer, invocationCollection);
            }
        }

        internal virtual Element ReadNewChildContents(
            EmberReader reader, ElementType actualType, Context context, out RequestState childRequestState)
        {
            reader.SkipToEndContainer();
            childRequestState = RequestState.Complete;
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

        /// <summary>Changes the online status of <paramref name="child"/>.</summary>
        /// <param name="child">The child to change the online status for.</param>
        /// <returns><c>true</c> if the online status has been changed; otherwise, <c>false</c>.</returns>
        internal virtual bool ChangeOnlineStatus(IElement child)
        {
            return false;
        }

        internal sealed override RequestState ReadChildren(EmberReader reader)
        {
            if (this.ReadChildrenCore(reader))
            {
                this.RequestState = RequestState.Complete;
            }

            return this.RequestState;
        }

        internal sealed override RequestState ReadQualifiedChild(
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
                    this.RequestState &= child.ReadQualifiedChild(reader, actualType, path, index + 1);
                }
            }

            return this.RequestState;
        }

        internal sealed override RequestState UpdateRequestState(bool throwForMissingRequiredChildren)
        {
            if (!this.IsOnline)
            {
                this.RequestState = RequestState.Verified;
            }
            else if (this.children.Count == 0)
            {
                if (this.RequestState.Equals(RequestState.Complete) &&
                    this.AreRequiredChildrenAvailable(throwForMissingRequiredChildren))
                {
                    this.RequestState = RequestState.Verified;
                }
            }
            else
            {
                if (!this.RequestState.Equals(RequestState.Verified))
                {
                    var accumulatedChildRequestState = RequestState.Verified;

                    foreach (var child in this.children.Values)
                    {
                        var childRequestState = child.UpdateRequestState(throwForMissingRequiredChildren);
                        accumulatedChildRequestState &= childRequestState;

                        if (child.IsOnlineChangeStatus != IsOnlineChangeStatus.Unchanged)
                        {
                            if ((child.IsOnline && childRequestState.Equals(RequestState.Verified)) || !child.IsOnline)
                            {
                                child.IsOnlineChangeStatus = IsOnlineChangeStatus.Unchanged;
                                this.ChangeOnlineStatus(child);
                            }
                        }
                    }

                    this.RequestState =
                        this.GetRequestState(throwForMissingRequiredChildren, accumulatedChildRequestState);
                }
            }

            return this.RequestState;
        }

        internal override void WriteRequest(EmberWriter writer)
        {
            if (this.RequestState.Equals(RequestState.None))
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

                this.WriteCommandCollection(writer);

                if (isEmpty)
                {
                    writer.WriteEndContainer();
                    writer.WriteEndContainer();
                }
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
            if (this.RequestState.Equals(RequestState.RequestSent))
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
            var childRequestState = RequestState.Complete;
            Element child;
            this.children.TryGetValue(number, out child);
            var isEmpty = true;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                isEmpty = false;
                var contextSpecificOuterNumber = reader.GetContextSpecificOuterNumber();

                if (contextSpecificOuterNumber == GlowNode.Contents.OuterNumber)
                {
                    this.ReadChildContents(reader, actualType, number, ref child, out childRequestState);
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
                            childRequestState = child.ReadChildren(reader);
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
                child.SetRequestState(isEmpty, ref childRequestState);
            }

            this.RequestState =
                (this.children.Count == 0 ? RequestState.Complete : this.RequestState) & childRequestState;
        }

        private void ReadChildContents(
            EmberReader reader,
            ElementType actualType,
            int number,
            ref Element child,
            out RequestState childRequestState)
        {
            reader.AssertInnerNumber(InnerNumber.Set);

            if (child != null)
            {
                childRequestState = child.ReadContents(reader, actualType);
            }
            else
            {
                reader.AssertRead();

                if (reader.CanReadContents && (reader.OuterId == GlowNodeContents.Identifier.OuterId))
                {
                    var context = new Context(this, number, reader.AssertAndReadContentsAsString());
                    child = this.ReadNewChildContents(reader, actualType, context, out childRequestState);

                    if (child != null)
                    {
                        this.children.Add(number, child);
                    }
                }
                else
                {
                    reader.SkipToEndContainer();
                    childRequestState = RequestState.Complete;
                    child = null;
                }
            }
        }

        private RequestState GetRequestState(
            bool throwForMissingRequiredChildren, RequestState accumulatedChildRequestState)
        {
            if (accumulatedChildRequestState.Equals(RequestState.Verified))
            {
                return this.AreRequiredChildrenAvailable(throwForMissingRequiredChildren) ?
                    RequestState.Verified : RequestState.Complete;
            }
            else
            {
                return accumulatedChildRequestState;
            }
        }
    }
}
