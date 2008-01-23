// Case-Insensitive OptionSet
using System;
using System.Collections.Generic;
using NDesk.Options;

class DemoOptionContext : OptionContext {
	public string OptionKey;
}

class DemoOptionSet : OptionSet {
	protected override void InsertItem (int index, Option item)
	{
		if (item.Prototype.ToLower () != item.Prototype)
			throw new ArgumentException ("prototypes must be lower-case!");
		base.InsertItem (index, item);
	}

	protected override OptionContext CreateOptionContext ()
	{
		return new DemoOptionContext ();
	}

	protected override bool Parse (string option, OptionContext c)
	{
		DemoOptionContext d = (DemoOptionContext) c;
		// Prevent --a --b
		string f, n, v;
		bool haveParts = GetOptionParts (option, out f, out n, out v);
		Option nextOption = haveParts ? GetOptionForName (n.ToLower ()) : null;
		if (haveParts && c.Option != null) {
			if (nextOption == null)
				; // ignore
			else if (c.Option.OptionValueType == OptionValueType.Optional) {
				c.OptionValue = null;
				c.Option.Invoke (c);
			}
			else 
				throw new OptionException (
					string.Format ("Found option value `{0}' for option `{1}'.",
						option, c.OptionName), c.OptionName);
		}

		// option name already found, so `option' is the option value
		if (c.Option != null) {
			if (c.Option is KeyValueOption && d.OptionKey == null) {
				HandleKeyValue (option, d);
				return true;
			}
			return base.Parse (option, c);
		}

		if (!haveParts)
			// Not an option; let base handle as a non-option argument.
			return base.Parse (option, c);

		// use lower-case version of the option name.
		if (nextOption != null && nextOption is KeyValueOption) {
			d.Option     = nextOption;
			d.OptionName = f + n.ToLower ();
			HandleKeyValue (v, d);
			return true;
		}
		return base.Parse (f + n.ToLower () + (v != null ? "=" + v : ""), c);
	}

	static void HandleKeyValue (string option, DemoOptionContext d)
	{
		if (option == null)
			return;
		string[] parts = option.Split ('=');
		if (parts.Length == 1) {
			d.OptionKey = option;
			return;
		}
		d.OptionKey   = parts [0];
		d.OptionValue = parts [1];
		if (d.Option != null) {
			d.Option.Invoke (d);
		}
	}

	class KeyValueOption : Option {
		public KeyValueOption (string prototype, Action<string,string,OptionContext> action)
			: base (prototype, null)
		{
			this.action = action;
		}

		Action<string,string,OptionContext> action;

		public override void Invoke (OptionContext c)
		{
			DemoOptionContext d = (DemoOptionContext) c;
			action (d.OptionKey, d.OptionValue, d);
			d.OptionKey = null;
			base.Invoke (c);
		}
	}

	public new DemoOptionSet Add (string prototype,
		Action<string,string,OptionContext> action)
	{
		base.Add (new KeyValueOption (prototype, action));
		return this;
	}
}

class Demo {
	public static void Main (string[] args)
	{
		bool show_help = false;
		List<string> names = new List<string> ();
		Dictionary<string,string> map = new Dictionary<string,string> ();
		int repeat = 1;

		OptionSet p = new DemoOptionSet () {
			{ "n|name=",    v => names.Add (v) },
			{ "r|repeat:",  (int v) => repeat = v },
			{ "m|map=",     (k,v,c) => map.Add (k, v) },
		};

		List<string> extra;
		try {
			extra = p.Parse (args);
		}
		catch (OptionException e) {
			Console.Write ("subclass: ");
			Console.WriteLine (e.Message);
			return;
		}

		string message;
		if (extra.Count > 0) {
			message = string.Join (" ", extra.ToArray ());
		}
		else {
			message = "Hello {0}!";
		}

		foreach (string name in names) {
			for (int i = 0; i < repeat; ++i)
				Console.WriteLine (message, name);
		}
		List<string> keys = new List<string>(map.Keys);
		keys.Sort ();
		foreach (string key in keys) {
			Console.WriteLine ("Key: {0}={1}", key, map [key]);
		}
	}
}

