# SQLab

SQL A-B testing console application. Inspired by [Scientist][sci].


## Why

Say you want to replace one piece of SQL with another one, in order to refactor or optimize your code. Naturally, you need to know that the new code will return exactly the same results as the old code.


## Installation

Binaries may be provided in future. For now, `clone` and build with VS 2013+.

Set your connection string in `sqlab.exe.config`


## Usage

	sqlab File1.sql File2.sql

	
## Behaviour

The script will run both of the specified SQL files on the database and then write all results to a CSV format in memory. It can handle multiple batches of results, accumulating them into one set.

When finished, the CSV files are compared, first for the Count of results, then line-by-line.

If a difference is encountered, the program will still continue to compare the other rows.


## Output

This application is strongly designed for the command line. It uses [Windows standard return codes][codes] and outputs helpful messages which you can grep/find.


### Return codes

	0x00	Results are equal
	0x01	Results are different
	0x02 	File not found
	0xA0	Not enough arguments

	
### Screen text

You can grep/find the following codes to get error messages.

 * If there are differences, a line beginning with the word **Fail:** will be written, followed by an error message. A few lines will be written to show the nature of the difference.
 * If there are no differences, a line beginngin with **Success:** will be written.
 * When the script has finished, the word **Done** will be written.


## License

See [LICENSE.txt](./LICENSE.txt)


## About us

At [Village Software][vs], we design and develop enterprise-grade solutions. We care about accuracy and quality. Get in touch via our website if you'd like to talk to us about your computing needs!


[sci]: https://github.com/github/scientist
[codes]: https://msdn.microsoft.com/en-gb/library/ms681382.aspx
[vs]: http://villagesoftware.co.uk
