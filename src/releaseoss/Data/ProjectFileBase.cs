/*
------------------------------------------------------------------------------
This source file is a part of Release OSS.

Copyright (c) 2017 Florian Haag

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
------------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace ReleaseOss.Data
{
    public abstract class ProjectFileBase : MsBuildBasedFile
    {
        public ProjectFileBase(Module owner, FileInfo file, string[] subDirectories) : base(file, subDirectories)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            this.owner = owner;
        }

        private readonly Module owner;

        public Module Owner => owner;

        public override void AnalyzeFile(ApplicationSettings settings)
        {
            OutputHelper.WriteLine(OutputKind.Debug, "Analyzing {0} (project {1}) ...", File.FullName, ProjectId);

            var doc = new XmlDocument();
            doc.Load(File.FullName);

            if (doc.DocumentElement.LocalName != "Project")
            {
                throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Unsupported file format; root node was {0}.",
                    doc.DocumentElement.LocalName));
            }

            var nsMgr = CreateNamespaceManager(doc);

            assemblyName = GetAssemblyName(doc, nsMgr);
            targetFrameworks = GetTargetFrameworks(doc, nsMgr);
        }

        protected XmlDocument LoadFile(ApplicationSettings settings, out XmlNamespaceManager nsMgr)
        {
            var doc = new XmlDocument();
            doc.Load(File.FullName);

            OutputHelper.WriteLine(OutputKind.Debug, "Loaded {0} (project {1})", File.FullName, ProjectId);

            if (doc.DocumentElement.LocalName != "Project")
            {
                throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Unsupported file format; root node was {0}.",
                    doc.DocumentElement.LocalName));
            }

            nsMgr = CreateNamespaceManager(doc);

            var ver = settings.CommandLine.BuildVerb.ReleaseVersion;

            // NuGet package version
            WriteMetaDataElement(doc, nsMgr, "Version", ver.ToString());

            // assembly version
            WriteMetaDataElement(doc, nsMgr, "AssemblyVersion",
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0}.{1}",
                    ver.Major, ver.Minor));

            // assembly file version
            WriteMetaDataElement(doc, nsMgr, "FileVersion",
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0}.{1}.{2}.0",
                    ver.Major, ver.Minor, ver.Patch));

            return doc;
        }

        private string assemblyName;

        private string GetAssemblyName(XmlDocument doc, XmlNamespaceManager nsMgr)
        {
            var nodes = doc.SelectNodes("/msbuild:Project/msbuild:PropertyGroup/msbuild:AssemblyName", nsMgr).OfType<XmlElement>().ToArray();
            switch (nodes.Length)
            {
                case 0:
                    return Path.GetFileNameWithoutExtension(File.Name);
                case 1:
                    return nodes[0].InnerText;
                default:
                    OutputHelper.WriteLine(OutputKind.Problem, "Several output names ({0}) found for the assembly from project {1}.", nodes.Length, File.FullName);
                    return null;
            }
        }

        private string[] targetFrameworks;

        protected IReadOnlyList<string> TargetFrameworks => targetFrameworks;

        private string[] GetTargetFrameworks(XmlDocument doc, XmlNamespaceManager nsMgr)
        {
            var result = new HashSet<string>();

            var nodes = doc.SelectNodes("/msbuild:Project/msbuild:PropertyGroup/msbuild:TargetFramework", nsMgr).OfType<XmlElement>().ToArray();
            foreach (var node in nodes)
            {
                result.Add(node.InnerText.Trim());
            }

            nodes = doc.SelectNodes("/msbuild:Project/msbuild:PropertyGroup/msbuild:TargetFrameworks", nsMgr).OfType<XmlElement>().ToArray();
            foreach (var node in nodes)
            {
                var targets = node.InnerText.Split(';');
                foreach (var trg in targets)
                {
                    result.Add(trg.Trim());
                }
            }

            if (result.Count <= 0)
            {
                nodes = doc.SelectNodes("/msbuild:Project/msbuild:PropertyGroup/msbuild:TargetFrameworkVersion", nsMgr).OfType<XmlElement>().ToArray();
                foreach (var node in nodes)
                {
                    var target = TargetFrameworkVersionToFrameworkId(node.InnerText);
                    if (target != null)
                    {
                        var profileNodes = node.SelectNodes("parent::msbuild:PropertyGroup/child::msbuild:TargetFrameworkProfile", nsMgr).OfType<XmlElement>().ToArray();
                        if (profileNodes.Length > 0)
                        {
                            foreach (var pn in profileNodes)
                            {
                                switch (pn.InnerText)
                                {
                                    case "Client":
                                        result.Add(target + "-client");
                                        break;
                                    default:
                                        OutputHelper.WriteLine(OutputKind.Problem, "Unknown target framework profile: {0}", pn.InnerText);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            result.Add(target);
                        }
                    }
                    else
                    {
                        OutputHelper.WriteLine(OutputKind.Problem, "Unknown target framework version: {0}", node.InnerText);
                    }
                }
            }

            return result.ToArray();
        }

        private static readonly Lazy<Regex> targetFrameworkVersionRegex = new Lazy<Regex>(() => new Regex(@"^v[0-9]+(?:\.[0-9]+)*$"));

        private static string TargetFrameworkVersionToFrameworkId(string tfv)
        {
            if (targetFrameworkVersionRegex.Value.IsMatch(tfv))
            {
                return "net" + tfv.Substring(1).Replace(".", "");
            }
            else
            {
                return null;
            }
        }

        public ProjectOutputInfo CreateOutputInfo()
        {
            OutputHelper.WriteLine(OutputKind.Debug, "Creating output info for project {0}.", ProjectId);
            return new ProjectOutputInfo(ProjectId, Path.GetFileNameWithoutExtension(File.Name), assemblyName, targetFrameworks);
        }

        public string ProjectId => string.Join("", AllSubDirectories.Select(sd => "/" + sd)) + "/" + File.Name;

        public string[] AllSubDirectories => Enumerable.Concat(owner.SubDirectories ?? new string[0], SubDirectories ?? new string[0]).ToArray();
    }
}
