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
using ReleaseOss.Build;

namespace ReleaseOss.Data
{
    public class ModuleSourceFileCollection : FileCollection
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
                        return new ProjectFile(owner, file, subDirectories);
                    default:
                        return new RelevantFile(file, subDirectories);
                }
            }
        }

        private Module AddModule(string name, DirectoryInfo folder, string[] subDirectories)
        {
            var newModule = new Module(this, name, folder, subDirectories, CreateFileFactory);
            modules.Add(newModule);
            return newModule;
        }

        private readonly List<Module> modules = new List<Module>();

        public List<Module> Modules => modules;

        protected virtual bool SuspectModulesInFolder(DirectoryInfo folder, string[] subDirectories)
        {
            return true;
        }

        protected override object ScanFolder(DirectoryInfo folder, string[] subDirectories, object context)
        {
            var scanForModules = SuspectModulesInFolder(folder, subDirectories);
            var projectFiles = scanForModules ? folder.EnumerateFiles("*.csproj").Concat(folder.EnumerateFiles("*.vbproj")).ToArray() : new FileInfo[0];
            if (projectFiles.Length == 1)
            {
                OutputHelper.WriteLine(OutputKind.Debug, "module found: " + folder.Name);
                AddModule(Path.GetFileNameWithoutExtension(projectFiles[0].Name), folder, subDirectories/*.Concat(new[] { folder.Name }).ToArray()*/);
                return null;
            }
            else
            {
                return this;
            }
        }

        public override void Analyze(ApplicationSettings settings)
        {
            foreach (var m in modules)
            {
                m.Sources.Analyze(settings);
            }
        }

        public override void Prepare(ApplicationSettings settings)
        {
            foreach (var m in modules)
            {
                m.Sources.Prepare(settings);
            }
        }

        public override void TidyUpPreparationFiles(ApplicationSettings settings)
        {
            foreach (var m in modules)
            {
                m.Sources.TidyUpPreparationFiles(settings);
            }
        }

        public override void ProvideFiles(ApplicationSettings settings, FileEntryCallback addFile)
        {
            foreach (var m in modules)
            {
                m.ProvideFiles(settings, (filePath, entryName) =>
                {
                    addFile(filePath, LogicalNamePrefix + entryName);
                });
            }
        }

        protected override string LogicalNamePrefix => "src/";

        protected virtual IRelevantFileFactory CreateFileFactory(Module module)
        {
            return new FileFactory(module);
        }

        public IEnumerable<ProjectOutputInfo> AllProjects => modules.SelectMany(m => m.AllProjects);
    }
}
