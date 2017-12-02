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
using ICSharpCode.SharpZipLib;

namespace ReleaseOss.Build
{
    public static class ArchiveCreator
    {
        public static void PackArchive(ApplicationSettings settings, string path, ArchiveKind kind, params Data.FileCollection[] includedFiles)
        {
            OutputHelper.WriteLine(OutputKind.Info, "Creating archive {0} ...", path);

            using (var fs = File.Create(path))
            {
                switch (kind)
                {
                    case ArchiveKind.Zip:
                        using (var zip = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(fs))
                        {
                            zip.UseZip64 = ICSharpCode.SharpZipLib.Zip.UseZip64.Off;
                            AddEntries(settings, (filePath, entryName) => {
                                var entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(entryName);
                                zip.PutNextEntry(entry);
                                try
                                {
                                    byte[] buffer = new byte[4096];
                                    using (var src = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(src, zip, buffer);
                                    }
                                }
                                finally
                                {
                                    zip.CloseEntry();
                                }
                            }, includedFiles);
                        }
                        break;
                    case ArchiveKind.TarGz:
                        using (var gz = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(fs))
                        {
                            using (var tar = new ICSharpCode.SharpZipLib.Tar.TarOutputStream(gz))
                            {
                                AddEntries(settings, (filePath, entryName) => {
                                    var tarHeader = new ICSharpCode.SharpZipLib.Tar.TarHeader
                                    {
                                        Name = entryName
                                    };
                                    var entry = new ICSharpCode.SharpZipLib.Tar.TarEntry(tarHeader);
                                    tar.PutNextEntry(entry);
                                    try
                                    {
                                        byte[] buffer = new byte[4096];
                                        using (var src = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                        {
                                            ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(src, tar, buffer);
                                        }
                                    }
                                    finally
                                    {
                                        tar.CloseEntry();
                                    }
                                }, includedFiles);
                            }
                        }
                        break;
                }
            }
        }

        private static void AddEntries(ApplicationSettings settings, FileEntryCallback addEntry, params Data.FileCollection[] includedFiles)
        {
            foreach (var fc in includedFiles)
            {
                fc.ProvideFiles(settings, addEntry);
            }
        }
    }
}
