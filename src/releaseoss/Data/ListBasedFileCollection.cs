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

namespace ReleaseOss.Data
{
    public abstract class ListBasedFileCollection : FileCollection
    {
        private readonly List<RelevantFile> files = new List<RelevantFile>();

        protected List<RelevantFile> Files => files;

        public int FileCount => files.Count;

        public override void Analyze(ApplicationSettings settings)
        {
            foreach (var f in files)
            {
                f.AnalyzeFile(settings);
            }
        }

        public override void Prepare(ApplicationSettings settings)
        {
            foreach (var f in files)
            {
                f.PrepareFile(settings);
            }
        }

        public override void TidyUpPreparationFiles(ApplicationSettings settings)
        {
            foreach (var f in files)
            {
                f.TidyUpPreparationFiles(settings);
            }
        }

        public override void ProvideFiles(ApplicationSettings settings, Build.FileEntryCallback addFile)
        {
            foreach (var f in files)
            {
                addFile(f.EffectivePath(settings), LogicalNamePrefix + f.LogicalName);
            }
        }
    }
}
