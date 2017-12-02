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
    public sealed class ProjectOutputInfo
    {
        public ProjectOutputInfo(string projectId, string projectName, string assemblyName, string[] targetFrameworks)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }
            if (projectName == null)
            {
                throw new ArgumentNullException(nameof(projectName));
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException(nameof(assemblyName));
            }
            if (targetFrameworks == null)
            {
                throw new ArgumentNullException(nameof(targetFrameworks));
            }

            this.projectId = projectId;
            this.projectName = projectName;
            this.assemblyName = assemblyName;
            this.targetFrameworks = targetFrameworks;
        }

        private readonly string projectId;

        public string ProjectId => projectId;

        private readonly string projectName;

        public string ProjectName => projectName;

        private readonly string assemblyName;

        public string AssemblyName => assemblyName;

        private readonly string[] targetFrameworks;

        public IReadOnlyList<string> TargetFrameworks => targetFrameworks;
    }
}
