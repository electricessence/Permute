using Permute;
using Open.Text.CSV;
using System.Text.RegularExpressions;

Regex FilePattern = new(@"\d+\)\s*(.+)(\.txt)", RegexOptions.Compiled);

var header = new List<string>();

var values = new List<List<string>>();

// Should get them in order.
foreach (var valueFile in Directory.GetFiles("./Values").Select(s => new FileInfo(s)))
{
	header.Add(FilePattern.Match(valueFile.Name).Groups[1].Value);
	using var reader = valueFile.OpenText();
	string? line;
	var v = new List<string>();
	values.Add(v);

	while((line = await reader.ReadLineAsync()) is not null)
	{
		if(!string.IsNullOrWhiteSpace(line))
			v.Add(line);
	}
}

using var outStream = File.Create("Permutations.csv");
using var sw = new StreamWriter(outStream);
using var writer = new CsvWriter(sw); 
writer.WriteRow(header);

foreach(var row in values.RowConfigurations())
{
	writer.WriteRow(row.Take(header.Count)); // .Take() to trim the buffer
}