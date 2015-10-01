﻿using System;
using HlslTools.Text;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.Text
{
    internal sealed class VisualStudioSourceText : SourceText
    {
        public VisualStudioSourceText(ITextSnapshot snapshot)
        {
            Snapshot = snapshot;
            Length = Snapshot.Length;
            Filename = null; // TODO: For now, the root file has its Filename set to null.
        }

        public override string GetText(TextSpan textSpan)
        {
            return Snapshot.GetText(textSpan.Start, textSpan.Length);
        }

        public ITextSnapshot Snapshot { get; }

        public override char this[int index] => Snapshot[index];

        public override int Length { get; }

        public override string Filename { get; }

        public override int GetLineNumberFromPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            return Snapshot.GetLineNumberFromPosition(position);
        }
    }
}