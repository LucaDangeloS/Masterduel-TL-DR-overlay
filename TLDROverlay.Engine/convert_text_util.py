import os

# giving directory name
dirname = './'

# iterating over all files
for file in os.listdir(dirname):
    if file.endswith('Negations.txt') and (file.startswith('False') or file.startswith('True')):
        # i = 1
        end_text = ""
        #Load lines of file
        with open(file, 'r', encoding="utf-8") as f:
            lines = f.readlines()
        for line in lines:
            # end_text += str(i) + "  "
            line_split = line.split(".")
            for word in line_split:
                if "negate" in word.lower():
                    end_text += (word+".\n")
            # i += 1
        #Save lines of file
        with open(file.strip(".txt") + "_stripped.txt", 'w', encoding="utf-8") as f:
            f.writelines(end_text)
    else:
        continue