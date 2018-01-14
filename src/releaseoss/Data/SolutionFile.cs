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

using net.r_eg.MvsSln;
using net.r_eg.MvsSln.Core;
using net.r_eg.MvsSln.Core.ObjHandlers;
using net.r_eg.MvsSln.Core.SlnHandlers;

namespace ReleaseOss.Data
{
    public class SolutionFile : RelevantFile
    {
        public SolutionFile(FileInfo file, string[] subDirectories) : base(file, subDirectories)
        {
        }

        private readonly ISet<string> includedProjects = new HashSet<string>();

        public override void AnalyzeFile(ApplicationSettings settings)
        {
            base.AnalyzeFile(settings);

            using (var sln = new Sln(File.FullName, SlnItems.Env))
            {
                foreach (var pj in sln.Result.ProjectItems)
                {
                    var projectId = string.Join("", pj.path.Split('\\').Select(p => "/" + p));
                    includedProjects.Add(projectId);
                    OutputHelper.WriteLine(OutputKind.Debug, "Found reference to project {0}.", projectId);
                }

                var pjWriteHandlers = new Dictionary<Type, HandlerValue>
                {
                    [typeof(LProject)] = new HandlerValue(new WProject(null, sln.Result.ProjectDependencies)) // TODO: set list of projects to include
                };

                using (var w = new SlnWriter("TODO", pjWriteHandlers))
                {
                    w.Write(sln.Result.Map);
                }
            }
        }
    }
}
