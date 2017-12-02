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
using System.ComponentModel;
using System.Linq;

namespace ReleaseOss
{
    public static class OutputHelper
    {
        private static readonly Dictionary<OutputKind, int> messageCount = new Dictionary<OutputKind, int>();

        public static void Write(OutputKind kind, string format, params object[] args)
        {
#if !DEBUG
            if (kind == OutputKind.Debug) {
                return;
            }
#endif

            var bgColor = Console.BackgroundColor;
            var fgColor = Console.ForegroundColor;

            switch (kind)
            {
                case OutputKind.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case OutputKind.Help:
                case OutputKind.External:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case OutputKind.Failure:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case OutputKind.Problem:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case OutputKind.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                default:
                    throw new InvalidEnumArgumentException("kind", (int)kind, typeof(OutputKind));
            }
            Console.BackgroundColor = ConsoleColor.Black;
            try
            {
                Console.Write("[" + kind.ToString() + "] " + string.Format(System.Globalization.CultureInfo.InvariantCulture, format, args));
            }
            finally
            {
                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = fgColor;
            }

            int msgCount;
            if (messageCount.TryGetValue(kind, out msgCount))
            {
                messageCount[kind] = msgCount + 1;
            }
            else
            {
                messageCount[kind] = 1;
            }
        }

        public static void WriteLine(OutputKind kind, string format, params object[] args)
        {
            Write(kind, format + Environment.NewLine, args);
        }

        public static void WriteMessageStats()
        {
            if (messageCount.Count > 0)
            {
                var pairs = messageCount.ToArray();
                int totalMessageCount = pairs.Select(p => p.Value).Sum();
                WriteLine(OutputKind.Info, "Messages by type (total: {0}):", totalMessageCount);
                foreach (var pair in pairs)
                {
                    WriteLine(OutputKind.Info, "{0}: {1}", pair.Key, pair.Value);
                }
            }
            else
            {
                WriteLine(OutputKind.Info, "No messages.");
            }
        }
    }
}
