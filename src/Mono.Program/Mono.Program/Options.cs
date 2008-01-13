//
// Options.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

// Compile With:
//   gmcs -debug+ -d:TEST -langversion:linq -r:System.Core Options.cs

//
// A Getopt::Long-inspired option parsing library for C#.
//
// Mono.Documentation.Options is built upon a key/value table, where the
// key is a option format string and the value is an Action<string>
// delegate that is invoked when the format string is matched.
//
// Option format strings:
//  BNF Grammar: ( name [=:]? ) ( '|' name [=:]? )+
// 
// Each '|'-delimited name is an alias for the associated action.  If the
// format string ends in a '=', it has a required value.  If the format
// string ends in a ':', it has an optional value.  If neither '=' or ':'
// is present, no value is supported.
//
// Options are extracted either from the current option by looking for
// the option name followed by an '=' or ':', or is taken from the
// following option IFF:
//  - The current option does not contain a '=' or a ':'
//  - The following option is not a registered named option
//
// The `name' used in the option format string does NOT include any leading
// option indicator, such as '-', '--', or '/'.  All three of these are
// permitted/required on any named option.
//
// Option bundling is permitted so long as:
//   - '-' is used to start the option group
//   - all of the bundled options do not require values
//   - all of the bundled options are a single character
//
// This allows specifying '-a -b -c' as '-abc'.
//
// Option processing is disabled by specifying "--".  All options after "--"
// are returned by Options.Parse() unchanged and unprocessed.
//
// Unprocessed options are returned from Options.Parse().
//
// Examples:
//  int verbose = 0;
//  Options p = new Options ()
//    .Add ("v", v => ++verbose)
//    .Add ("name=|value=", v => Console.WriteLine (v));
//  p.Parse (new string[]{"-v", "--v", "/v", "-name=A", "/name", "B", "extra"});
//
// The above would parse the argument string array, and would invoke the
// lambda expression three times, setting `verbose' to 3 when complete.  
// It would also print out "A" and "B" to standard output.
// The returned arrray would contain the string "extra".
//
// C# 3.0 collection initializers are supported:
//  var p = new Options () {
//    { "h|?|help", v => ShowHelp () },
//  };
//
// System.ComponentModel.TypeConverter is also supported, allowing the use of
// custom data types in the callback type; TypeConverter.ConvertFromString()
// is used to convert the value option to an instance of the specified
// type:
//
//  var p = new Options () {
//    { "foo=", (Foo f) => Console.WriteLine (f.ToString ()) },
//  };
//
// Random other tidbits:
//  - Boolean options (those w/o '=' or ':' in the option format string)
//    are explicitly enabled if they are followed with '+', and explicitly
//    disabled if they are followed with '-':
//      string a = null;
//      var p = new Options () {
//        { "a", s => a = s },
//      };
//      p.Parse (new string[]{"-a"});   // sets v != null
//      p.Parse (new string[]{"-a+"});  // sets v != null
//      p.Parse (new string[]{"-a-"});  // sets v == null
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

#if TEST
using Mono.Documentation;
#endif

namespace Mono.Documentation {

	enum OptionValue {
		None, 
		Optional,
		Required
	}

	public class Option {
		string prototype, description;
		Action<string> action;
		string[] prototypes;
		OptionValue type;

		public Option (string prototype, string description, Action<string> action)
		{
			if (prototype == null)
				throw new ArgumentNullException ("prototype");
			if (prototype.Length == 0)
				throw new ArgumentException ("Cannot be the empty string.", "prototype");
			if (action == null)
				throw new ArgumentNullException ("action");
			this.prototype   = prototype;
			this.prototypes  = prototype.Split ('|');
			this.description = description;
			this.action = action;
			this.type = GetOptionValue ();
		}

		public string Prototype { get { return prototype; } }
		public string Description { get { return description; } }
		public Action<string> Action { get { return action; } }

		internal string[] Prototypes { get { return prototypes; } }
		internal OptionValue OptionValue { get { return type; } }

