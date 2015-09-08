////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    using Lawo.EmberPlus.Ember;
    using Lawo.EmberPlus.Glow;

    /// <summary>Provides the common functionality for all nodes in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public abstract class NodeBase<TMostDerived> : ElementWithSchemas<TMostDerived>, IParent
        where TMostDerived : NodeBase<TMostDerived>
    {
        private readonly SortedDictionary<int, Element> children = new SortedDictionary<int, Element>();

        /// <summary>See <see cref="ChildrenState"/> for more information.</summary>
        /// <remarks>This field and its sibling <see cref="offlineChildrenState"/> are modified by the following
        /// methods, which are directly or indirectly called from
        /// <see cref="Consumer{T}.CreateAsync(Lawo.EmberPlus.S101.S101Client)"/>:
        /// <list type="number">
        /// <item><see cref="Element.UpdateChildrenState"/></item>
        /// <item><see cref="Element.WriteChildrenQuery"/></item>
        /// <item><see cref="Element.ReadChildren"/></item>
        /// <item><see cref="Element.AreRequiredChildrenAvailable"/></item>
        /// </list>
        /// See individual method documentation for semantics. This rather complex system was implemented to make the
        /// process of querying the provider as efficient as possible, namely:
        /// <list type="bullet">
        /// <item>As few as possible messages are sent to query for children.</item>
        /// <item>The computational effort for tree traversal is kept as low as possible. This is necessary because all
        /// code is always executed on the applications GUI thread. Without these optimizations, a full tree traversal
        /// would be necessary after each processed message. Some providers send a new message for each updated
        /// parameter, which would very quickly lead to significant CPU load and an unresponsive GUI if many parameters
        /// are changed at once in a large tree.</item>
        /// </list>
        /// </remarks>
        private ChildrenState onlineChildrenState;
        private ChildrenState offlineChildrenState = ChildrenState.Complete;

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

        internal NodeBase()
        {
        }

        internal ChildrenState ChildrenState
        {
            get
            {
                return this.IsOnline ? this.onlineChildrenState : this.offlineChildrenState;
            }

            private set
            {
                if (this.IsOnline)
                {
                    this.onlineChildrenState = value;
                }
                else
                {
                    this.offlineChildrenState = value;
                }
            }
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

        internal void WriteChildrenQueryCollection(EmberWriter writer)
        {
            if (this.children.Count == 0)
            {
                writer.WriteStartApplicationDefinedType(GlowElementCollection.Element.OuterId, GlowCommand.InnerNumber);
                writer.WriteValue(GlowCommand.Number.OuterId, 32);
                writer.WriteEndContainer();
                this.ChildrenState = ChildrenState.GetDirectorySent;
            }
            else
            {
                foreach (var child in this.children.Values)
                {
                    child.WriteChildrenQuery(writer);
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
            EmberReader reader, ElementType actualType, Context context, out ChildrenState childChildrenState)
        {
            reader.SkipToEndContainer();
            childChildrenState = ChildrenState.Complete;
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

        internal sealed override void SetChildrenState(bool isEmpty, ref ChildrenState newChildrenState)
        {
            if (isEmpty)
            {
                base.SetChildrenState(isEmpty, ref newChildrenState);
            }

            this.ChildrenState = newChildrenState;
        }

        internal sealed override ChildrenState ReadChildren(EmberReader reader)
        {
            if (this.ReadChildrenCore(reader))
            {
                this.ChildrenState = ChildrenState.Complete;
            }

            return this.ChildrenState;
        }

        internal sealed override ChildrenState ReadQualifiedChild(
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
                    this.ChildrenState &= child.ReadQualifiedChild(reader, actualType, path, index + 1);
                }
            }

            return this.ChildrenState;
        }

        internal sealed override ChildrenState UpdateChildrenState(bool throwForMissingRequiredChildren)
        {
            if (!this.IsOnline)
            {
                this.ChildrenState = ChildrenState.Verified;
            }
            else if (this.children.Count == 0)
            {
                if (this.ChildrenState.Equals(ChildrenState.Complete) &&
                    this.AreRequiredChildrenAvailable(throwForMissingRequiredChildren))
                {
                    this.ChildrenState = ChildrenState.Verified;
                }
            }
            else
            {
                if (!this.ChildrenState.Equals(ChildrenState.Verified))
                {
                    var accumulatedChildChildrenState = ChildrenState.Verified;

                    foreach (var child in this.children.Values)
                    {
                        var childChildrenState = child.UpdateChildrenState(throwForMissingRequiredChildren);
                        accumulatedChildChildrenState &= childChildrenState;

                        if (child.IsOnlineChangeStatus != IsOnlineChangeStatus.Unchanged)
                        {
                            if ((child.IsOnline && childChildrenState.Equals(ChildrenState.Verified)) || !child.IsOnline)
                            {
                                child.IsOnlineChangeStatus = IsOnlineChangeStatus.Unchanged;
                                this.ChangeOnlineStatus(child);
                            }
                        }
                    }

                    this.ChildrenState =
                        this.GetChildrenState(throwForMissingRequiredChildren, accumulatedChildChildrenState);
                }
            }

            return this.ChildrenState;
        }

        internal override void WriteChildrenQuery(EmberWriter writer)
        {
            if (this.ChildrenState.Equals(ChildrenState.None))
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

                this.WriteChildrenQueryCollection(writer);

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

            this.ChildrenState = ChildrenState.Complete;
        }

        internal sealed override IParent GetFirstIncompleteChild()
        {
            if (this.ChildrenState.Equals(ChildrenState.GetDirectorySent))
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
            var childChildrenState = ChildrenState.Complete;
            Element child;
            this.children.TryGetValue(number, out child);
            var isEmpty = true;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                isEmpty = false;
                var contextSpecificOuterNumber = reader.GetContextSpecificOuterNumber();

                if (contextSpecificOuterNumber == GlowNode.Contents.OuterNumber)
                {
                    this.ReadChildContents(reader, actualType, number, ref child, out childChildrenState);
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
                            childChildrenState = child.ReadChildren(reader);
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
                child.SetChildrenState(isEmpty, ref childChildrenState);
            }

            this.ChildrenState =
                (this.children.Count == 0 ? ChildrenState.Complete : this.ChildrenState) & childChildrenState;
        }

        private void ReadChildContents(
            EmberReader reader,
            ElementType actualType,
            int number,
            ref Element child,
            out ChildrenState childChildrenState)
        {
            reader.AssertInnerNumber(InnerNumber.Set);

            if (child != null)
            {
                childChildrenState = child.ReadContents(reader, actualType);
            }
            else
            {
                reader.AssertRead();

                if (reader.CanReadContents && (reader.OuterId == GlowNodeContents.Identifier.OuterId))
                {
                    var context = new Context(this, number, reader.AssertAndReadContentsAsString());
                    child = this.ReadNewChildContents(reader, actualType, context, out childChildrenState);

                    if (child != null)
                    {
                        this.children.Add(number, child);
                    }
                }
                else
                {
                    reader.SkipToEndContainer();
                    childChildrenState = ChildrenState.Complete;
                    child = null;
                }
            }
        }

        private ChildrenState GetChildrenState(
            bool throwForMissingRequiredChildren, ChildrenState accumulatedChildChildrenState)
        {
            if (accumulatedChildChildrenState.Equals(ChildrenState.Verified))
            {
                return this.AreRequiredChildrenAvailable(throwForMissingRequiredChildren) ?
                    ChildrenState.Verified : ChildrenState.Complete;
            }
            else
            {
                return accumulatedChildChildrenState;
            }
        }
    }
}
