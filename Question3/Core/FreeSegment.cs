namespace AllocationAlgorithms.Question3.Core
{
    internal sealed class FreeSegment
    {
        public int Start;
        public int Length;
        public FreeSegment? Next;

        public FreeSegment(int start, int length)
        {
            Start = start;
            Length = length;
            Next = null;
        }

        public int EndExclusive => Start + Length;

        public override string ToString() => $"[{Start}, {Length}]";
    }
}
