using System;
using System.Text;
using System.IO;

#pragma warning disable 0219

namespace TemplateEngine
{
	public class CLI
	{
		public static void Main(string[] args)
		{
            string usage = @"Usage: template <source>";
            if (args.Length < 1)
            {
                Console.WriteLine(usage);
            }

            string filename = args[0];
            string outname;
            ILanguage lang = null;
            if (filename.EndsWith(".t.rb"))
                lang = new Ruby();
            if (filename.EndsWith(".t.py"))
                lang = new Python3();
            if (filename.EndsWith(".t.java"))
                lang = new Java7();
            if (filename.EndsWith(".t.cs"))
                lang = new CSharp();
            if (lang == null)
            {
                Console.WriteLine("Unknown file type!");
                return;
            }
            else
            {
                outname = filename.Remove(filename.LastIndexOf(".t."));
            }

            string contents = File.ReadAllText(filename);
            Template t = new Template(contents, lang);

            TextWriter writer = File.CreateText(outname);

            IdeoneJob.Authorize("mikeroll", "lucky_starfish");
            try
            {
                t.Render(writer);
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot process template.");
                File.Delete(outname);
            }
            writer.Close();
		}
	}
}