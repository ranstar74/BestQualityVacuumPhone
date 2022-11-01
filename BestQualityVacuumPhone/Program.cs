using BestQualityVacuumPhone;
using System.Text;
using System.Web;

Console.OutputEncoding = Encoding.Unicode;

// Configuration

int steps = 50; // Number of translation times
int batch = 35; // Number of lines per request (more - faster, but can be blocked by google)

string oxtPath = @"C:\\Users\\falco\\Desktop\\global.oxt.txt";
string outPath = @"C:\\Users\\falco\\Desktop\\lost.oxt.txt";

// End

List<string> oxtLines = File
    .ReadAllLines(oxtPath)
    .Skip(2)
    .TakeWhile(x => x != "}")
    .ToList();

static void WriteOn(int x, int y, string text)
{
    Console.SetCursorPosition(y, x);
    Console.WriteLine(new string(' ', 100));
    Console.SetCursorPosition(y, x);
    Console.Write(text);
}

Dictionary<string, string> translation = new();

List<string> keys = new();
List<string> values = new();

int index = 0;
int batchIndex = 0;
foreach (string oxtLine in oxtLines)
{
    index++;
    string[] parts = oxtLine.Split('=', StringSplitOptions.TrimEntries);

    string key = parts[0];
    string value = parts[1];

    // Packing batch
    keys.Add(key);
    values.Add(value);
    if (index != oxtLines.Count && keys.Count < batch)
    {
        continue;
    }

    // Sending batch
    string batchText = string.Join("\r\n", values);
    batchText = HttpUtility.UrlEncode(batchText);
    VacuumResult result = await VacuumTranslator.Translate(
        batchText,
        "English",
        "French",
        steps: steps,
        onProgress: (step, from, to, text) =>
        {
            WriteOn(batchIndex, 0, $"[{batchIndex}] - {(double)step / steps:0%} {step}/{steps} {from}-{to}");
        },
        onError: (attemptsLeft) =>
        {
            WriteOn(batchIndex, 0, $"[{batchIndex}] - Connection failed. Attempts left: {attemptsLeft}");
        });

    // Unpacking batch
    string[] results = result.Text.Split("\r\n");
    for (int i = 0; i < results.Length; i++)
    {
        WriteOn(batchIndex, 0, $"[{batchIndex}] - {results[i]}");
        translation[keys[i]] = results[i];
        batchIndex++;
    }

    // Cleaning up batch
    keys.Clear();
    values.Clear();
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