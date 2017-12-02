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
    public sealed class Module
    {
        public Module(ModuleSourceFileCollection owner, string name, DirectoryInfo folder, string[] subDirectories, Func<Module, IRelevantFileFactory> fileFactoryFactory)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }
            if (subDirectories == null)
            {
                throw new ArgumentNullException(nameof(subDirectories));
            }
            if (fileFactoryFactory == null)
            {
                throw new ArgumentNullException(nameof(fileFactoryFactory));
            }

            this.owner = owner;
            this.name = name;
            this.folder = folder;
            this.subDirectories = subDirectories;

            sources.FileFactory = fileFactoryFactory(this);
            sources.FindFiles(folder.FullName);
        }

        private readonly ModuleSourceFileCollection owner;

        public ModuleSourceFileCollection Owner => owner;

        private readonly string name;

        private readonly DirectoryInfo folder;
        
        private readonly string[] subDirectories;

        public IReadOnlyList<string> SubDirectories => subDirectories;

        public string[] GetAbsoluteName(RelevantFile fromFile, string[] pathParts)
        {
            var path = new List<string>(subDirectories);
            path.AddRange(fromFile.SubDirectories);

            foreach (var part in pathParts)
            {
                switch (part)
                {
                    case ".":
                        break;
                    case "..":
                        if (path.Count > 0)
                        {
                            path.RemoveAt(path.Count - 1);
                        }
                        else
                        {
                            return null;
                        }
                        break;
                    default:
                        path.Add(part);
                        break;
                }
            }

            return path.ToArray();
        }

        private readonly SourceCodeFileCollection sources = new SourceCodeFileCollection();

        public SourceCodeFileCollection Sources => sources;

        public void ProvideFiles(ApplicationSettings settings, Build.FileEntryCallback addFile)
        {
            var prefix = string.Join("", subDirectories.Select(d => d + "/"));
            sources.ProvideFiles(settings, (filePath, entryName) =>
            {
                addFile(filePath, prefix + entryName);
            });
        }

        public IEnumerable<ProjectOutputInfo> AllProjects => sources.AllProjects;
    }
}
