# Memory Allocation Algorithms – Final Project

This repository contains the implementation and experimental evaluation of classic
memory allocation algorithms using linked lists and bitmaps.

The project was prepared as part of the final assignment and includes both
AI-generated baseline implementations and rewritten, manually modified versions
to demonstrate understanding of the algorithms.

---

## 📁 Project Structure

AllocationAlgorithms/
│
├── Bitmap/
│ ├── BitmapAllocator_AI.cs
│ └── BitmapAllocator_Rewritten.cs
│
├── LinkedList/
│ ├── LinkedListAllocator_AI.cs
│ └── LinkedListAllocator_Rewritten.cs
│
├── results/
│ ├── part_b_output.txt
│ ├── part_c_output.txt
│ ├── part_d_output.txt
│ └── part_e_output.txt
│
├── Program.cs
├── AllocationAlgorithms.csproj
└── README.md



---

## 🧠 Implemented Algorithms

The following memory allocation strategies are implemented and tested:

- Best Fit  
- Worst Fit  
- Next Fit  

Each algorithm operates on a linked list of free memory segments of the form:
[start, length].

Bitmap-based allocation is also included for comparison (Question 2).

---

## 📄 Important Note About Program.cs

Program.cs intentionally contains code for multiple questions and experiments
inside the same file.

Each part (Question 2, Question 3 Part B/C/D/E) is clearly separated using
large comment blocks.

Only **one experiment is active at a time**.
Other parts are commented out so the instructor can easily inspect or run
any section if needed.

To run a specific part:
- Comment out other sections
- Leave only the desired section active
- Run the program normally

---

## ▶️ How to Run the Project

### Requirements
- .NET SDK (8.0 or later)

### Commands
```bash
dotnet clean
dotnet build
dotnet run
🧪 Experiments (Question 3)
Part B – Implementation
Best Fit, Worst Fit, Next Fit allocation

Free with adjacent block merging

Fully commented to explain allocation decisions

Part C – Allocation Trace
Fixed allocation/free sequence

Full free list printed after each step

Behavioral differences clearly visible

Part D – Fragmentation Test
12 random allocations (size 3–12)

Free exactly 4 allocated blocks

Attempt large allocation of size 25

Demonstrates external fragmentation

Part E – Speed Test
200 iterations of allocate + free

Execution time measured

Performance comparison based on list scanning behavior

📊 Results Folder
The results directory contains console outputs used directly in the report:

part_b_output.txt

part_c_output.txt

part_d_output.txt

part_e_output.txt

These outputs allow verification without rerunning the code.

🔁 Reproducibility
Reproducibility is ensured by:

Fixed random seeds

Clear directory structure

No external dependencies

Simple build and run steps

Anyone cloning this repository can reproduce the results easily.

🔗 GitHub Repository Link
👉 https://github.com/tahaefekusoglu/Operating-Systems-and-Hardware--Memory-Allocation-Algorithms-Final-Project


