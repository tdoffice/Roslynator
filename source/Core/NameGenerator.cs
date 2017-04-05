// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Roslynator.Extensions;
using Roslynator.Helpers;

namespace Roslynator
{
    public static class NameGenerator
    {
        private static StringComparer OrdinalComparer { get; } = StringComparer.Ordinal;

        public static string EnsureUniqueMemberName(
            string baseName,
            SemanticModel semanticModel,
            int position,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            INamedTypeSymbol containingType = semanticModel.GetEnclosingNamedType(position, cancellationToken);

            if (containingType != null)
            {
                return EnsureUniqueMemberName(baseName, containingType);
            }
            else
            {
                return EnsureUniqueName(baseName, semanticModel.LookupSymbols(position));
            }
        }

        public static string EnsureUniqueMemberName(
            string baseName,
            INamedTypeSymbol containingType)
        {
            if (containingType == null)
                throw new ArgumentNullException(nameof(containingType));

            return EnsureUniqueName(baseName, containingType.GetMembers());
        }

        public static string EnsureUniqueLocalName(
            string baseName,
            SemanticModel semanticModel,
            int position,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            ImmutableArray<ISymbol> symbols = semanticModel
                .GetSymbolsDeclaredInEnclosingSymbol(position, excludeAnonymousTypeProperty: true, cancellationToken: cancellationToken)
                .AddRange(semanticModel.LookupSymbols(position));

            return EnsureUniqueName(baseName, symbols);
        }

        internal static async Task<string> EnsureUniqueAsyncMethodNameAsync(
            string baseName,
            IMethodSymbol methodSymbol,
            Solution solution,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (methodSymbol == null)
                throw new ArgumentNullException(nameof(methodSymbol));

            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            HashSet<string> reservedNames = await GetReservedNamesAsync(methodSymbol, solution, cancellationToken).ConfigureAwait(false);

            var generator = new AsyncMethodNameGenerator();
            return generator.EnsureUniqueName(baseName, reservedNames);
        }

        public static async Task<string> EnsureUniqueMemberNameAsync(
            string baseName,
            ISymbol memberSymbol,
            Solution solution,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (memberSymbol == null)
                throw new ArgumentNullException(nameof(memberSymbol));

            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            HashSet<string> reservedNames = await GetReservedNamesAsync(memberSymbol, solution, cancellationToken).ConfigureAwait(false);

            return EnsureUniqueName(baseName, reservedNames);
        }

        public static string EnsureUniqueEnumMemberName(string baseName, INamedTypeSymbol enumSymbol)
        {
            if (enumSymbol == null)
                throw new ArgumentNullException(nameof(enumSymbol));

            return EnsureUniqueName(baseName, enumSymbol.MemberNames);
        }

        public static bool IsUniqueMemberName(
            string name,
            SemanticModel semanticModel,
            int position,
            bool isCaseSensitive = true,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            INamedTypeSymbol containingType = semanticModel.GetEnclosingNamedType(position, cancellationToken);

            return IsUniqueMemberName(name, containingType, isCaseSensitive);
        }

        public static bool IsUniqueMemberName(
            string name,
            INamedTypeSymbol containingType,
            bool isCaseSensitive = true)
        {
            if (containingType == null)
                throw new ArgumentNullException(nameof(containingType));

            return IsUniqueName(name, containingType.GetMembers(), GetStringComparison(isCaseSensitive));
        }

        public static async Task<bool> IsUniqueMemberNameAsync(
            string name,
            ISymbol memberSymbol,
            Solution solution,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (memberSymbol == null)
                throw new ArgumentNullException(nameof(memberSymbol));

            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            HashSet<string> reservedNames = await GetReservedNamesAsync(memberSymbol, solution, cancellationToken).ConfigureAwait(false);

            return !reservedNames.Contains(name);
        }

        private static async Task<HashSet<string>> GetReservedNamesAsync(
            ISymbol memberSymbol,
            Solution solution,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            HashSet<string> reservedNames = GetMemberNames(memberSymbol);

            foreach (ReferencedSymbol referencedSymbol in await SymbolFinder.FindReferencesAsync(memberSymbol, solution, cancellationToken).ConfigureAwait(false))
            {
                foreach (ReferenceLocation referenceLocation in referencedSymbol.Locations)
                {
                    if (!referenceLocation.IsImplicit
                        && !referenceLocation.IsCandidateLocation)
                    {
                        SemanticModel semanticModel = await referenceLocation.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                        foreach (ISymbol symbol in semanticModel.LookupSymbols(referenceLocation.Location.SourceSpan.Start))
                        {
                            if (!memberSymbol.Equals(symbol))
                                reservedNames.Add(symbol.Name);
                        }
                    }
                }
            }

            return reservedNames;
        }

        private static HashSet<string> GetMemberNames(ISymbol memberSymbol)
        {
            INamedTypeSymbol containingType = memberSymbol.ContainingType;

            Debug.Assert(containingType != null);

            if (containingType != null)
            {
                IEnumerable<string> memberNames = containingType
                    .GetMembers()
                    .Where(f => !memberSymbol.Equals(f))
                    .Select(f => f.Name);

                return new HashSet<string>(memberNames, OrdinalComparer);
            }
            else
            {
                return new HashSet<string>(OrdinalComparer);
            }
        }

        public static string EnsureUniqueName(string baseName, ImmutableArray<ISymbol> symbols, bool isCaseSensitive = true)
        {
            return EnsureUniqueName(baseName, symbols, GetStringComparison(isCaseSensitive));
        }

        public static string EnsureUniqueName(string baseName, IEnumerable<string> reservedNames, bool isCaseSensitive = true)
        {
            return EnsureUniqueName(baseName, reservedNames, GetStringComparison(isCaseSensitive));
        }

        private static string EnsureUniqueName(string baseName, IList<ISymbol> symbols, StringComparison stringComparison)
        {
            int suffix = 2;

            string name = baseName;

            while (!IsUniqueName(name, symbols, stringComparison))
            {
                name = baseName + suffix.ToString();
                suffix++;
            }

            return name;
        }

        public static string EnsureUniqueName(string baseName, IEnumerable<string> reservedNames, StringComparison stringComparison)
        {
            if (reservedNames == null)
                throw new ArgumentNullException(nameof(reservedNames));

            int suffix = 2;

            string name = baseName;

            while (!IsUniqueName(name, reservedNames, stringComparison))
            {
                name = baseName + suffix.ToString();
                suffix++;
            }

            return name;
        }

        private static bool IsUniqueName(string name, IList<ISymbol> symbols, StringComparison stringComparison)
        {
            return !symbols.Any(symbol => string.Equals(symbol.Name, name, stringComparison));
        }

        private static bool IsUniqueName(string name, IEnumerable<string> reservedNames, StringComparison stringComparison)
        {
            return !reservedNames.Any(f => string.Equals(f, name, stringComparison));
        }

        public static string CreateName(ITypeSymbol typeSymbol, bool firstCharToLower = false)
        {
            return CreateNameFromTypeSymbolHelper.CreateName(typeSymbol, firstCharToLower);
        }

        private static StringComparison GetStringComparison(bool isCaseSensitive)
        {
            if (isCaseSensitive)
            {
                return StringComparison.Ordinal;
            }
            else
            {
                return StringComparison.OrdinalIgnoreCase;
            }
        }
    }
}
