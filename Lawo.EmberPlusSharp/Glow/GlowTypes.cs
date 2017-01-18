////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    using Ember;

    /// <summary>Provides a singleton <see cref="EmberTypeBag"/> instance containing all Glow types.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class GlowTypes
    {
        /// <summary>Gets the singleton <see cref="EmberTypeBag"/> instance containing all Glow types.</summary>
        public static EmberTypeBag Instance { get; } =
            new EmberTypeBag(
                typeof(GlowGlobal),
                typeof(GlowParameter),
                new EmberType(typeof(GlowParameter.Contents), typeof(GlowParameterContents)),
                typeof(GlowCommand),
                typeof(GlowNode),
                new EmberType(typeof(GlowNode.Contents), typeof(GlowNodeContents)),
                typeof(GlowElementCollection),
                typeof(GlowStreamEntry),
                typeof(GlowStreamCollection),
                typeof(GlowStringIntegerPair),
                typeof(GlowStringIntegerCollection),
                typeof(GlowQualifiedParameter),
                new EmberType(typeof(GlowQualifiedParameter.Contents), typeof(GlowParameterContents)),
                typeof(GlowQualifiedNode),
                new EmberType(typeof(GlowQualifiedNode.Contents), typeof(GlowNodeContents)),
                typeof(GlowRootElementCollection),
                typeof(GlowStreamDescription),
                typeof(GlowMatrix),
                new EmberType(typeof(GlowMatrix.Contents), typeof(GlowMatrixContents)),
                new EmberType(typeof(GlowMatrix.Contents), typeof(GlowMatrixContents.Labels), typeof(GlowLabelCollection)),
                new EmberType(typeof(GlowMatrix.Targets), typeof(GlowTargetCollection)),
                new EmberType(typeof(GlowMatrix.Sources), typeof(GlowSourceCollection)),
                new EmberType(typeof(GlowMatrix.Connections), typeof(GlowConnectionCollection)),
                typeof(GlowTarget),
                typeof(GlowSource),
                typeof(GlowConnection),
                typeof(GlowQualifiedMatrix),
                new EmberType(typeof(GlowQualifiedMatrix.Contents), typeof(GlowMatrixContents)),
                new EmberType(typeof(GlowQualifiedMatrix.Contents), typeof(GlowMatrixContents.Labels), typeof(GlowLabelCollection)),
                new EmberType(typeof(GlowQualifiedMatrix.Targets), typeof(GlowTargetCollection)),
                new EmberType(typeof(GlowQualifiedMatrix.Sources), typeof(GlowSourceCollection)),
                new EmberType(typeof(GlowQualifiedMatrix.Connections), typeof(GlowConnectionCollection)),
                typeof(GlowLabel),
                typeof(GlowFunction),
                new EmberType(typeof(GlowFunction.Contents), typeof(GlowFunctionContents)),
                new EmberType(typeof(GlowFunction.Contents), typeof(GlowFunctionContents.Arguments), typeof(GlowTupleDescription)),
                new EmberType(typeof(GlowFunction.Contents), typeof(GlowFunctionContents.Result), typeof(GlowTupleDescription)),
                typeof(GlowQualifiedFunction),
                new EmberType(typeof(GlowQualifiedFunction.Contents), typeof(GlowFunctionContents)),
                new EmberType(typeof(GlowQualifiedFunction.Contents), typeof(GlowFunctionContents.Arguments), typeof(GlowTupleDescription)),
                new EmberType(typeof(GlowQualifiedFunction.Contents), typeof(GlowFunctionContents.Result), typeof(GlowTupleDescription)),
                typeof(GlowTupleItemDescription),
                typeof(GlowInvocation),
                new EmberType(typeof(GlowInvocation.Arguments), typeof(GlowTuple)),
                typeof(GlowInvocationResult),
                new EmberType(typeof(GlowInvocationResult.Result), typeof(GlowTuple)));
    }
}
