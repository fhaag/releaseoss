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
    public class RelevantFile
    {
        public RelevantFile(FileInfo file, string[] subDirectories)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            if (subDirectories == null)
            {
                throw new ArgumentNullException(nameof(subDirectories));
            }

            this.file = file;
            this.subDirectories = subDirectories;
        }

        private readonly FileInfo file;

        protected FileInfo File => file;

        private readonly string[] subDirectories;

        public IEnumerable<string> SubDirectories => subDirectories;

        public string LogicalName
        {
            get
            {
                return string.Join("", subDirectories.Select(d => d + "/").ToArray()) + file.Name;
            }
        }

        public virtual void AnalyzeFile(ApplicationSettings settings)
        {
            OutputHelper.WriteLine(OutputKind.Debug, "Analyzing file {0}:", LogicalName);
        }

        public virtual void PrepareFile(ApplicationSettings settings)
        {
            OutputHelper.WriteLine(OutputKind.Debug, "Preparing file {0}:", LogicalName);

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
        }

        public virtual void TidyUpPreparationFiles(ApplicationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
        }

        public virtual string EffectivePath(ApplicationSettings settings)
        {
            return file.FullName;
        }
    }
}
