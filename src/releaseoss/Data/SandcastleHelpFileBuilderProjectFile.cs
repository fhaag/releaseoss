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
using System.Xml;

namespace ReleaseOss.Data
{
    public sealed class SandcastleHelpFileBuilderProjectFile : MsBuildBasedFile
    {
        public SandcastleHelpFileBuilderProjectFile(FileInfo file, string[] subDirectories) : base(file, subDirectories)
        {
        }

        private string BackupFilePath
        {
            get
            {
                return File.FullName + ".bak";
            }
        }

        public override void PrepareFile(ApplicationSettings settings)
        {
            base.PrepareFile(settings);

            File.CopyTo(BackupFilePath);

            var doc = new XmlDocument();
            doc.Load(File.FullName);

            if (doc.DocumentElement.LocalName != "Project")
            {
                throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Unsupported file format; root node was {0}.",
                    doc.DocumentElement.LocalName));
            }

            var nsMgr = CreateNamespaceManager(doc);

            var ver = settings.CommandLine.BuildVerb.ReleaseVersion;

            // help file version
            WriteMetaDataElement(doc, nsMgr, "HelpFileVersion",
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0}.{1}.{2}.0",
                    ver.Major, ver.Minor, ver.Patch));

            // preliminary marking
            WriteMetaDataElement(doc, nsMgr, "Preliminary", settings.CommandLine.BuildVerb.IsPrerelease ? "True" : "False");

            doc.Save(File.FullName);
        }

        public override void TidyUpPreparationFiles(ApplicationSettings settings)
        {
            base.TidyUpPreparationFiles(settings);

            File.Delete();
            System.IO.File.Move(BackupFilePath, File.FullName);
        }
    }
}
