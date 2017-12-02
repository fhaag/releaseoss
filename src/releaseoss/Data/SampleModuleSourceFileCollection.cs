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

namespace ReleaseOss.Data
{
    public class SampleModuleSourceFileCollection : ModuleSourceFileCollection
    {
        private sealed class FileFactory : IRelevantFileFactory
        {
            public FileFactory(Module owner)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException(nameof(owner));
                }

                this.owner = owner;
            }

            private readonly Module owner;

            public RelevantFile CreateFile(FileInfo file, string[] subDirectories)
            {
                switch (Path.GetExtension(file.Name))
                {
                    case ".suo":
                    case ".user":
                    case ".bak":
                    case ".old":
                    case ".cache":
                        return null;
                    case ".csproj":
                    case ".vbproj":
                        return new SampleProjectFile(owner, file, subDirectories);
                    default:
                        return new RelevantFile(file, subDirectories);
                }
            }
        }

        public SampleModuleSourceFileCollection(IEnumerable<ProjectOutputInfo> globallyAllProjects)
        {
            if (globallyAllProjects == null)
            {
                throw new ArgumentNullException(nameof(globallyAllProjects));
            }

            this.globallyAllProjects = globallyAllProjects.ToDictionary(poi => poi.ProjectId);
        }

        private readonly IReadOnlyDictionary<string, ProjectOutputInfo> globallyAllProjects;

        public IReadOnlyDictionary<string, ProjectOutputInfo> GloballyAllProjects => globallyAllProjects;

        protected override bool SuspectModulesInFolder(DirectoryInfo folder, string[] subDirectories)
        {
            if (subDirectories.Length > 0)
            {
                switch (subDirectories[0])
                {
                    case "Samples":
                    case "Examples":
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected override string LogicalNamePrefix => "";

        protected override IRelevantFileFactory CreateFileFactory(Module module)
        {
            return new FileFactory(module);
        }
    }
}
