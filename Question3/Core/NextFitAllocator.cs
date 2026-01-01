namespace AllocationAlgorithms.Question3.Core
{
    internal sealed class NextFitAllocator : AllocatorBase
    {
        private FreeSegment? cursor;

        public NextFitAllocator(int totalMemory) : base(totalMemory)
        {
            cursor = head;
        }

        public override int Allocate(int size)
        {
            if (size <= 0) return -1;
            if (head == null) return -1;

            if (cursor == null) cursor = head;

            // We'll scan starting from cursor, wrap once at most.
            FreeSegment? startPoint = cursor;
            FreeSegment? cur = cursor;
            FreeSegment? prev = FindPrev(cur);

            bool wrapped = false;

            while (cur != null)
            {
                if (cur.Length >= size)
                {
                    int start = AllocateFrom(prev, cur, size);

                    // IMPORTANT:
                    // After AllocateFrom, 'cur' might have been removed (Length==0 case).
                    // If removed, we must move cursor to the next segment (or head).
                    if (cur.Length > 0)
                    {
                        cursor = cur;
                    }
                    else
                    {
                        // cur is removed from list; prev.Next is the next node (or head if prev is null)
                        cursor = (prev == null) ? head : prev.Next;
                        if (cursor == null) cursor = head; // safety
                    }

                    return start;
                }

                
                if (cur.Next != null)
                {
                    prev = cur;
                    cur = cur.Next;
                }
                else
                {
                    // reached end -> wrap
                    if (wrapped) break;
                    wrapped = true;

                    prev = null;
                    cur = head;
                }

                // if we are back to the starting point after wrapping, stop
                if (wrapped && cur == startPoint) break;
            }

            return -1;
        }
    }
}
