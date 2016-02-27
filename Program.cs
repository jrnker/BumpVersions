// Copyright(c) 2016 Christoffer Järnåker
// License and source: https://github.com/jrnker/BumpVersions

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace bumpversions
{
    class Program
    {
        static void Main(string[] args)
        {
            // We want at least two options or this will be pointless
            if (args.Length < 2)
                args = new string[] { "-?" };

            var path = args[0];

            bool major = false;
            bool minor = false;
            bool build = false;
            bool revision = false;

            bool reset = false;

            foreach (var a in args)
            {
                // Parse the options
                switch (a)
                {
                    // Will bump the respective part with +1
                    case "-major":
                        major = true;
                        break;
                    case "-minor":
                        minor = true;
                        break;
                    case "-build":
                        build = true;
                        break;
                    case "-revision":
                        revision = true;
                        break;

                    // Will set all after the mark to zero. E.g. v1.2.3.4 with a -build -reset will become v1.2.4.0.
                    // You normally want this
                    case "-reset":
                        reset = true;
                        break;

                    // Display help
                    case "-?":
                    case "-h":
                    case "--help":
                        Console.WriteLine(string.Format("{0}, v{1}", Assembly.GetExecutingAssembly().GetName().Name.ToString(), Assembly.GetExecutingAssembly().GetName().Version.ToString()));
                        Console.WriteLine("License, source and doc: https://github.com/jrnker/BumpVersions");
                        Console.WriteLine(@"
Usage:
  bumpversions.exe [pathtosearch] [-major|-minor|-build|-revision] [-reset]
  Note: if submitting a path with quotation marks, e.g. a path with spaces, then place space between the end of the path and the second quotation mark.

Example:
  bumpversions.exe C:\repo\BumpVersions\ -build -reset");
                        return;
                }
            }

            // Check if submitted path exists
            if (!Directory.Exists(path))
            {
                Console.WriteLine(string.Format("Directory {0} doesn't exist.", path));
                return;
            }

            // Find all assemblyinfo files
            var files = findAllFiles(path.Trim());

            // Array of what to search for and try to bump
            // First two are for C#, last two for VB.Net
            string[] prefixes = new string[] { "[assembly: AssemblyVersion(\"", "[assembly: AssemblyFileVersion(\"",
            "<Assembly: AssemblyVersion(\"", "<Assembly: AssemblyFileVersion(\""};

            // Iterate through the found files
            foreach (var f in files)
            {
                bool fileUpdated = false;
                try
                {
                    string data = "";
                    List<string> sresult = new List<string>();

                    // Read the file
                    using (StreamReader sr = new StreamReader(f))
                    {
                        data = sr.ReadToEnd();
                    }

                    // Search for matching part to bump
                    foreach (string s in data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var d = s;
                        foreach (string p in prefixes)
                        {
                            if (s.Length > p.Length && s.Substring(0, p.Length) == p)
                            {
                                // C#
                                //[assembly: AssemblyVersion("1.0.0.0")]
                                //[assembly: AssemblyFileVersion("1.0.0.0")]

                                // vb.net
                                //<Assembly: AssemblyVersion("2.6.0.0")>
                                //<Assembly: AssemblyFileVersion("2.6.0.0")>

                                // Set the ending of the string if we edit it.
                                // C# ends with ] while VB.Net ends with >
                                string postfix = "\")" + (p[0] == '[' ? "]" : ">");

                                Version v = null;
                                if (Version.TryParse(d.Replace(p, "").Split('"')[0], out v))
                                {
                                    int imajor = v.Major;
                                    int iminor = v.Minor;
                                    int ibuild = v.Build;
                                    int irevision = v.Revision;

                                    if (revision)
                                    {
                                        irevision += 1;
                                    }
                                    if (build)
                                    {
                                        ibuild += 1;
                                        if (reset)
                                        {
                                            irevision = 0;
                                        }
                                    }
                                    if (minor)
                                    {
                                        iminor += 1;
                                        if (reset)
                                        {
                                            ibuild = 0;
                                            irevision = 0;
                                        }
                                    }
                                    if (major)
                                    {
                                        imajor += 1;
                                        if (reset)
                                        {
                                            iminor = 0;
                                            ibuild = 0;
                                            irevision = 0;
                                        }
                                    }
                                    // Rebuild this line with the new version info
                                    d = p + new Version(imajor, iminor, ibuild, irevision) + postfix;
                                    fileUpdated = true;
                                }
                            }
                        }
                        // Add it to the rebuild file list
                        sresult.Add(d);
                    }

                    // If we've been busy, then write the file back with the information
                    if (fileUpdated)
                    {
                        Console.WriteLine(string.Format("Updated {0}", f));
                        using (StreamWriter sr = new StreamWriter(f))
                        {
                            foreach (var l in sresult)
                                sr.WriteLine(l);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format(" FAILED with error {0}", ex.Message));
                }
            }
        }
        /// <summary>
        /// Find all AssemblyInfo files in a particular path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>List of full paths to the found files</returns>
        private static List<string> findAllFiles(string path)
        {
            List<string> files = new List<string>();
            var find = "AssemblyInfo.*";

            foreach (var f in Directory.EnumerateFiles(path, find))
            {
                files.Add(f);
            }
            foreach (var d in Directory.EnumerateDirectories(path))
            {
                files.AddRange(findAllFiles(d));
            }
            return files;
        }
    }
}
