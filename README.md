![Social Preview Header, Showing the Logo and User Interface](https://repository-images.githubusercontent.com/1206363130/2c1b92a6-849d-4b70-85f1-388265b9df5a)

mutateBinary is a desktop application that converts files into DNA sequences, applies configurable mutation logic, and converts the mutated sequences back into binary files.
The project combines a practical file-processing workflow with biological mutation concepts in a way that is easy to explore and experiment with. 

## **How it works**
- Select a folder with input files.
- Configure mutation probabilities.
- Start mutation processing.

The app creates two directories, one for the DNA string and one for the mutated results. If you leave the mutation parameters at their default, the app will convert all data into a DNA sequence, which you could synthesize into DNA.
```
root/
├── dna/
└── mutated/
```

## **Requirements:**
.NET SDK

#### Additional Info
- The parameters are saved into the mutated files name so that it includes a way to document your methodology.
- For mutating image files for glitch-art purposes very small point mutation are recommended.
- The app ueses filestream system, which means that a 20GB file is read byte by byte instead of being crammed into your RAM at once.
- Things I'd like to do next are catalogued [https://github.com/lemonspurple/mutateBinary/issues](here), e.g. multithread support.
- It works non-destructive, meaning, that you convert a .dna file back to its original file, as long as not mutation has been applied.
