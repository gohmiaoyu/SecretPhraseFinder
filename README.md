# Secret Phrase Finder

This program was written to solve a backend developer challenge designed by a Danish software company.

The challenge consists of finding three different secret phrases, given an anagram, the MD5 hashes of the secret phrases, and a list of nearly 100,000 words that are allowed to be in the secret phrases.

On a typical desktop computer in 2017, given the following:

- The anagram "poultry outwit ants"
- The MD hashes "e4820b45d2277f3844eac66c903e84be", "23170acc097c24edb98fc5488ab033fe" and "665e5bcb0c20062fe8abaaf4628bb154"

the three secret phrases take ~0.1 seconds, ~0.05 seconds and ~2 seconds respectively to be found. (See the screenshot "Found Secret Phrases.PNG" in the folder "CodeChallenge".)