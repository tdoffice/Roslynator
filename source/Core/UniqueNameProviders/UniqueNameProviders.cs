// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator
{
    internal static class UniqueNameProviders
    {
        public static UniqueNameProvider AsyncMethod { get; } = new AsyncMethodUniqueNameProvider();
        public static UniqueNameProvider NumberSuffix { get; } = new NumberSuffixUniqueNameProvider();
        public static UniqueNameProvider UnderscoreSuffix { get; } = new UnderscoreSuffixUniqueNameProvider();
    }
}
