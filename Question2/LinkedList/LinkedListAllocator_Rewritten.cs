using System;
using System.Collections.Generic;

namespace AllocationAlgorithms.LinkedList
{
    /*
     * Rewritten version by the me(based on the AI-generated draft).
     *
     * I kept the core idea the same, but I rewrote the structure and comments
     * in my own style so it’s clear I understand what is happening.
     *
     * Linked-list allocation idea:
     * - Blocks do NOT have to be consecutive.
     * - Each allocated block stores a pointer to the next block of the same file.
     * - Free space is also managed as a linked list (free list).
     */
    public class LinkedListAllocatorRewritten
    {
        // I use one "nextPointer" array to represent the "next" link between blocks.
        // Depending on the state, it can represent:
        // - the free list chain, or
        // - a file's block chain.
        private readonly int[] nextPointer;

       
        private int freeListHead;

        
        private readonly bool[] isAllocated;

        public LinkedListAllocatorRewritten(int totalBlocks)
        {
            if (totalBlocks <= 0)
                throw new ArgumentException("totalBlocks must be greater than 0.");

            nextPointer = new int[totalBlocks];
            isAllocated = new bool[totalBlocks];

            // I set up the free list like: 0 -> 1 -> 2 -> ... -> End
            freeListHead = 0;

            for (int i = 0; i < totalBlocks - 1; i++)
                nextPointer[i] = i + 1;

            nextPointer[totalBlocks - 1] = -1; // End of free list
        }

        /*
         * AllocateBlocks(size):
         * - Takes 'size' free blocks from the free list.
         * - Links them together as a file chain.
         * - Returns the head of the file chain.
         *
         * Important detail:
         * - In linked-list allocation, blocks can be anywhere on disk.
         * - We just connect them using pointers.
         */
        public int AllocateBlocks(int size)
        {
            if (size <= 0)
                return -1;

            int fileHead = -1;
            int previousBlock = -1;

            for (int i = 0; i < size; i++)
            {
                
                if (freeListHead == -1)
                {
                    FreeFile(fileHead);
                    return -1;
                }

                
                int currentBlock = freeListHead;
                freeListHead = nextPointer[freeListHead];

               
                isAllocated[currentBlock] = true;

                
                nextPointer[currentBlock] = -1;

               
                if (fileHead == -1)
                    fileHead = currentBlock;

                
                if (previousBlock != -1)
                    nextPointer[previousBlock] = currentBlock;

                previousBlock = currentBlock;
            }

            return fileHead;
        }

        /*
         * FreeFile(head):
         * - This is the "delete file" simulation.
         * - I start from the head pointer (same idea as reading a file),
         *   follow the pointer chain, and return each block back to the free list.
         *
         * After freeing, the blocks become reusable for future allocations.
         */
        public void FreeFile(int head)
        {
            int current = head;

            while (current != -1)
            {
                int blockToFree = current;
                current = nextPointer[current];

                isAllocated[blockToFree] = false;

                
                nextPointer[blockToFree] = freeListHead;
                freeListHead = blockToFree;
            }
        }

        /*
         * This helper prints the chain in a readable form:
         * Example: "Block 2 -> Block 5 -> Block 9 -> End"
         *
         * I used this because it makes the allocation trace easy to understand
         * without needing a debugger.
         */
        public string GetFileChainString(int head)
        {
            if (head == -1)
                return "Empty";

            List<string> parts = new List<string>();
            int current = head;

            while (current != -1)
            {
                parts.Add("Block " + current);
                current = nextPointer[current];
            }

            return string.Join(" -> ", parts) + " -> End";
        }

      
        public string GetDiskUsageString()
        {
            int[] map = new int[isAllocated.Length];
            for (int i = 0; i < isAllocated.Length; i++)
                map[i] = isAllocated[i] ? 1 : 0;

            return string.Join(" ", map);
        }
    }
}
