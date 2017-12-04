namespace ReamQuery.Services
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using NLog;

    // overrides the internal naming service used by scaffolding:
    // Microsoft.EntityFrameworkCore.Relational.Design\Internal\CandidateNamingService.cs
    public class EntityNamingService : CandidateNamingService
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public override string GenerateCandidateIdentifier(string originalIdentifier)
        {
            var isValid = SyntaxFacts.IsValidIdentifier(originalIdentifier);
            Logger.Debug("originalIdentifier {0}, isValid {1}", originalIdentifier, isValid);
            if (isValid)
            {
                return originalIdentifier;
            }
            return base.GenerateCandidateIdentifier(originalIdentifier);
        }
    }
}
