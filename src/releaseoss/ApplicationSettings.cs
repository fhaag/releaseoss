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
using System.IO;

namespace ReleaseOss
{
    public sealed class ApplicationSettings
    {
        private readonly GlobalCommandLineSettings commandLine = new GlobalCommandLineSettings();

        public GlobalCommandLineSettings CommandLine => commandLine;

        private readonly ConfigFile.ProjectConfig configFile = new ConfigFile.ProjectConfig();

        public ConfigFile.ProjectConfig ConfigFile => configFile;

        private readonly ConfigFile.AppConfig appConfigFile = new ConfigFile.AppConfig();

        public ConfigFile.AppConfig AppConfigFile => appConfigFile;

        public static string AppConfigDirectoryPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".releaseoss");
            }
        }

        public static string AppConfigFilePath
        {
            get
            {
                return Path.Combine(AppConfigDirectoryPath, "appConfig.json");
            }
        }

        public void LoadSettingsIfRequired()
        {
            string appConfigJson;
            try
            {
                appConfigJson = File.ReadAllText(AppConfigFilePath);
            }
            catch
            {
                appConfigJson = "{}";
            }
            Newtonsoft.Json.JsonConvert.PopulateObject(appConfigJson, appConfigFile);

            var verbWithConfigFile = commandLine.ActiveVerb as CommandLineReleaseSettingsBase;
            if (verbWithConfigFile != null)
            {
                string configJson;
                try
                {
                    configJson = File.ReadAllText(verbWithConfigFile.ProjectConfigPath);
                }

                catch
                {
                    configJson = "{}";
                }
                Newtonsoft.Json.JsonConvert.PopulateObject(configJson, configFile);
            }
        }

        public string MsBuildPath
        {
            get
            {
                return appConfigFile.MsBuildPath;
            }
        }
    }
}
