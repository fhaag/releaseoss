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
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReleaseOss.Data
{
    public sealed class SampleProjectFile : ProjectFileBase
    {
        public SampleProjectFile(Module owner, FileInfo file, string[] subDirectories) : base(owner, file, subDirectories)
        {
            temporaryFileName = Guid.NewGuid().ToString("N") + file.Extension;
        }

        private readonly string temporaryFileName;

        public override string EffectivePath(ApplicationSettings settings)
        {
            return Path.Combine(settings.CommandLine.BuildVerb.TemporaryPath, temporaryFileName);
        }

        public override void PrepareFile(ApplicationSettings settings)
        {
            base.PrepareFile(settings);

            var doc = LoadFile(settings, out var nsMgr);

            // TODO: update/generate solution file that only includes sample projects

            UpdateReferences(doc, nsMgr);

            var tmpPath = EffectivePath(settings);
            OutputHelper.WriteLine(OutputKind.Debug, "Writing sample project {0} to {1} ...", File.Name, tmpPath);
            doc.Save(tmpPath);
        }

        private void UpdateReferences(XmlDocument doc, XmlNamespaceManager nsMgr)
        {
            var msBuildUri = nsMgr.LookupNamespace("msbuild");
            var projectsWithSources = new HashSet<string>(Owner.Owner.AllProjects.Select(p => p.ProjectId));
            var localPath = AllSubDirectories;

            var pjRefs = doc.SelectNodes("/msbuild:Project/msbuild:ItemGroup/msbuild:ProjectReference", nsMgr).OfType<XmlElement>().ToArray();
            foreach (var pjRef in pjRefs)
            {
                var rawPath = pjRef.GetAttribute("Include");
                if (!string.IsNullOrEmpty(rawPath))
                {
                    var referencedProjectId = DecodeProjectReference(localPath, rawPath);
                    if (referencedProjectId != null)
                    {
                        if (!projectsWithSources.Contains(referencedProjectId))
                        {
                            ProjectOutputInfo poi;
                            if (((SampleModuleSourceFileCollection)(Owner.Owner)).GloballyAllProjects.TryGetValue(referencedProjectId, out poi))
                            {
                                var newElements = new List<XmlElement>();

                                foreach (var trg in TargetFrameworks)
                                {
                                    var newRefEl = doc.CreateElement(doc.DocumentElement.Prefix, "Reference", msBuildUri);
                                    if (TargetFrameworks.Count > 1)
                                    {
                                        newRefEl.SetAttribute("Condition", "'$(TargetFramework)' == '" + trg + "'");
                                    }
                                    newRefEl.SetAttribute("Include", poi.AssemblyName);

                                    var hintPath = new StringBuilder();
                                    for (int i = 0; i < localPath.Length; i++)
                                    {
                                        hintPath.Append(@"..\");
                                    }
                                    if (poi.TargetFrameworks.Contains(trg))
                                    {
                                        if (poi.TargetFrameworks.Count > 1)
                                        {
                                            hintPath.Append(trg + @"\");
                                        }
                                    }
                                    else
                                    {
                                        OutputHelper.WriteLine(OutputKind.Problem,
                                            "Project {0} targets {1}, but references project {2} that does not support that target.",
                                            ProjectId, trg, poi.ProjectId);
                                    }
                                    hintPath.Append(poi.AssemblyName + ".dll");

                                    var hintPathEl = doc.CreateElement(doc.DocumentElement.Prefix, "HintPath", msBuildUri);
                                    hintPathEl.InnerText = hintPath.ToString();
                                    newRefEl.AppendChild(hintPathEl);

                                    newElements.Add(newRefEl);
                                }

                                Console.WriteLine("Replacing");
                                var parentNode = pjRef.ParentNode;
                                for (int i = 0; i < newElements.Count; i++)
                                {
                                    if (i == 0)
                                    {
                                        parentNode.ReplaceChild(newElements[0], pjRef);
                                    }
                                    else
                                    {
                                        parentNode.InsertAfter(newElements[i], newElements[i - 1]);
                                    }
                                }
                            }
                        }
                    }
                }
                /*
    <Reference Include="FlexibleLocalization">
      <HintPath>..\..\lib\FlexibleLocalization.dll</HintPath>
    </Reference>

    <ProjectReference Include="..\..\NameBasedGrid\NameBasedGrid.csproj">
      <Project>{01D2D040-A2AF-42A1-9821-D1C6D77A3309}</Project>
      <Name>NameBasedGrid</Name>
    </ProjectReference>
                 */
            }
        }

        private string DecodeProjectReference(string[] localPath, string includePath)
        {
            var includePathParts = includePath.Split('/', '\\');
            
            var resultPath = new List<string>(localPath);
            foreach (var part in includePathParts)
            {
                switch (part)
                {
                    case ".":
                        break;
                    case "..":
                        if (resultPath.Count > 0)
                        {
                            resultPath.RemoveAt(resultPath.Count - 1);
                        }
                        else
                        {
                            return null;
                        }
                        break;
                    default:
                        resultPath.Add(part);
                        break;
                }
            }
            
            return string.Join("", resultPath.Select(d => "/" + d));
        }
    }
}
