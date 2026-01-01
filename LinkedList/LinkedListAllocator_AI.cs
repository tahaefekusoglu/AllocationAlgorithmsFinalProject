using System;
using System.Collections.Generic;

namespace AllocationAlgorithms.LinkedList
{
    
    public class LinkedListAllocator
    {
        private readonly int[] next;     
        private int freeHead;            
        private readonly bool[] used;   

        public LinkedListAllocator(int numberOfBlocks)
        {
            if (numberOfBlocks <= 0) throw new ArgumentException("numberOfBlocks must be > 0");

            next = new int[numberOfBlocks];
            used = new bool[numberOfBlocks];

        
            freeHead = 0;
            for (int i = 0; i < numberOfBlocks - 1; i++)
                next[i] = i + 1;

            next[numberOfBlocks - 1] = -1;
        }

       
        public int Allocate(int size)
        {
            if (size <= 0) return -1;

            int head = -1;
            int prev = -1;

            for (int i = 0; i < size; i++)
            {
                if (freeHead == -1)
                {
                    
                    Free(head);
                    return -1;
                }

                int block = freeHead;
                freeHead = next[freeHead];

                used[block] = true;
                next[block] = -1;

                if (head == -1) head = block;
                if (prev != -1) next[prev] = block;

                prev = block;
            }

            return head;
        }

        
        public void Free(int head)
        {
            int current = head;

            while (current != -1)
            {
                int toFree = current;
                current = next[current];

                used[toFree] = false;

                
                next[toFree] = freeHead;
                freeHead = toFree;
            }
        }

        public string GetChainString(int head)
        {
            if (head == -1) return "Empty";

            List<int> blocks = new List<int>();
            int current = head;
            while (current != -1)
            {
                blocks.Add(current);
                current = next[current];
            }

            return string.Join(" -> ", blocks) + " -> End";
        }

        public string GetUsedMapString()
        {
            // 1 = used, 0 = free
            int[] map = new int[used.Length];
            for (int i = 0; i < used.Length; i++)
                map[i] = used[i] ? 1 : 0;

            return string.Join(" ", map);
        }
    }
}
