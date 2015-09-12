////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    using ComponentModel;
    using Ember;

    /// <summary>Represents an element in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
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

        /// <summary>Gets the parent, see <see cref="IElement.Parent"/>.</summary>
        /// <remarks>Is <c>null</c> for the value of <see cref="IMatrix.Parameters"/> and <see cref="MatrixLabels"/>
        /// elements in the <see cref="IMatrix.Labels"/> collection.</remarks>
        public INode Parent
        {
            get
            {
                // IParent cannot derive from INode, because some *internal* IParent implementations (e.g. ShadowNode)
                // cannot implement all INode members. This property getter is only called by client code.
                return this.parent as INode;
            }
        }

        /// <summary>Gets <b>number</b>, see <see cref="IElement.Number"/>.</summary>
        public int Number
        {
            get { return this.numberPath.Length == 0 ? 0 : this.numberPath[this.numberPath.Length - 1]; }
        }

        /// <summary>Gets <b>identifier</b>, see <see cref="IElement.Identifier"/>.</summary>
        public string Identifier
        {
            get { return this.identifier; }
        }

        /// <summary>Gets <b>description</b>, see <see cref="IElement.Description"/>.</summary>
        public string Description
        {
            get { return this.description; }
            internal set { this.SetValue(ref this.description, value); }
        }

        /// <summary>Gets a value indicating whether this element is online, see <see cref="IElement.IsOnline"/>.
        /// </summary>
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

        /// <summary>Gets or sets an arbitrary object value, see <see cref="IElement.Tag"/>.</summary>
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

        internal virtual void SetChildrenState(bool isEmpty, ref ChildrenState newChildrenState)
        {
            newChildrenState = ChildrenState.Complete;
        }

        internal abstract ChildrenState ReadContents(EmberReader reader, ElementType actualType);

        /// <summary>Recursively reads the children of an element as they appear in the message payload and returns
        /// the state of this element.</summary>
        /// <remarks>
        /// <para>Nodes for which an empty children collection is received are marked with
        /// <see cref="ChildrenState.Complete"/>. The same happens if the collection only contains children we're not
        /// interested in. In all other cases, the state of the node is lowered to the lowest state of the
        /// interesting children appearing in the payload.</para>
        /// <para>This approach ensures that any node for which incomplete interesting children have been received will
        /// be visited by <see cref="UpdateChildrenState"/>. This is necessary because some providers send messages with
        /// payloads where the same node appears multiple times. For example, the first time the state of the node may
        /// be set to <see cref="ChildrenState.None"/>, due to the fact that there are indirect children for which a
        /// getDirectory request needs to be sent. The second time the node appears only with direct and indirect
        /// children that have the state <see cref="ChildrenState.Complete"/>. Now the state of the node cannot be set
        /// to <see cref="ChildrenState.Complete"/> because then no getDirectory requests would be issued for the children
        /// that appeared the first time.</para>
        /// <para>It follows that the state of a node cannot be set to its definitive value while its children are read.
        /// Instead there needs to be a second step that visits all affected nodes and updates their state, which is
        /// implemented by <see cref="UpdateChildrenState"/>.</para></remarks>
        internal virtual ChildrenState ReadChildren(EmberReader reader)
        {
            reader.Skip();
            return ChildrenState.Complete;
        }

        internal virtual ChildrenState ReadQualifiedChild(
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
        /// <remarks>Only the children with a state not equal to <see cref="ChildrenState.Verified"/> are visited.
        /// The state of a node is set to the lowest state of its children. If a node without children has the state
        /// <see cref="ChildrenState.Complete"/> and does not require children, the state is lifted to 
        /// <see cref="ChildrenState.Verified"/>. In all other cases, the state is left as is.</remarks>
        internal virtual ChildrenState UpdateChildrenState(bool throwForMissingRequiredChildren)
        {
            return ChildrenState.Verified;
        }

        /// <summary>Writes the payload of a message that contains a getDirectory request for all nodes that require
        /// one.</summary>
        /// <remarks>Recursively visits all children with a state equal to <see cref="ChildrenState.None"/>, writes
        /// a getDirectory request for nodes that do not yet have children and marks them with
        /// <see cref="ChildrenState.GetDirectorySent"/>.</remarks>
        internal virtual void WriteChildrenQuery(EmberWriter writer)
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
