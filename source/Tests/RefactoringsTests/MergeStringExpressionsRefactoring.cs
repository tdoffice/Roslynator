﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class MergeStringExpressionsRefactoring
    {
        public static string Foo()
        {
            string s = "a" + "b" + "c" + "d";

            s = "\"1\"" +
"\"2\"";

            return s;
        }

        public static string Foo2()
        {
            string x = null;

            x = "{\"}" + x + x + "{" + @"""" + "}" + Foo() + $@"{{""}}{Foo()}{{""}}";

            string s = x + "a" + x + "b";

            return s;
        }
    }
}
