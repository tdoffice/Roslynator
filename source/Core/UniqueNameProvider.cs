// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    //TODO: přejmenovat UniqueNameProvider
    public abstract class UniqueNameProvider
    {
        public abstract string EnsureUniqueName(string baseName, HashSet<string> reservedNames);
        public abstract string EnsureUniqueName(string baseName, ImmutableArray<ISymbol> symbols, bool isCaseSensitive);
    }
}
