﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1078, RCS1056, RCS1176

using System;
using s = System.String;

namespace Roslynator.CSharp.Analyzers.Test
{
    /// <summary>
    /// <see cref="String"/>
    /// <see cref="System.String"/>
    /// <see cref="global::System.String"/>
    /// <see cref="s"/>
    /// </summary>
    internal static class UsePredefinedType
    {
        public static void MethodName()
        {
            s value = s.Empty;

            String s1 = String.Empty;
            System.String s2 = System.String.Empty;
            global::System.String s3 = global::System.String.Empty;
        }
    }
}