		OptionValue GetOptionValue ()
		{
			foreach (string n in Prototypes) {
				if (n.IndexOf ('=') >= 0)
					return OptionValue.Required;
				if (n.IndexOf (':') >= 0)
					return OptionValue.Optional;
			}
			return OptionValue.None;
		}

		public override string ToString ()
		{
			return Prototype;
		}
	}

	[Serializable]
	public class OptionException : Exception {
		private string option;

		public OptionException (string messageFormat, string optionName)
			: base (string.Format (messageFormat, optionName))
		{
			this.option = optionName;
		}

		public OptionException (string messageFormat, string optionName, Exception innerException)
			: base (string.Format (messageFormat, optionName), innerException)
		{
			this.option = optionName;
		}

		protected OptionException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.option = info.GetString ("OptionName");
		}

		public string OptionName {
			get {return this.option;}
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("OptionName", option);
		}
	}

	public class Options : Collection<Option>
	{
		Dictionary<string, Option> options = new Dictionary<string, Option> ();

		protected override void ClearItems ()
		{
			this.options.Clear ();
		}

		protected override void InsertItem (int index, Option item)
		{
			Add (item);
			base.InsertItem (index, item);
		}

		protected override void RemoveItem (int index)
		{
			Option p = Items [index];
			foreach (string name in GetOptionNames (p.Prototypes)) {
				this.options.Remove (name);
			}
			base.RemoveItem (index);
		}

		protected override void SetItem (int index, Option item)
		{
			RemoveItem (index);
			Add (item);
			base.SetItem (index, item);
		}

		public new Options Add (Option option)
		{
			foreach (string name in GetOptionNames (option.Prototypes)) {
				this.options.Add (name, option);
			}
			return this;
		}

		public Options Add (string options, Action<string> action)
		{
			return Add (options, null, action);
		}

		public Options Add (string options, string description, Action<string> action)
		{
			Option p = new Option (options, description, action);
			base.Add (p);
			return this;
		}

		public Options Add<T> (string options, Action<T> action)
		{
			return Add (options, null, action);
		}

		public Options Add<T> (string options, string description, Action<T> action)
		{
			TypeConverter c = TypeDescriptor.GetConverter (typeof(T));
			Action<string> a = delegate (string s) {
				T t = default(T);
				try {
					if (s != null)
						t = (T) c.ConvertFromString (s);
				}
				catch (Exception e) {
					throw new OptionException (
							string.Format ("Could not convert string `{0}' to type {1} for options `{{0}}'.",
								s, typeof(T).Name), options, e);
				}
				action (t);
			};
			return Add (options, description, a);
		}

		static readonly char[] NameTerminator = new char[]{'=', ':'};
		static IEnumerable<string> GetOptionNames (string[] names)
		{
			foreach (string name in names) {
				int end = name.IndexOfAny (NameTerminator);
				if (end >= 0)
					yield return name.Substring (0, end);
				else 
					yield return name;
			}
		}

		static readonly Regex ValueOption = new Regex (
			@"^(?<flag>--|-|/)(?<name>[^:=]+)([:=](?<value>.*))?$");

		public List<string> Parse (IEnumerable<string> options)
		{
			Option prev = null;
			string prevOption = null;
			bool process = true;
			List<string> unprocessed = new List<string> ();
			foreach (string option in options) {
				if (option == "--") {
					process = false;
					continue;
				}
				if (!process) {
					unprocessed.Add (option);
					continue;
				}
				if (prev != null) {
					prev.Action (option);
					prev = null;
					prevOption = null;
					continue;
				}
				Match m = ValueOption.Match (option);
				if (!m.Success) {
					unprocessed.Add (option);
					continue;
				}
				string f = m.Groups ["flag"].Value;
				string n = m.Groups ["name"].Value;
				string v = !m.Groups ["value"].Success 
					? null 
					: m.Groups ["value"].Value;
				do {
					if (this.options.TryGetValue (n, out prev)) {
						prevOption = option;
						break;
					}
					// no match; is it a bool option?
					if (n.Length >= 1 && (n [n.Length-1] == '+' || n [n.Length-1] == '-') &&
							this.options.TryGetValue (n.Substring (0, n.Length-1), out prev)) {
						v = n [n.Length-1] == '+' ? n : null;
						prev.Action (v);
						prev = null;
						break;
					}
					// is it a bundled option?
					if (f == "-" && this.options.TryGetValue (n [0].ToString (), out prev)) {
						int i = 0;
						do {
							if (prev.OptionValue != OptionValue.None)
								throw new OptionException ("Cannot bundle option '{0}' that requires a value.", 
										"-" + n [i].ToString ());
							prev.Action (n);
						} while (++i < n.Length && this.options.TryGetValue (n [i].ToString (), out prev));
						prev = null;
					}
					else
						unprocessed.Add (option);
				} while (false);
				if (prev != null) {
					switch (prev.OptionValue) {
						case OptionValue.None:
							prev.Action (n);
							prev = null;
							break;
						case OptionValue.Optional:
						case OptionValue.Required: 
							if (v != null) {
								prev.Action (v);
								prev = null;
							}
							break;
					}
				}
			}
			if (prev != null)
				NoValue (ref prev, ref prevOption);
			return unprocessed;
		}

		static void NoValue (ref Option p, ref string option)
		{
			if (p != null && p.OptionValue == OptionValue.Optional) {
				p.Action (null);
				p = null;
			}
			else if (p != null && p.OptionValue == OptionValue.Required) {
				throw new OptionException ("Missing required value for option '{0}'.", option);
			}
			option = null;
		}

		const int OptionWidth = 29;

		public void WriteOptionDescriptions (TextWriter o)
		{
			foreach (Option p in this) {
				List<string> names = new List<string> (GetOptionNames (p.Prototypes));

				int written = 0;
				if (names [0].Length == 1) {
					Write (o, ref written, "  -");
					Write (o, ref written, names [0]);
				}
				else {
					Write (o, ref written, "      --");
					Write (o, ref written, names [0]);
				}

				for (int i = 1; i < names.Count; ++i) {
					Write (o, ref written, ", ");
					Write (o, ref written, names [i].Length == 1 ? "-" : "--");
					Write (o, ref written, names [i]);
				}

				if (p.OptionValue == OptionValue.Optional)
					Write (o, ref written, "[=VALUE]");
				else if (p.OptionValue == OptionValue.Required)
					Write (o, ref written, "=VALUE");

				if (written < OptionWidth)
					o.Write (new string (' ', OptionWidth - written));
				else {
					o.WriteLine ();
					o.Write (new string (' ', OptionWidth));
				}

				o.WriteLine (p.Description);
			}
		}

		static void Write (TextWriter o, ref int n, string s)
		{
			n += s.Length;
			o.Write (s);
		}
	}
}

