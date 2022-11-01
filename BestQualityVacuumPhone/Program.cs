using BestQualityVacuumPhone;
using System.Text;

Console.OutputEncoding = Encoding.Unicode;

// Configuration

int steps = 5;
int batch = 20;

string oxtPath = @"C:\\Users\\falco\\Desktop\\global.oxt.txt";
string outPath = @"C:\\Users\\falco\\Desktop\\lost.oxt.txt";

// End

IEnumerable<string> oxtLines = File
    .ReadAllLines(oxtPath)
    .Skip(2)
    .TakeWhile(x => x != "}");

static void WriteOn(int x, int y, string text)
{
    Console.SetCursorPosition(y, x);
    Console.WriteLine(new string(' ', 100));
    Console.SetCursorPosition(y, x);
    Console.Write(text);
}

Dictionary<string, string> translation = new();

List<string> keys = new();
StringBuilder batchSb = new();

int index = 0;
foreach (string oxtLine in oxtLines)
{
    string[] parts = oxtLine.Split('=', StringSplitOptions.TrimEntries);

    string key = parts[0];
    string value = parts[1];

    // Packing batch
    if (keys.Count < batch)
    {
        batchSb.AppendLine(value);
        keys.Add(key);
        continue;
    }

    // Sending batch
    string batchText = batchSb.ToString();
    VacuumResult result = await VacuumTranslator.Translate(
        batchText,
        "English",
        "English",
        steps: steps,
        onProgress: (step, from, to, text) =>
        {
            WriteOn(index, 0, $"[{index}] - {((double)step / steps):0%} {step}/{steps} {from}-{to}");
        },
        onError: (attemptsLeft) =>
        {
            WriteOn(index, 0, $"[{index}] - Connection failed. Attempts left: {attemptsLeft}");
        });

    // Unpacking batch
    string[] results = result.Text.Split("\r\n");
    for (int i = 0; i < results.Length; i++)
    {
        WriteOn(index, 0, $"[{index}] - {results[i]}");
        translation[keys[i]] = results[i];
        index++;
    }

    // Cleaning up batch
    keys.Clear();
    batchSb.Clear();
}
Console.WriteLine($"\n\nWriting {outPath}");

StringBuilder sb = new();
sb.AppendLine("Version 2 30");
sb.AppendLine("{");
foreach (var entry in translation)
{
    sb.AppendLine($"\t{entry.Key} = {entry.Value}");
}
sb.AppendLine("}");

File.WriteAllText(outPath, sb.ToString());

Console.WriteLine("We hope you enjoyed Best Quality Vacuum Phone OXT Translation services!");