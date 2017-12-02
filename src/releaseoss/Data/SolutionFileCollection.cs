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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseOss.Data
{
    public sealed class SolutionFileCollection : ListBasedFileCollection
    {
        protected override object ScanFolder(DirectoryInfo folder, string[] subDirectories, object context)
        {
            foreach (var f in folder.EnumerateFiles("*.sln"))
            {
                Files.Add(new RelevantFile(f, new string[0]));
            }
            return null;
        }

        public bool Build(ApplicationSettings settings)
        {
            if (string.IsNullOrEmpty(settings.MsBuildPath))
            {
                throw new InvalidOperationException("MSBuild path not found.");
            }

            foreach (var sln in Files)
            {
                OutputHelper.WriteLine(OutputKind.Info, "Building solution from {0} ...", sln.EffectivePath(settings));

                var psi = new ProcessStartInfo(settings.MsBuildPath, "\"" + sln.EffectivePath(settings) + "\" /t:Rebuild /p:Configuration=Release")
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                var p = Process.Start(psi);
                OutputHelper.WriteLine(OutputKind.External, p.StandardOutput.ReadToEnd());
                p.WaitForExit();
                if (p.ExitCode == 0)
                {
                    OutputHelper.WriteLine(OutputKind.Info, "Process finished successfully.");
                }
                else
                {
                    OutputHelper.WriteLine(OutputKind.Problem, "The build process exited with code {0} for solution {1}.", p.ExitCode, sln.EffectivePath(settings));
                    return false;
                }
            }
            return true;
        }

        protected override string LogicalNamePrefix => "src/";
    }
}
