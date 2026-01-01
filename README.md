# Allocation Algorithms – Final Project (C# Console)

This repository contains my final project implementation and experiments for **disk/memory allocation algorithms**.

It includes:

- **Question 2:** Bitmap vs Linked-List allocation  
  (AI version vs Rewritten version — 4 versions total)
- **Question 3:** Linked-list free-list allocator experiments  
  (**Best Fit, Worst Fit, Next Fit**)  
  Parts **B / C / D / E** are runnable from a menu.

---

##  How to Run (Instructor)

### Requirements
- .NET SDK (recommended: .NET 6/7/8)
- Run from the project folder that contains `AllocationAlgorithms.csproj`

### Run Command
```bash
dotnet run




1 – Question 2 (Bitmap vs LinkedList | AI vs Rewritten)

Runs all experiments for 4 versions:

Bitmap (AI)

Bitmap (Rewritten)

Linked-list (AI)

Linked-list (Rewritten)

Experiments included:

Speed Test (100 allocations)

Fragmentation Test (20 allocs, free 5, try allocate size 12)

Allocation Trace (fixed 15 allocations, prints disk state each step)

Runner file: Question2/Question2Runner.cs
Algorithms:

Question2/Bitmap/*

Question2/LinkedList/*

2 – Question 3 Part B (Implementation Demo)

Demonstrates the linked-list free-list allocator implementation:

AllocateBestFit

AllocateWorstFit

AllocateNextFit

Free + merge adjacent segments

Runner file: Question3/Runners/Part_B_Runner.cs

3 – Question 3 Part C (Experiment 1: Allocation Trace)

Runs a fixed request sequence:

[10, 5, 20, -5, 12, -10, 8, 6, 7, 3, 10]

After each request (allocate/free), prints the full free list for:

Best Fit

Worst Fit

Next Fit

Runner file: Question3/Runners/Part_C_Runner.cs

4 – Question 3 Part D (Experiment 2: Fragmentation Test)

Fragmentation experiment:

12 random allocations (size 3–12)

Free exactly 4 previous blocks (randomly selected)

Attempt one large allocation of size 25

Fixed seeds are used for reproducibility.

Runner file: Question3/Runners/Part_D_Runner.cs

5 – Question 3 Part E (Experiment 3: Speed Test)

Speed experiment:

Repeat 200 iterations:

allocate a random block (size 1–10)

free one previously allocated block

Measures total time for:

Best Fit

Worst Fit

Next Fit

Runner file: Question3/Runners/Part_E_Runner.cs




Project Structure


AllocationAlgorithms

Program.cs

Question2

Bitmap
BitmapAllocator_AI.cs
BitmapAllocator_Rewritten.cs

LinkedList
LinkedListAllocator_AI.cs
LinkedListAllocator_Rewritten.cs

Question3

Core
Allocator.cs
FreeSegment.cs
AllocatorBase.cs
BestFitAllocator.cs
WorstFitAllocator.cs
NextFitAllocator.cs

Runners
Part_B_Runner.cs
Part_C_Runner.cs
Part_D_Runner.cs
Part_E_Runner.cs
Question2Runner.cs





Notes for Evaluation
Reproducibility: Parts D and E use fixed random seeds so results are repeatable.

No bin/obj in repository: Build artifacts are ignored via .gitignore.



Troubleshooting

If you get build errors:

Ensure you are in the folder containing the .csproj

Run:

dotnet build
dotnet run