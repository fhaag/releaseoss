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

namespace ReleaseOss
{
    class Program
    {
        static void Main(string[] args)
        {
            OutputHelper.WriteLine(OutputKind.Debug, "Debugging output enabled.");

            try
            {
                try
                {
                    var settings = new ApplicationSettings();
                    if (!CommandLine.Parser.Default.ParseArguments(args, settings.CommandLine, (v, o) => { }))
                    {
                        OutputHelper.WriteLine(OutputKind.Problem, "Options not recognized.");
                        OutputHelper.WriteLine(OutputKind.Help, CommandLine.Text.HelpText.AutoBuild(settings.CommandLine));
                        Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
                    }

                    settings.LoadSettingsIfRequired();

                    if (settings.CommandLine.BuildVerb != null)
                    {
                        var pj = new Data.Project();
                        pj.FindContents(settings);
                        pj.AnalyzeContents(settings);
                        pj.PrepareContents(settings);
                        return;
                    }

                    if (settings.CommandLine.PublishVerb != null)
                    {
                        var pj = new Data.Project();
                        pj.FindContents(settings);

                        return;
                    }

                    if (settings.CommandLine.SetupVerb != null)
                    {
                        Setup.FileCreator.CreateFiles(settings);

                        return;
                    }

                    OutputHelper.WriteLine(OutputKind.Problem, "No action specified.");
                }
                finally
                {
                    OutputHelper.WriteMessageStats();
                }
            }
            catch (Exception ex)
            {
                OutputHelper.WriteLine(OutputKind.Failure, ex.ToString());
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }
        }
    }
}
