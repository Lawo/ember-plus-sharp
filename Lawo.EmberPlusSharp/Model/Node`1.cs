////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Ember;
    using Glow;

    /// <summary>This class is not intended to be referenced in your code.</summary>
    /// <remarks>Provides common implementation for all nodes in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class Node<TMostDerived> : NodeBase<TMostDerived>, INode
        where TMostDerived : Node<TMostDerived>
    {
        /// <inheritdoc/>
        public bool IsRoot
        {
            get { return this.GetIsRoot(); }
            private set { this.SetValue(ref this.isRoot, value); }
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Required property is provided by subclasses.")]
        ReadOnlyObservableCollection<IElement> INode.Children => this.readOnlyObservableChildren;

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Required property is provided by subclasses.")]
        IElement INode.this[int number] => this.GetChild(number);

        /// <inheritdoc/>
        public IElement GetElement(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return this.GetElement(path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries), 0);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Node()
        {
            this.readOnlyObservableChildren = new ReadOnlyObservableCollection<IElement>(this.observableChildren);
        }

        internal virtual bool GetIsRoot() => this.isRoot;

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal override bool ChangeVisibility(IElement child)
        {
            VisibilityHelper.ChangeVisibility(this.observableChildren, child);
            return base.ChangeVisibility(child);
        }

        internal override RetrievalState ReadContents(EmberReader reader, ElementType actualType)
        {
            this.AssertElementType(ElementType.Node, actualType);

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowNodeContents.Description.OuterNumber:
                        this.Description = reader.AssertAndReadContentsAsString();
                        break;
                    case GlowNodeContents.IsRoot.OuterNumber:
                        this.IsRoot = reader.AssertAndReadContentsAsBoolean();
                        break;
                    case GlowNodeContents.IsOnline.OuterNumber:
                        this.IsOnline = reader.AssertAndReadContentsAsBoolean();
                        this.RetrievalState &= RetrievalState.Complete;
                        break;
                    case GlowNodeContents.SchemaIdentifiers.OuterNumber:
                        this.ReadSchemaIdentifiers(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return this.RetrievalState;
        }

        internal override void WriteChanges(EmberWriter writer, IInvocationCollection pendingInvocations)
        {
            if (this.HasChanges)
            {
                this.WriteChangesCollection(writer, pendingInvocations);
                this.HasChanges = false;
            }
        }

        internal sealed override IElement GetElement(string[] pathElements, int index)
        {
            var candidate = base.GetElement(pathElements, index);

            if (candidate != null)
            {
                return candidate;
            }

            var child = (Element)this.observableChildren.FirstOrDefault(c => c.Identifier == pathElements[index]);
            return child?.GetElement(pathElements, index + 1);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly ObservableCollection<IElement> observableChildren = new ObservableCollection<IElement>();
        private readonly ReadOnlyObservableCollection<IElement> readOnlyObservableChildren;
        private bool isRoot;
    }
}
