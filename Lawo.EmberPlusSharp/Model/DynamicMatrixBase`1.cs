////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Diagnostics.CodeAnalysis;

    using Ember;

    /// <summary>This API is not intended to be used directly from your code.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>Provides the common implementation for all matrices containing dynamic elements in the object tree
    /// accessible through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class DynamicMatrixBase<TMostDerived> : Matrix<TMostDerived>
        where TMostDerived : DynamicMatrixBase<TMostDerived>
    {
        internal DynamicMatrixBase()
        {
        }

        internal sealed override Element ReadNewDynamicChildContents(
            EmberReader reader, ElementType actualType, Context context, out RetrievalState childRetrievalState)
        {
            return DynamicNodeHelper.ReadDynamicChildContents(reader, actualType, context, out childRetrievalState);
        }
    }
}
