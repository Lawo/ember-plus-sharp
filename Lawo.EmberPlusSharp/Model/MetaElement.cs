////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Ember;

    /// <content>Contains the private <see cref="FieldNode{TMostDerived}.MetaElement"/> class.</content>
    public abstract partial class FieldNode<TMostDerived>
        where TMostDerived : FieldNode<TMostDerived>
    {
        private abstract class MetaElement
        {
            /// <summary>Creates and returns a <see cref="MetaElement"/> subclass object representing
            /// <paramref name="property"/>.</summary>
            internal static MetaElement Create(PropertyInfo property)
            {
                Type metaPropertyType = GetMetaPropertyType(property);
                var ctorParameters = new[] { typeof(PropertyInfo) };
                var info = metaPropertyType.GetTypeInfo().DeclaredConstructors.FirstOrDefault(
                    c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(ctorParameters));
                return (MetaElement)info.Invoke(new[] { property });
            }

            internal string Identifier => this.identifier;

            internal abstract Element ReadContents(
                EmberReader reader, ElementType actualType, Context context, out RetrievalState retrievalState);

            internal abstract bool IsAvailable(IParent parent, bool throwIfMissing);

            internal abstract void ChangeVisibility(IParent parent, IElement element);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            protected MetaElement(PropertyInfo property)
            {
                this.property = property;
                var attribute = property.GetCustomAttribute<ElementAttribute>();
                this.identifier =
                    (attribute == null) || (attribute.Identifier == null) ? property.Name : attribute.Identifier;
                this.isOptional = (attribute != null) && attribute.IsOptional;
            }

            protected PropertyInfo Property => this.property;

            protected bool IsOptional => this.isOptional;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private static Type GetMetaPropertyType(PropertyInfo property)
            {
                var implementationType = Element.GetImplementationType(property.PropertyType);

                if (implementationType != null)
                {
                    return typeof(MetaElement<>).MakeGenericType(typeof(TMostDerived), implementationType);
                }

                const string Format = "The property {0} in the type {1} has an unsupported type.";
                throw new ModelException(
                    string.Format(CultureInfo.InvariantCulture, Format, property.Name, property.DeclaringType));
            }

            private readonly PropertyInfo property;
            private readonly string identifier;
            private readonly bool isOptional;
        }
    }
}
