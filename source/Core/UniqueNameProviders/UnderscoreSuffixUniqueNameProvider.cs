// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal class UnderscoreSuffixUniqueNameProvider : UniqueNameProvider
    {
        public override string EnsureUniqueName(string baseName, HashSet<string> reservedNames)
        {
            string suffix = "";

            string name = baseName;

            while (!NameGenerator.IsUniqueName(name, reservedNames))
            {
                suffix += "_";
                name = baseName + suffix;
            }

            return name;
        }

        public override string EnsureUniqueName(string baseName, ImmutableArray<ISymbol> symbols, bool isCaseSensitive)
        {
            string suffix = "";

            string name = baseName;

            while (!NameGenerator.IsUniqueName(name, symbols, isCaseSensitive))
            {
                suffix += "_";
                name = baseName + suffix;
            }

            return name;
        }
    }
}
