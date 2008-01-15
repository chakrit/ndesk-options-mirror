// Localization with NDesk.Options.OptionSet.
//
// Compile as:
//   gmcs -r:Mono.Posix.dll -r:NDesk.Options.dll code-localization.cs
using System;
using Mono.Unix;
using NDesk.Options;

class LocalizationDemo {
	public static void Main (string[] args)
	{
		Converter<string, string> localizer = f => f;
		var p = new OptionSet () {
			{ "u|with-unix",    
				v => { localizer = f => { return Catalog.GetString (f); }; } },
			{ "h|with-hello",   
				v => { localizer = f => { return "hello:" + f; }; } },
			{ "d|with-default", v => { /* do nothing */ } },
		};
		p.Parse (args);

		bool help = false;
		int verbose = 0;
		bool version = false;
		p = new OptionSet (localizer) {
			{ "h|?|help", "show this message and exit.", 
				v => help = v != null },
			{ "v|verbose", "increase message verbosity.",
				v => { ++verbose; } },
			{ "V|version", "output version information and exit.",
				v => version = v != null },
		};
		p.Parse (args);
		if (help)
			p.WriteOptionDescriptions (Console.Out);
		if (version)
			Console.WriteLine ("NDesk.Options Localizer Demo 1.0");
		if (verbose > 0)
			Console.WriteLine ("Message level: {0}", verbose);
	}
}
