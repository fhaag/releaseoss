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
    public sealed class Project
    {
        private readonly ModuleSourceFileCollection modules = new ModuleSourceFileCollection();

        private readonly ReleaseNotesFileCollection releaseNotes = new ReleaseNotesFileCollection();

        private readonly HelpSourceFileCollection helpSources = new HelpSourceFileCollection();

        private readonly KeyFileCollection keyFiles = new KeyFileCollection();

        private readonly PublicInfoFileCollection publicInfoFiles = new PublicInfoFileCollection();

        private readonly SolutionFileCollection solutions = new SolutionFileCollection();

        private SampleModuleSourceFileCollection sampleFiles;

        private IEnumerable<FileCollection> AllFileCollections
        {
            get
            {
                yield return modules;
                yield return releaseNotes;
                yield return helpSources;
                yield return keyFiles;
                yield return publicInfoFiles;
                if (sampleFiles != null)
                {
                    yield return sampleFiles;
                }
            }
        }

        public void FindContents(ApplicationSettings settings)
        {
            var releaseVerb = settings.CommandLine.ActiveVerb as CommandLineReleaseSettingsBase;
            if (releaseVerb == null)
            {
                throw new InvalidOperationException("The application is not being executed with a release-related command.");
            }

            var rootPath = releaseVerb.RootPath;

            solutions.FindFiles(Path.Combine(rootPath, "src"));
            modules.FindFiles(Path.Combine(rootPath, "src"));
            releaseNotes.FindFiles(rootPath);
            helpSources.FindFiles(Path.Combine(rootPath, "doc"));
            keyFiles.FindFiles(Path.Combine(rootPath, "keys"));
            publicInfoFiles.FindFiles(Path.Combine(rootPath, "pubinfo"));
        }

        /// <summary>
        /// Collects information found within the files belonging to the project.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <returns>A value that indicates whether the operation was successful.</returns>
        public bool AnalyzeContents(ApplicationSettings settings)
        {
            var releaseVerb = settings.CommandLine.ActiveVerb as CommandLineReleaseSettingsBase;
            if (releaseVerb == null)
            {
                throw new InvalidOperationException("The application is not being executed with a release-related command.");
            }
            if (releaseVerb.ReleaseVersion == null)
            {
                throw new InvalidOperationException("No release version set.");
            }

            foreach (var fc in AllFileCollections)
            {
                fc.Analyze(settings);
            }

            return true;
        }

        /// <summary>
        /// Prepares temporary files based upon all detected files in the project.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <returns>A value that indicates whether the operation was successful.</returns>
        public bool PrepareContents(ApplicationSettings settings)
        {
            var releaseVerb = settings.CommandLine.ActiveVerb as CommandLineReleaseSettingsBase;
            if (releaseVerb == null)
            {
                throw new InvalidOperationException("The application is not being executed with a release-related command.");
            }
            if (releaseVerb.ReleaseVersion == null)
            {
                throw new InvalidOperationException("No release version set.");
            }
            
            Directory.CreateDirectory(releaseVerb.ReleasePath);
            Directory.CreateDirectory(releaseVerb.TemporaryPath);

            foreach (var fc in AllFileCollections)
            {
                fc.Prepare(settings);
            }

            try
            {
                if (!solutions.Build(settings))
                {
                    return false;
                }
                if (!helpSources.Build(settings))
                {
                    return false;
                }

                var binFiles = new BinaryFileCollection();
                binFiles.FindFiles(Path.Combine(releaseVerb.RootPath, "bin", "Release"));

                sampleFiles = new SampleModuleSourceFileCollection(modules.AllProjects);
                sampleFiles.FindFiles(Path.Combine(releaseVerb.RootPath, "src"));
                sampleFiles.Analyze(settings);
                sampleFiles.Prepare(settings);

                var helpFiles = new FileByExtensionCollection(null, true, ".chm");
                helpFiles.FindFiles(Path.Combine(releaseVerb.RootPath, "doc"));

                var requiredReleaseArchiveKinds = settings.ConfigFile.ReleaseArchiveKinds.ToArray();
                CreateArchive(settings, "src", requiredReleaseArchiveKinds,
                    modules,
                    releaseNotes,
                    helpSources,
                    solutions,
                    keyFiles);
                CreateArchive(settings, "bin", requiredReleaseArchiveKinds,
                    releaseNotes,
                    binFiles,
                    sampleFiles);
                if (helpFiles.FileCount > 0)
                {
                    CreateArchive(settings, "help", requiredReleaseArchiveKinds,
                        helpFiles,
                        releaseNotes);
                }
            }
            finally
            {
                foreach (var fc in AllFileCollections)
                {
                    fc.TidyUpPreparationFiles(settings);
                }
            }

            return true;
        }

        private void CreateArchive(ApplicationSettings settings, string name, IEnumerable<ArchiveKind> kinds, params FileCollection[] includedFiles)
        {
            var releaseVerb = (CommandLineReleaseSettingsBase)settings.CommandLine.ActiveVerb;

            foreach (var kind in kinds)
            {
                // TODO: include project name in release file name
                string path = Path.Combine(releaseVerb.ReleasePath, name + "-" + releaseVerb.ReleaseVersion.ToString() + kind.GetFileExtension());
                Build.ArchiveCreator.PackArchive(settings, path, kind, includedFiles);
            }
        }
    }
}
