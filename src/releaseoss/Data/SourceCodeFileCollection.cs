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
    public sealed class SourceCodeFileCollection : ListBasedFileCollection
    {
        public IRelevantFileFactory FileFactory { get; set; }

        protected override object ScanFolder(DirectoryInfo folder, string[] subDirectories, object context)
        {
            switch (folder.Name)
            {
                case "bin":
                case "obj":
                    return null;
            }

            if (FileFactory == null)
            {
                throw new InvalidOperationException("No file factory set.");
            }

            foreach (var file in folder.EnumerateFiles())
            {
                if (!IsLocalSettingsFile(file))
                {
                    var f = FileFactory.CreateFile(file, subDirectories);
                    if (f != null)
                    {
                        Files.Add(f);
                    }
                }
            }
            return this;
        }

        public IEnumerable<ProjectOutputInfo> AllProjects => Files.OfType<ProjectFileBase>().Select(pf => pf.CreateOutputInfo());
    }
}
