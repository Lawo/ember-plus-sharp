////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowLogConverter.Main
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            if ((args.Length != 1) || !File.Exists(args[0]))
            {
                Console.WriteLine(
                    "Invalid number of arguments or invalid argument!" + Environment.NewLine + Environment.NewLine +
                    Assembly.GetExecutingAssembly().GetName().Name + " source" +
                    Environment.NewLine + Environment.NewLine +
                    "  source    Specifies the source S101Log XML file.");
                return;
            }

            var settings = new XmlWriterSettings() { Indent = true, CloseOutput = true };

            using (var reader = XmlReader.Create(args[0], null, null))
            using (var writer = XmlWriter.Create(Path.ChangeExtension(args[0], "converted.xml"), settings))
            {
                EmberPlusSharp.Glow.GlowLogConverter.Convert(reader, writer);
            }
        }
    }
}
