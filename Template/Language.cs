using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace TemplateEngine
{
	public interface ILanguage
	{
		/// <summary>
		/// (C# only) Optional parameters for template.
		/// </summary>
		/// <param name="assemblies">Assemblies.</param>
		/// <param name="namespaces">Namespaces.</param>
		/// <param name="json">Json.</param>
		void AddExtras(string[] assemblies, string[] namespaces, string json);

		/// <summary>
		/// Composes the code by concating chunks from codePack.
		/// </summary>
		/// <returns>Code in single string.</returns>
		/// <param name="codePack">Chunks of code.</param>
		string ComposeCode(List<Tuple<TokenType, string>> codePack);

		/// <summary>
		/// Compiles composed code and runs it.
		/// </summary>
		/// <returns>TextWriter holding program output.</returns>
		/// <param name="output">TextWriter to hold output.</param>
		void CompileAndRun(TextWriter output);
	}

	public class CSharp : ILanguage
	{
		public string CodeToRun
		{ 
			get; 
			private set;
		}

		public string OutString
		{ 
			get; 
			private set; 
		}

		public string[] Assemblies
		{ 
			get; 
			set; 
		}

		public string[] Namespaces
		{ 
			get; 
			set; 
		}

		public string Json
		{ 
			get; 
			set; 
		}

		public void AddExtras(string[] assemblies, string[] namespaces, string json)
		{
			Assemblies = assemblies;
			Namespaces = namespaces;
			Json = json;
		}

		Assembly BuildAssembly(string code)
		{
			CodeDomProvider codeProvider = new CSharpCodeProvider();
			CompilerParameters parameters = new CompilerParameters();
			if (Assemblies != null)
			{
				foreach (string a in Assemblies)
				{
					parameters.ReferencedAssemblies.Add(a);
				}
			}
			if (Json != null)
			{
				parameters.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
			}
			parameters.CompilerOptions = "/optimize";
			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = true;
			parameters.IncludeDebugInformation = false;
			var results = codeProvider.CompileAssemblyFromSource(
				parameters,
				code
			);
			if (results.Errors.HasErrors)
			{
				throw new BadCodeException("Compile error");
			}
			return results.CompiledAssembly;
		}

		public string ComposeCode(List<Tuple<TokenType, string>> codePack)
		{
			StringBuilder code = new StringBuilder();
            code.Append("using System;\n\n");
			code.Append("using System.IO;\n\n");
			if (Namespaces != null)
			{
				foreach (string n in Namespaces)
				{
					code.Append("using " + n + ";\n");
				}
			}
			if (Json != null)
			{
				code.Append("using Newtonsoft.Json;\n");
			}
			code.Append("class CSharpTemplate\n{\n")
                .Append("public static TextWriter method(string json)\n{\n")
                .Append("StringWriter output = new StringWriter();\n");
			if (Json != null)
			{
				code.Append("JObject input = JObject.Parse(json);\n");
			}
			foreach (Tuple<TokenType, string> chunk in codePack)
			{
				switch (chunk.Item1)
				{
					case TokenType.Text:
						code.Append("output.Write(\"")
                            .Append(chunk.Item2)
                            .Append("\");\n");
						break;
					case TokenType.Eval:
						code.Append("output.Write((")
                            .Append(chunk.Item2)
                            .Append(").ToString());\n");
						break;
					case TokenType.Code:
						code.Append(chunk.Item2);
						break;
				}
			}
			code.Append("return output;")
                .Append("\n}\n}");
			CodeToRun = code.ToString();
			return CodeToRun;
		}

		public void CompileAndRun(TextWriter output)
		{
			object tw = new object();
			Assembly dll = BuildAssembly(CodeToRun);
			Type[] types = dll.GetTypes();
			foreach (Type t in types)
			{
				MethodInfo mi = t.GetMethod("method");
				if (mi != null)
				{
					string typeName = t.FullName;
					dll.CreateInstance(typeName);
					tw = mi.Invoke(t, new object[] { Json });
				}
			}
			OutString = tw.ToString();
			output.Write(OutString);
			output = (TextWriter)tw;
		}
	}

	public class Ruby : ILanguage
	{
		const int RubyNum = 17;
		public string CodeToRun
		{ 
			get; 
			private set; 
		}

		public string OutString
		{ 
			get; 
			private set; 
		}

		public void AddExtras(string[] assemblies, string[] namespaces, string json)
		{

		}

		public string ComposeCode(List<Tuple<TokenType, string>> codePack)
		{
			StringBuilder code = new StringBuilder();
			foreach (Tuple<TokenType, string> chunk in codePack)
			{
				switch (chunk.Item1)
				{
					case TokenType.Text:
						code.Append("print \"")
                            .Append(chunk.Item2)
                            .Append("\";");
						break;
					case TokenType.Eval:
						code.Append("print ")
                            .Append(chunk.Item2)
                            .Append(";");
						break;
                    case TokenType.Code:
                        code.Append(chunk.Item2)
                            .Append(";");
						break;
				}
			}
			CodeToRun = code.ToString();
			return CodeToRun;
		}

		public void CompileAndRun(TextWriter output)
		{
			var job = new IdeoneJob(CodeToRun, RubyNum);
			OutString = job.Execute();
			output.Write(OutString);
		}
	}

	public class Python3 : ILanguage
	{
		const int PyNum = 116;

		public string CodeToRun
		{ 
			get; 
			private set; 
		}

		public string OutString
		{ 
			get; 
			private set; 
		}

		public void AddExtras(string[] assemblies, string[] namespaces, string json)
		{
		}

		public string ComposeCode(List<Tuple<TokenType, string>> codePack)
		{
			StringBuilder code = new StringBuilder();
			foreach (Tuple<TokenType, string> chunk in codePack)
			{
				switch (chunk.Item1)
				{
					case TokenType.Text:
						code.Append(@"print(""")
                            .Append(chunk.Item2)
                                .Append(@""", end='');");
						break;
					case TokenType.Eval:
						code.Append(@"print(")
                            .Append(chunk.Item2)
                                .Append(@", end='');");
						break;
					case TokenType.Code:
						code.Append(chunk.Item2);
						break;
				}
			}
			CodeToRun = code.ToString();
			return CodeToRun;
		}

		public void CompileAndRun(TextWriter output)
		{
			var job = new IdeoneJob(CodeToRun, PyNum);
			OutString = job.Execute();
			output.Write(OutString);
		}
	}

	public class Java7 : ILanguage
	{
		const int JavaNum = 55;

		public string CodeToRun
		{
			get; 
			private set; 
		}

		public string OutString
		{ 
			get; 
			private set; 
		}

		public void AddExtras(string[] assemblies, string[] namespaces, string json)
		{
		}

		public string ComposeCode(List<Tuple<TokenType, string>> codePack)
		{
			StringBuilder code = new StringBuilder();
			code.Append("class JavaTemplate\n{\n")
            .Append("public static void main(String[] args)\n{\n");
			foreach (Tuple<TokenType, string> chunk in codePack)
			{
				switch (chunk.Item1)
				{
					case TokenType.Text:
						code.Append(@"System.out.print(""")
                        .Append(chunk.Item2)
                            .Append(@""");");
						break;
					case TokenType.Eval:
						code.Append(@"System.out.print(")
                        .Append(chunk.Item2)
                            .Append(@");");
						break;
					case TokenType.Code:
						code.Append(chunk.Item2);
						break;
				}
			}
			code.Append("\n}\n}");
			CodeToRun = code.ToString();
			return CodeToRun;
		}

		public void CompileAndRun(TextWriter output)
		{
			var job = new IdeoneJob(CodeToRun, JavaNum);
			OutString = job.Execute();
			output.Write(OutString);
		}
	}
}
