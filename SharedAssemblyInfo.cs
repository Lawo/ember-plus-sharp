////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Resources;

// General
[assembly: AssemblyCompany("Lawo AG")]
[assembly: AssemblyProduct("Lawo.EmberPlus")]
[assembly: AssemblyCopyright("Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.")]
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
