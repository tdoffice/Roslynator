// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp
{
    public class StatementContainerSlice : ListSlice<StatementSyntax>
    {
        private StatementContainerSlice(StatementContainer container, TextSpan span)
             : base(container.Statements, span)
        {
            Container = container;
        }

        public static StatementContainerSlice Create(BlockSyntax block, TextSpan span)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            var container = new StatementContainer(block);

            return new StatementContainerSlice(container, span);
        }

        public static StatementContainerSlice Create(SwitchSectionSyntax switchSection, TextSpan span)
        {
            if (switchSection == null)
                throw new ArgumentNullException(nameof(switchSection));

            var container = new StatementContainer(switchSection);

            return new StatementContainerSlice(container, span);
        }

        public StatementContainer Container { get; }
    }
}
