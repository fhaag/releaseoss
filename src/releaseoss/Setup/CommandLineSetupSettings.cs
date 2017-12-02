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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;

namespace ReleaseOss.Setup
{
    public sealed class CommandLineSetupSettings : CommandLineReleaseSettingsBase
    {
        [Option('a', "appconfig", HelpText = "Creates skeletons for application-wide configuration files in the user's settings folder.")]
        public bool AppConfigFile { get; set; }

        [Option('p', "projectconfig", HelpText = "Creates a skeleton for a project configuration file.")]
        public bool ProjectConfigFile { get; set; }

        [Option('g', "gitignore", HelpText = "Adds default exclusions to .gitignore files.")]
        public bool GitIgnoreSettings { get; set; }

        [Option('k', "keyfile", HelpText = "Generates a template for a project-specific key file.")]
        public bool ProjectKeyFile { get; set; }
    }
}
