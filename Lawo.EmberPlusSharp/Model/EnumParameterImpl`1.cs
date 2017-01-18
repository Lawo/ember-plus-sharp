////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Ember;

    internal sealed class EnumParameterImpl<TEnum>
        where TEnum : struct
    {
        internal EnumParameterImpl(IParameter parent)
        {
            this.parent = parent;
        }

        internal IReadOnlyList<KeyValuePair<string, int>> EnumMapCore
        {
            get
            {
                if (this.enumMap == null)
                {
                    this.enumMap = FastEnum.GetNameValueMap<TEnum>().Select(
                        p => new KeyValuePair<string, int>(p.Key, (int)FastEnum.ToInt64(p.Value))).ToList();
                }

                return this.enumMap;
            }

            set
            {
                this.providerHasSentEnum = true;
                var consumerMap = FastEnum.GetValueNameMap<TEnum>();

                if (value.Count != consumerMap.Count)
                {
                    const string Format =
                        "The number of named constants of the enum specified for the parameter with the path {0} " +
                        "does not match the number of entries sent by the provider.";
                    throw new ModelException(
                        string.Format(CultureInfo.InvariantCulture, Format, this.parent.GetPath()));
                }

                foreach (var providerEntry in value)
                {
                    string consumerName;

                    if (!consumerMap.TryGetValue(FastEnum.ToEnum<TEnum>(providerEntry.Value), out consumerName))
                    {
                        const string Format = "The enum specified for the parameter with the path {0} " +
                            "does not have a named constant for the value {1}.";
                        throw new ModelException(string.Format(
                            CultureInfo.InvariantCulture, Format, this.parent.GetPath(), providerEntry.Value));
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Official Glow name.")]
        internal RetrievalState ReadContents(
            Func<EmberReader, ElementType, RetrievalState> baseImpl, EmberReader reader, ElementType actualType)
        {
            if (!IsEnum)
            {
                const string Format = "The type argument passed to the enum parameter with the path {0} is not an enum.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, this.parent.GetPath()));
            }

            var result = baseImpl(reader, actualType);

            if (!this.providerHasSentEnum)
            {
                const string Format = "No enumeration or enumMap field is available for the enum parameter with the path {0}.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, this.parent.GetPath()));
            }

            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly bool IsEnum = typeof(TEnum).GetTypeInfo().IsEnum;
        private readonly IParameter parent;
        private bool providerHasSentEnum;
        private IReadOnlyList<KeyValuePair<string, int>> enumMap;
    }
}
