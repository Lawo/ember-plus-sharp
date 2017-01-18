////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;

    using Ember;

    /// <content>Contains the private <see cref="FieldNode{TMostDerived}.MetaElement{TProperty}"/> class.</content>
    public abstract partial class FieldNode<TMostDerived>
        where TMostDerived : FieldNode<TMostDerived>
    {
        private sealed class MetaElement<TProperty> : MetaElement
            where TProperty : Element<TProperty>
        {
            internal MetaElement(PropertyInfo property)
                : base(property)
            {
                var objParam = Expression.Parameter(typeof(TMostDerived));
                this.get = Expression.Lambda<Func<TMostDerived, TProperty>>(
                    Expression.Convert(Expression.Property(objParam, property), typeof(TProperty)), objParam).Compile();
                var valueParam = Expression.Parameter(typeof(TProperty));
                var assignee = Expression.Property(objParam, property);
                this.set = Expression.Lambda<Action<TMostDerived, TProperty>>(
                    Expression.Assign(assignee, valueParam), objParam, valueParam).Compile();
            }

            internal sealed override Element ReadContents(
                EmberReader reader, ElementType actualType, Context context, out RetrievalState retrievalState)
            {
                return Element<TProperty>.ReadContents(reader, actualType, context, out retrievalState);
            }

            internal sealed override bool IsAvailable(IParent parent, bool throwIfMissing)
            {
                var value = this.get((TMostDerived)parent);

                if (value == null)
                {
                    if (!this.IsOptional && throwIfMissing)
                    {
                        const string Format =
                            "No data value available for the required property {0}.{1} in the node with the path {2}.";
                        throw this.CreateRequiredPropertyException(parent, Format);
                    }

                    return this.IsOptional;
                }
                else
                {
                    return true;
                }
            }

            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
            internal sealed override void ChangeVisibility(IParent parent, IElement element)
            {
                if (!this.IsOptional && !element.IsOnline)
                {
                    const string Format =
                        "The required property {0}.{1} in the node with the path {2} has been set offline by the provider.";
                    throw this.CreateRequiredPropertyException(parent, Format);
                }

                this.set((TMostDerived)parent, (TProperty)(element.IsOnline ? element : null));
                parent.OnPropertyChanged(new PropertyChangedEventArgs(this.Property.Name));
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly Func<TMostDerived, TProperty> get;
            private readonly Action<TMostDerived, TProperty> set;

            private ModelException CreateRequiredPropertyException(IParent parent, string format) =>
                new ModelException(string.Format(
                    CultureInfo.InvariantCulture,
                    format,
                    this.Property.DeclaringType.FullName,
                    this.Property.Name,
                    parent.GetPath()));
        }
    }
}
