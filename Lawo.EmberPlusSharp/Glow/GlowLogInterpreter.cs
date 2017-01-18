////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Xml;

    using Ember;
    using Model;
    using S101;

    /// <summary>Reads the events written by a <see cref="S101Logger"/> instantiated with
    /// <see cref="GlowTypes.Instance"/> and applies them to the tree rooted in <see cref="Root"/>.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class GlowLogInterpreter
    {
        /// <summary>Initializes a new instance of the <see cref="GlowLogInterpreter"/> class.</summary>
        /// <param name="types">The types to pass to the internal <see cref="S101LogReader"/>.</param>
        /// <param name="logReader">The <see cref="XmlReader"/> to read the messages from. The format needs to match the
        /// one written by <see cref="S101Logger"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="types"/> and/or <paramref name="logReader"/> equal
        /// <c>null</c>.</exception>
        public GlowLogInterpreter(EmberTypeBag types, XmlReader logReader)
        {
            this.reader = new S101LogReader(types, logReader);
        }

        /// <summary>Gets the UTC time of the current message.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public DateTime TimeUtc => this.reader.TimeUtc;

        /// <summary>Gets the direction of the current message.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public string Direction => this.reader.Direction;

        /// <summary>Gets the number of the current message.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public int Number => this.reader.Number;

        /// <summary>Gets the current message.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public S101Message Message => this.reader.Message;

        /// <summary>Gets the root node.</summary>
        public INode Root => this.root;

        /// <summary>Reads the next message.</summary>
        /// <returns><c>true</c> if the next message was read successfully; <c>false</c> if there are no more messages
        /// to read.</returns>
        /// <exception cref="XmlException">An error occurred while parsing the XML-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <remarks>
        /// <para>When a <see cref="GlowLogInterpreter"/> is first created and initialized, there is no information
        /// available. You must call <see cref="Read"/> to read the first message.</para></remarks>
        public bool Read() => this.reader.Read();

        /// <summary>Applies the payload of the current message to the tree rooted in <see cref="Root"/>.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        /// <exception cref="XmlException">An error occurred while parsing the XML-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        public void ApplyPayload()
        {
            var payload = this.reader.GetPayload();

            if (payload.Length > 0)
            {
                using (var stream = new MemoryStream(payload))
                using (var emberReader = new EmberReader(stream))
                using (var dummyWriter = new EmberWriter(Stream.Null))
                {
                    this.root.Read(emberReader, this.pendingInvocations, this.streamedParameters);
                    this.root.WriteRequest(dummyWriter, this.streamedParameters);
                    this.root.SetComplete();
                    this.root.UpdateRetrievalState(true);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly MyDynamicRoot root = Root<MyDynamicRoot>.Construct(new Context(null, 0, string.Empty));
        private readonly InvocationCollection pendingInvocations = new InvocationCollection();
        private readonly StreamedParameterCollection streamedParameters = new StreamedParameterCollection();
        private readonly S101LogReader reader;

        [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
        private sealed class MyDynamicRoot : DynamicRoot<MyDynamicRoot>
        {
        }
    }
}
