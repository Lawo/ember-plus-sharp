////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Ember;

    /// <summary>Represents an element in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class Element<TMostDerived> : Element where TMostDerived : Element<TMostDerived>
    {
        private static readonly Func<TMostDerived> Constructor = GetConstructor();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Element()
        {
        }

        internal int ReadInt(EmberReader reader, string fieldName)
        {
            try
            {
                return reader.AssertAndReadContentsAsInt32();
            }
            catch (ModelException ex)
            {
                const string Format =
                    "The value of the field {0} is out of the allowed range for the element with the path {1}.";
                throw new ModelException(
                    string.Format(CultureInfo.InvariantCulture, Format, fieldName, this.GetPath()), ex);
            }
        }

        internal T ReadEnum<T>(EmberReader reader, string fieldName) where T : struct
        {
            Exception exception = null;

            try
            {
                var result = FastEnum.ToEnum<T>(reader.AssertAndReadContentsAsInt32());

                if (FastEnum.IsDefined(result))
                {
                    return result;
                }
            }
            catch (ModelException ex)
            {
                exception = ex;
            }

            const string Format = "The field {0} has an unexpected value for the element with the path {1}.";
            throw new ModelException(
                string.Format(CultureInfo.InvariantCulture, Format, fieldName, this.GetPath()), exception);
        }

        internal static TMostDerived Construct(Context context)
        {
            if (Constructor == null)
            {
                const string Format = "The type {0} does not have a default constructor.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, typeof(TMostDerived)));
            }

            var result = Constructor();
            result.SetContext(context);
            return result;
        }

        internal static TMostDerived ReadContents(
            EmberReader reader, ElementType actualType, Context context, out ChildrenState childrenState)
        {
            var result = Construct(context);
            childrenState = result.ReadContents(reader, actualType);
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Func<TMostDerived> GetConstructor()
        {
            var info = typeof(TMostDerived).GetTypeInfo().DeclaredConstructors.FirstOrDefault(
                c => c.GetParameters().Length == 0);
            return info == null ? null : Expression.Lambda<Func<TMostDerived>>(Expression.New(info)).Compile();
        }
    }
}
