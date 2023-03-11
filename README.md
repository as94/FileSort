# File Sorter

This program sorts large files with strings of the form '{number}. {word}', for example

> 415\. Apple\
30432\. Something something something\
1\. Apple\
32\. Cherry is the best\
2\. Banana is yellow

To generate a new file, run the program with arguments
> generate 1600000

where 'generate' is the command and '1600000' is the number of lines in the file.

To sort the file, run the program with arguments
> sort 1600000.txt

where 'sort' is the command and '1600000.txt' is the path to the sorted file.

Sort order by word and then by number, sometimes words are repeated.

While the program is running, a 'Temp' folder is created with intermediate files. This folder will be deleted at the end of the program.
