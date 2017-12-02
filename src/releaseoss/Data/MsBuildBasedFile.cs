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
    public abstract class MsBuildBasedFile : RelevantFile
    {
        protected MsBuildBasedFile(FileInfo file, string[] subDirectories) : base(file, subDirectories)
        {
        }

        protected static XmlNamespaceManager CreateNamespaceManager(XmlDocument document)
        {
            var result = new XmlNamespaceManager(document.NameTable);
            result.AddNamespace("msbuild", document.DocumentElement.NamespaceURI);
            OutputHelper.WriteLine(OutputKind.Debug, "Root namespace: " + document.DocumentElement.NamespaceURI);
            return result;
        }

        protected void WriteMetaDataElement(XmlDocument doc, XmlNamespaceManager nsMgr, string localElementName, string newValue)
        {
            var prefixedElementName = "msbuild:" + localElementName;
            var nodes = doc.SelectNodes("/msbuild:Project/msbuild:PropertyGroup/" + prefixedElementName, nsMgr);
            if (nodes.Count > 0)
            {
                foreach (var node in nodes.OfType<XmlElement>())
                {
                    node.InnerText = newValue;
                }
            }
            else
            {
                var msBuildUri = nsMgr.LookupNamespace("msbuild");
                var msBuildPrefix = doc.DocumentElement.Prefix;

                var parent = (XmlElement)doc.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup[child::msbuild:Title or child::msbuild:Description or child::msbuild:HelpTitle]", nsMgr);
                if (parent == null)
                {
                    parent = doc.CreateElement(msBuildPrefix, "PropertyGroup", msBuildUri);
                    doc.DocumentElement.AppendChild(parent);
                }

                var newChild = doc.CreateElement(msBuildPrefix, localElementName, msBuildUri);
                newChild.InnerText = newValue;
                parent.AppendChild(newChild);
            }
        }
    }
}
