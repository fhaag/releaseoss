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

using Newtonsoft.Json;

namespace ReleaseOss.Setup
{
    /// <summary>
    /// Creates default settings files.
    /// </summary>
    public static class FileCreator
    {
        public static void CreateFiles(ApplicationSettings settings)
        {
            var ss = settings.CommandLine.SetupVerb;
            if (ss == null)
            {
                throw new InvalidOperationException("Application not launched with setup verb.");
            }

            if (ss.AppConfigFile)
            {
                CreateAppConfig(settings, ss);
            }

            if (ss.ProjectConfigFile)
            {
                CreateProjectConfig(settings, ss);
            }

            if (ss.ProjectKeyFile)
            {
                CreateProjectKeyFile(settings, ss);
            }

            if (ss.GitIgnoreSettings)
            {
                CreateGitIgnoreSettings(settings, ss);
            }
        }

        private static void CreateAppConfig(ApplicationSettings settings, CommandLineSetupSettings ss)
        {
            Directory.CreateDirectory(ApplicationSettings.AppConfigDirectoryPath);
            File.WriteAllText(ApplicationSettings.AppConfigFilePath, JsonConvert.SerializeObject(settings.AppConfigFile));
        }

        private static void CreateProjectConfig(ApplicationSettings settings, CommandLineSetupSettings ss)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ss.ProjectConfigPath));

            var pj = new ConfigFile.ProjectConfig();
            pj.Sites.Default = new ConfigFile.ProjectSiteConfig
            {
                ReleaseArchiveType = ArchiveKind.Zip
            };

            File.WriteAllText(ss.ProjectConfigPath, JsonConvert.SerializeObject(pj, Formatting.Indented));
        }

        private static void CreateProjectKeyFile(ApplicationSettings settings, CommandLineSetupSettings ss)
        {
            // TODO: keys file template
        }

        private static void CreateGitIgnoreSettings(ApplicationSettings settings, CommandLineSetupSettings ss)
        {
            var rootPath = ss.RootPath;
            Directory.CreateDirectory(rootPath);

            WriteGitIgnoreFile(rootPath,
                "**/bin/",
                "**/obj/",
                "doc/*.xml",
                "**/*.bak",
                "**/*.cache");

            if (Directory.Exists(Path.Combine(rootPath, "src")))
            {
                WriteGitIgnoreFile(Path.Combine(rootPath, "src"),
                    ".vs/",
                    "packages/*/");
            }
        }

        private static void WriteGitIgnoreFile(string dirPath, params string[] ignoredPatterns)
        {
            var patterns = new HashSet<string>();
            var fn = Path.Combine(dirPath, ".gitignore");

            try
            {
                if (File.Exists(fn))
                {
                    foreach (var line in File.ReadAllLines(fn))
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            patterns.Add(line);
                        }
                    }
                }

                foreach (var p in ignoredPatterns)
                {
                    patterns.Add(p);
                }

                File.WriteAllLines(fn, patterns);
            }
            catch
            {
            }
        }
    }
}
