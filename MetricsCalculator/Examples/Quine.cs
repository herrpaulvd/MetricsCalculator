using System;
using System.IO;

namespace Quine
{
    static class Program
    {
        const string otherSource = "using System;\r\nusing System.IO;\r\n\r\nnamespace Quine\r\n{\r\n    static class Program\r\n    {\r\n        const string otherSource = \"!\";\r\n\r\n        static string Format(string x, string y)\r\n            => x.Replace(\"\\x21\", y);\r\n\r\n        static void Main(string[] args)\r\n        {\r\n            File.WriteAllText(\"output.txt\", Format(otherSource, otherSource.Replace(\"\\\\\", \"\\\\\\\\\").Replace(\"\\n\", \"\\\\n\").Replace(\"\\r\", \"\\\\r\").Replace(\"\\\"\", \"\\\\\\\"\")));\r\n        }\r\n    }\r\n}\r\n";

        static string Format(string x, string y)
            => x.Replace("\x21", y);

        static void Main(string[] args)
        {
            File.WriteAllText("output.txt", Format(otherSource, otherSource.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"")));
        }
    }
}
