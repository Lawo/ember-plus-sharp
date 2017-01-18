////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Resources;

// General
[assembly: AssemblyCompany("Lawo AG")]
[assembly: AssemblyProduct("Lawo.EmberPlusSharp")]
[assembly: AssemblyCopyright("Copyright 2012-2017 Lawo AG (http://www.lawo.com). Distributed under the Boost Software License, Version 1.0. (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)")]
[assembly: AssemblyTrademark("")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// i18n
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en-US")]

// Versioning
[assembly: AssemblyVersion("0.0.0.1")]
[assembly: AssemblyFileVersion("0.0.0.1")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
