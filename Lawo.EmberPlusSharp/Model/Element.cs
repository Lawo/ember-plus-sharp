////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    using ComponentModel;
    using Ember;

    /// <summary>This class is not intended to be referenced in your code.</summary>
    /// <remarks>Provides common implementation for all elements in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <threadsafety static="true" instance="false"/>
    public abstract class Element : NotifyPropertyChanged, IElement
    {
        private IParent parent;
        private int[] numberPath;
        private string identifier;
        private string description;
        private bool isOnline = true;
        private IsOnlineChangeStatus isOnlineChangedStatus;
        private bool hasChanges;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <inheritdoc/>
        public INode Parent
        {
            get
            {
                // IParent cannot derive from INode, because some *internal* IParent implementations (e.g. ShadowNode)
                // cannot implement all INode members. This property getter is only called by client code.
                return this.parent as INode;
            }
        }

        /// <inheritdoc/>
        public int Number
        {
            get { return this.numberPath.Length == 0 ? 0 : this.numberPath[this.numberPath.Length - 1]; }
        }

        /// <inheritdoc/>
        public string Identifier
        {
            get { return this.identifier; }
        }

        /// <inheritdoc/>
        public string Description
        {
            get { return this.description; }
            internal set { this.SetValue(ref this.description, value); }
        }

        /// <inheritdoc/>
        public bool IsOnline
        {
            get
            {
                return this.isOnline;
            }

            internal set
            {
                if (this.SetValue(ref this.isOnline, value))
                {
                    // We're deliberately not simply setting this to Changed here, because we want to correctly handle
                    // the case when IsOnline is changed twice without being observed between the changes.
                    if (this.IsOnlineChangeStatus == IsOnlineChangeStatus.Unchanged)
                    {
                        this.isOnlineChangedStatus = IsOnlineChangeStatus.Changed;
                    }
                    else if (this.isOnlineChangedStatus == IsOnlineChangeStatus.Changed)
                    {
                        this.isOnlineChangedStatus = IsOnlineChangeStatus.Unchanged;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public object Tag { get; set; }

        /// <inheritdoc/>
        public string GetPath()
        {
            var path = new StringBuilder(64);
            this.AppendPath(path);

            if (path.Length > 1)
            {
                path.Remove(path.Length - 1, 1);
            }

            return path.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Element()
        {
        }

        internal int[] NumberPath
        {
            get { return this.numberPath; }
        }

        internal IsOnlineChangeStatus IsOnlineChangeStatus
        {
            get { return this.isOnlineChangedStatus; }
            set { this.isOnlineChangedStatus = value; }
        }

        internal bool HasChanges
        {
            get
            {
                return this.hasChanges;
            }

            set
            {
                if (value != this.hasChanges)
                {
                    this.hasChanges = value;

                    if (value && (this.parent != null))
                    {
                        this.parent.SetHasChanges();
                    }
                }
            }
        }

        internal void SetContext(Context context)
        {
            this.parent = context.Parent;

            if (this.parent == null)
            {
                this.numberPath = new int[0];
            }
            else
            {
                var parentNumberPath = this.parent.NumberPath;
                this.numberPath = new int[parentNumberPath.Length + 1];
                Array.Copy(parentNumberPath, this.numberPath, parentNumberPath.Length);
                this.numberPath[parentNumberPath.Length] = context.Number;
            }

            this.identifier = context.Identifier;
        }

        internal void AppendPath(StringBuilder builder)
        {
            if (this.parent != null)
            {
                this.parent.AppendPath(builder);
            }

            builder.Append(this.identifier);
            builder.Append('/');
        }

        internal void AssertElementType(ElementType expectedType, ElementType actualType)
        {
            if (expectedType != actualType)
            {
                const string Format = "Found a {0} data value while expecting a {1} for the element with the path {2}.";
                throw new ModelException(
                    string.Format(CultureInfo.InvariantCulture, Format, actualType, expectedType, this.GetPath()));
            }
        }

        internal void SetConsumerValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!GenericCompare.Equals(field, newValue))
            {
                field = newValue;
                this.HasChanges = true;
                this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }

        internal virtual void SetRequestState(bool isEmpty, ref RequestState newRequestState)
        {
            newRequestState = RequestState.Complete;
        }

        internal abstract RequestState ReadContents(EmberReader reader, ElementType actualType);

        /// <summary>Recursively reads the children of an element as they appear in the message payload and returns
        /// the state of this element.</summary>
        /// <remarks>
        /// <para>Nodes for which an empty children collection is received are marked with
        /// <see cref="RequestState.Complete"/>. The same happens if the collection only contains children we're not
        /// interested in. In all other cases, the state of the node is lowered to the lowest state of the
        /// interesting children appearing in the payload.</para>
        /// <para>This approach ensures that any node for which incomplete interesting children have been received will
        /// be visited by <see cref="UpdateRequestState"/>. This is necessary because some providers send messages with
        /// payloads where the same node appears multiple times. For example, the first time the state of the node may
        /// be set to <see cref="RequestState.None"/>, due to the fact that there are indirect children for which a
        /// <code>getDirectory</code> request needs to be sent. The second time the node appears only with direct and
        /// indirect children that have the state <see cref="RequestState.Complete"/>. Now the state of the node cannot
        /// be set to <see cref="RequestState.Complete"/> because then no <code>getDirectory</code> requests would be
        /// issued for the children that appeared the first time.</para>
        /// <para>It follows that the state of a node cannot be set to its definitive value while its children are read.
        /// Instead there needs to be a second step that visits all affected nodes and updates their state, which is
        /// implemented by <see cref="UpdateRequestState"/>.</para></remarks>
        internal virtual RequestState ReadChildren(EmberReader reader)
        {
            reader.Skip();
            return RequestState.Complete;
        }

        internal virtual RequestState ReadQualifiedChild(
            EmberReader reader, ElementType actualType, int[] path, int index)
        {
            const string Format =
                "The path of a qualified element attempts to address a direct or indirect child of the element with the path {0}.";
            throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, this.GetPath()));
        }

        internal virtual void ReadAdditionalFields(EmberReader reader)
        {
            reader.Skip();
        }

        /// <summary>Recursively updates the state of all children and returns the state of this element.</summary>
        /// <remarks>Only the children with a state not equal to <see cref="RequestState.Verified"/> are visited.
        /// The state of a node is set to the lowest state of its children. If a node without children has the state
        /// <see cref="RequestState.Complete"/> and does not require children, the state is lifted to 
        /// <see cref="RequestState.Verified"/>. In all other cases, the state is left as is.</remarks>
        internal virtual RequestState UpdateRequestState(bool throwForMissingRequiredChildren)
        {
            return RequestState.Verified;
        }

        /// <summary>Writes the payload of a message that contains an appropriate requests for all elements that require
        /// one.</summary>
        /// <remarks>Recursively visits all children with a state equal to <see cref="RequestState.None"/>, writes
        /// a getDirectory request for nodes that do not yet have children or a subscribe request for stream parameters
        /// and changes their state accordingly.</remarks>
        internal virtual void WriteRequest(EmberWriter writer)
        {
        }

        internal abstract void WriteChanges(EmberWriter writer, IInvocationCollection invocationCollection);

        internal virtual void SetComplete()
        {
        }

        /// <summary>Returns a value indicating whether all required children are available for this element.</summary>
        internal virtual bool AreRequiredChildrenAvailable(bool throwIfMissing)
        {
            return true;
        }

        internal virtual IParent GetFirstIncompleteChild()
        {
            return null;
        }

        internal virtual IElement GetElement(string[] pathElements, int index)
        {
            return index == pathElements.Length ? this : null;
        }

        internal static Type GetImplementationType(Type type)
        {
            if (type == typeof(IParameter))
            {
                return typeof(DynamicParameter);
            }
            else if (type == typeof(INode))
            {
                return typeof(DynamicNode);
            }
            else if (type == typeof(IFunction))
            {
                return typeof(DynamicFunction);
            }
            else if (IsElement(type))
            {
                return type;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static bool IsElement(Type type)
        {
            try
            {
                return type.GetTypeInfo().IsSubclassOf(typeof(Element<>).MakeGenericType(type));
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
