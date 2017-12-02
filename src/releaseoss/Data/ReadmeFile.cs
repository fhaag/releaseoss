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
    public class ReadmeFile : RelevantFile
    {
        public ReadmeFile(FileInfo file, string[] subDirectories) : base(file, subDirectories)
        {
        }

        public override void PrepareFile(ApplicationSettings settings)
        {
            var releaseVerb = settings.CommandLine.ActiveVerb as CommandLineReleaseSettingsBase;
            if (releaseVerb == null)
            {
                throw new InvalidOperationException("The application is not being executed with a release-related command.");
            }

            base.PrepareFile(settings);

            string contents;
            using (var r = new StreamReader(new FileStream(File.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                contents = r.ReadToEnd();
            }

            var parts = contents.Split(settings.ConfigFile.ReadmeKeywordDelimiter);
            var modifiedContents = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                bool isKeyword;
                switch (parts[i])
                {
                    case "VERSION":
                        isKeyword = true;
                        modifiedContents.Append(releaseVerb.ReleaseVersion);
                        break;
                    case "DATE":
                        isKeyword = true;
                        modifiedContents.Append(DateTime.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case "DATETIME":
                        isKeyword = true;
                        modifiedContents.Append(DateTime.UtcNow.ToString("yyyy-MM-dd mm:hh:ss", System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    default:
                        isKeyword = false;
                        modifiedContents.Append(parts[i]);
                        break;
                }

                if (isKeyword)
                {
                    if (i + 1 < parts.Length)
                    {
                        i++;
                        modifiedContents.Append(parts[i]);
                    }
                }
            }

            System.IO.File.WriteAllText(EffectivePath(settings), modifiedContents.ToString());
        }

        public override string EffectivePath(ApplicationSettings settings)
        {
            var releaseVerb = settings.CommandLine.ActiveVerb as CommandLineReleaseSettingsBase;
            if (releaseVerb == null)
            {
                throw new InvalidOperationException("The application is not being executed with a release-related command.");
            }

            return Path.Combine(releaseVerb.TemporaryPath, File.Name);
        }
    }
}