#if TEST
namespace MonoTests.Mono.Documentation {
	using System.Linq;

	class FooConverter : TypeConverter {
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
				CultureInfo culture, object value)
		{
			string v = value as string;
			if (v != null) {
				switch (v) {
					case "A": return Foo.A;
					case "B": return Foo.B;
				}
			}

			return base.ConvertFrom (context, culture, value);
		}
	}

	[TypeConverter (typeof(FooConverter))]
	class Foo {
		public static readonly Foo A = new Foo ("A");
		public static readonly Foo B = new Foo ("B");
		string s;
		Foo (string s) { this.s = s; }
		public override string ToString () {return s;}
	}

	class Test {
		public static void Main (string[] args)
		{
			var tests = new Dictionary<string, Action> () {
				{ "boolean",      () => CheckBoolean () },
				{ "bundling",     () => CheckOptionBundling () },
				{ "descriptions", () => CheckWriteOptionDescriptions () },
				{ "exceptions",   () => CheckExceptions () },
				{ "halt",         () => CheckHaltProcessing () },
				{ "many",         () => CheckMany () },
				{ "optional",     () => CheckOptional () },
				{ "required",     () => CheckRequired () },
			};
			bool run  = true;
			bool help = false;
			var p = new Options () {
				{ "t|test=", 
					"Run the specified test.  Valid tests:\n" + new string (' ', 32) +
						string.Join ("\n" + new string (' ', 32), tests.Keys.OrderBy (s => s).ToArray ()),
					v => { run = false; Console.WriteLine (v); tests [v] (); } },
				{ "h|?|help", "Show this message and exit", (v) => help = v != null },
			};
			p.Parse (args);
			if (help) {
				Console.WriteLine ("usage: Options.exe [OPTION]+\n");
				Console.WriteLine ("Options unit test program.");
				Console.WriteLine ("Valid options include:");
				p.WriteOptionDescriptions (Console.Out);
			} else if (run) {
				foreach (Action a in tests.Values)
					a ();
			}
		}

		static IEnumerable<string> _ (params string[] a)
		{
			return a;
		}

		static void CheckRequired ()
		{
			string a = null;
			int n = 0;
			Options p = new Options () {
				{ "a=", v => a = v },
				{ "n=", (int v) => n = v },
			};
			List<string> extra = p.Parse (_("a", "-a", "s", "-n=42", "n"));
			Assert (extra.Count, 2);
			Assert (extra [0], "a");
			Assert (extra [1], "n");
			Assert (a, "s");
			Assert (n, 42);

			extra = p.Parse (_("-a="));
			Assert (extra.Count, 0);
			Assert (a, "");
		}

		static void CheckOptional ()
		{
			string a = null;
			int n = -1;
			Foo f = null;
			Options p = new Options () {
				{ "a:", v => a = v },
				{ "n:", (int v) => n = v },
				{ "f:", (Foo v) => f = v },
			};
			p.Parse (_("-a=s"));
			Assert (a, "s");
			p.Parse (_("-a"));
			Assert (a, null);
			p.Parse (_("-a="));
			Assert (a, "");

			p.Parse (_("-f", "A"));
			Assert (f, Foo.A);
			p.Parse (_("-f"));
			Assert (f, null);

			p.Parse (_("-n", "42"));
			Assert (n, 42);
			p.Parse (_("-n"));
			Assert (n, 0);
		}

		static void CheckBoolean ()
		{
			bool a = false;
			Options p = new Options () {
				{ "a", v => a = v != null },
			};
			p.Parse (_("-a"));
			Assert (a, true);
			p.Parse (_("-a+"));
			Assert (a, true);
			p.Parse (_("-a-"));
			Assert (a, false);
		}

		static void CheckMany ()
		{
			int a = -1, b = -1;
			string av = null, bv = null;
			Foo f = null;
			int help = 0;
			int verbose = 0;
			Options p = new Options () {
				{ "a=", v => { a = 1; av = v; } },
				{ "b", "desc", v => {b = 2; bv = v;} },
				{ "f=", (Foo v) => f = v },
				{ "v", v => { ++verbose; } },
				{ "h|?|help", (v) => { switch (v) {
					case "h": help |= 0x1; break; 
					case "?": help |= 0x2; break;
					case "help": help |= 0x4; break;
				} } },
			};
			List<string> e = p.Parse (new string[]{"foo", "-v", "-a=42", "/b-",
				"-a", "64", "bar", "--f", "B", "/h", "-?", "--help", "-v"});

			Assert (e.Count, 2);
			Assert (e[0], "foo");
			Assert (e[1], "bar");
			Assert (a, 1);
			Assert (av, "64");
			Assert (b, 2);
			Assert (bv, null);
			Assert (verbose, 2);
			Assert (help, 0x7);
			Assert (f, Foo.B);
		}

		static void Assert<T>(T actual, T expected)
		{
			if (!object.Equals (actual, expected))
				throw new InvalidOperationException (
					string.Format ("Assertion failed: {0} != {1}", actual, expected));
		}

		static void CheckExceptions ()
		{
			string a = null;
			var p = new Options () {
				{ "a=", v => a = v },
				{ "c",  v => { } },
				{ "n=", (int v) => { } },
				{ "f=", (Foo v) => { } },
			};
			// missing argument
			AssertException (typeof(OptionException), 
					"Missing required value for option '-a'.", 
					p, v => { v.Parse (_("-a")); });
			// another named option while expecting one -- follow Getopt::Long
			AssertException (null, null,
					p, v => { v.Parse (_("-a", "-a")); });
			Assert (a, "-a");
			// no exception when an unregistered named option follows.
			AssertException (null, null, 
					p, v => { v.Parse (_("-a", "-b")); });
			Assert (a, "-b");

			// bad type
			AssertException (typeof(OptionException),
					"Could not convert string `value' to type Int32 for options `n='.",
					p, v => { v.Parse (_("-n", "value")); });
			AssertException (typeof(OptionException),
					"Could not convert string `invalid' to type Foo for options `f='.",
					p, v => { v.Parse (_("-f", "invalid")); });

			// try to bundle with an option requiring a value
			AssertException (typeof(OptionException), 
					"Cannot bundle option '-a' that requires a value.", 
					p, v => { v.Parse (_("-ca", "value")); });

			AssertException (typeof(ArgumentNullException), 
					"Argument cannot be null.\nParameter name: prototype", 
					p, v => { new Option (null, null, null); });
			AssertException (typeof(ArgumentException), 
					"Cannot be the empty string.\nParameter name: prototype",
					p, v => { new Option ("", null, null); });
			AssertException (typeof(ArgumentNullException), 
					"Argument cannot be null.\nParameter name: action",
					p, v => { new Option ("p", null, null); });
		}

		static void AssertException<T> (Type exception, string message, T a, Action<T> action)
		{
			Type actualType = null;
			string stack = null;
			string actualMessage = null;
			try {
				action (a);
			}
			catch (Exception e) {
				actualType    = e.GetType ();
				actualMessage = e.Message;
				if (!object.Equals (actualType, exception))
					stack = e.ToString ();
			}
			if (!object.Equals (actualType, exception)) {
				throw new InvalidOperationException (
					string.Format ("Assertion failed: Expected Exception Type {0}, got {1}.\n" +
						"Actual Exception: {2}", exception, actualType, stack));
			}
			if (!object.Equals (actualMessage, message))
				throw new InvalidOperationException (
					string.Format ("Assertion failed:\n\tExpected: {0}\n\t  Actual: {1}",
						message, actualMessage));
		}

		static void CheckWriteOptionDescriptions ()
		{
			var p = new Options () {
				{ "p|indicator-style=", "append / indicator to directories", v => {} },
				{ "color:", "controls color info", v => {} },
				{ "h|?|help", "show help text", v => {} },
				{ "version", "output version information and exit", v => {} },
			};

			StringWriter expected = new StringWriter ();
			expected.WriteLine ("  -p, --indicator-style=VALUE");
			expected.WriteLine ("                             append / indicator to directories");
			expected.WriteLine ("      --color[=VALUE]        controls color info");
			expected.WriteLine ("  -h, -?, --help             show help text");
			expected.WriteLine ("      --version              output version information and exit");

			StringWriter actual = new StringWriter ();
			p.WriteOptionDescriptions (actual);

			Assert (actual.ToString (), expected.ToString ());
		}

		static void CheckOptionBundling ()
		{
			string a, b, c;
			a = b = c = null;
			var p = new Options () {
				{ "a", v => a = "a" },
				{ "b", v => b = "b" },
				{ "c", v => c = "c" },
			};
			p.Parse (_ ("-abc"));
			Assert (a, "a");
			Assert (b, "b");
			Assert (c, "c");
		}

		static void CheckHaltProcessing ()
		{
			var p = new Options () {
				{ "a", v => {} },
				{ "b", v => {} },
			};
			List<string> e = p.Parse (_ ("-a", "-b", "--", "-a", "-b"));
			Assert (e.Count, 2);
			Assert (e [0], "-a");
			Assert (e [1], "-b");
		}
	}
}
#endif

