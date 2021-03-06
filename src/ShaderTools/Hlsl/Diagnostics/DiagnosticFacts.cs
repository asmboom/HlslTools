﻿using ShaderTools.Core.Diagnostics;

namespace ShaderTools.Hlsl.Diagnostics
{
    internal static class DiagnosticFacts
    {
        public static DiagnosticSeverity GetSeverity(DiagnosticId id)
        {
            switch (id)
            {
                case DiagnosticId.LoopControlVariableConflict:
                case DiagnosticId.ImplicitTruncation:
                    return DiagnosticSeverity.Warning;
                default:
                    return DiagnosticSeverity.Error;
            }
        }
    }
}