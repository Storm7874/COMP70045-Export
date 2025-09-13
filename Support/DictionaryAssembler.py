#Dictionary Assembler Program
#Supplied as part of the MSc Dissertation Project - LoRa Emergency Messaging System
# Matthew Boyce - b018567j@student.staffs.ac.uk

import csv, json, os

##Dictionary configuration parameters
MaxDictSize = 65536
InputFile = 'unigram_freq.csv'  ##Add wordlist filename here
OutputDir = ''                  ##Add output dir here

##Load words from file

wordList = []
with open(InputFile, 'r') as csvfile:
    reader = csv.reader(csvfile)
    # Iterate over lines in file
    for row in reader:
        # Check to see if row exists
        if row:
            # Add word from row into wordlist
            wordList.append(row[0])

shelf = []
# Iterate through wordList, one full dictionary at a time
for i in range(0,len(wordList), MaxDictSize):
    # Extract MaxDictSize chunk from wordList and add to shelf
    chunk = wordlist[i:i+MaxDictSize]
    shelf.append(chunk)

# Make output dir
os.makedirs(OutputDirectory, exist_ok=True)

# Iterate through shelf and dump each dictionary to JSON file
for i, dictionary in enumerate(shelf):
    out_path= os.path.join(OutputDirectory, f"dict{i:02d}.json")
    with open(out_path, 'w', encoding="utf-8") as f:
        json.dump(dictionary, f, ensure_ascii=False, indent=2)
