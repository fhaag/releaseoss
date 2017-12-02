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

using CommandLine;

using Semver;

namespace ReleaseOss
{
    public abstract class CommandLineReleaseSettingsBase : CommandLineSettingsBase
    {
        #region release version
        private SemVersion releaseVersion;

        public SemVersion ReleaseVersion => releaseVersion;

        [ValueOption(0)]
        public string ReleaseVersionAsString
        {
            get
            {
                return releaseVersion.ToString();
            }
            set
            {
                SemVersion.TryParse(value, out releaseVersion);
            }
        }

        public bool IsPrerelease => (releaseVersion.Major <= 0) || !string.IsNullOrEmpty(releaseVersion.Prerelease);
        #endregion

        #region config path
        private string configPath;

        [Option('f', "configfile", HelpText = "A path to a configuration file. By default, the file ossrelease.json in the current directory will be used.")]
        public string ProjectConfigPath
        {
            get
            {
                if (configPath == null)
                {
                    return Path.Combine(Environment.CurrentDirectory, "ossrelease.json");
                }
                else
                {
                    return Path.Combine(Environment.CurrentDirectory, configPath);
                }
            }
            set
            {
                configPath = value;
            }
        }
        #endregion

        #region folder
        private string rootPath;

        [Option('d', "directory", HelpText = "The path to the directory that serves as the root for the project data. By default, this will be assumed to be the same as the configuration file directory.")]
        public string RootPath
        {
            get
            {
                if (rootPath == null)
                {
                    return Path.GetDirectoryName(ProjectConfigPath);
                }
                else
                {
                    return rootPath;
                }
            }
            set
            {
                rootPath = value;
            }
        }

        public string ReleasePath
        {
            get
            {
                return Path.Combine(RootPath, "release");
            }
        }

        public string TemporaryPath
        {
            get
            {
                return Path.Combine(RootPath, "tmp");
            }
        }
        #endregion
    }
}
