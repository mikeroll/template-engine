using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TemplateEngine
{
	public class Template
	{
		ILanguage language;

		public string TemplateString
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TemplateEngine.Template"/> class.
		/// </summary>
		/// <param name="snippet">Template string..</param>
		/// <param name="lang">ILanguage object.</param>
		/// <param name="assemblies">(C# only) Assemblies to reference in resulting program.</param>
		/// <param name="namespaces">(C# only) Namespaces to include in code.</param>
		/// <param name="json">(C# only) Json to access from code.</param>
		public Template(string snippet, ILanguage lang, 
		                      string[] assemblies = null,
		                      string[] namespaces = null,
		                      string json = null)
		{
			language = lang;
			language.AddExtras(assemblies, namespaces, json);
			TemplateString = snippet;
		}

		/// <summary>
		/// Render the template specified in constructor.
		/// </summary>
		/// <param name="output">TextWriter to hold program output.</param>
		public void Render(TextWriter output)
		{
			var chunks = new Parser(TemplateString).Parse();
			language.ComposeCode(chunks);
			language.CompileAndRun(output);
		}
	}
}
