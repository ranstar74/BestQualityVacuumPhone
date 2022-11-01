using BestQualityVacuumPhone;
using System.Collections.Concurrent;
using System.Text;
using System.Web;

Console.OutputEncoding = Encoding.Unicode;
Console.CursorVisible = false;

// Configuration

int numSteps = 50; // Number of translation times
int linesPerBatch = 5; // Number of lines per request (more - faster, but can be blocked by google)

string oxtPath = @"C:\\Users\\falco\\Desktop\\global.oxt.txt";
string outPath = @"C:\\Users\\falco\\Desktop\\lost.oxt.txt";

// End

DateTime startTime = DateTime.Now;

List<string> oxtLines = File
    .ReadAllLines(oxtPath)
    .Skip(2)
    .TakeWhile(x => x != "}")
    .ToList();

static void WriteOn(int x, int y, string text)
{
    Console.SetCursorPosition(x, y);
    Console.WriteLine(new string(' ', 100));
    Console.SetCursorPosition(x, y);
    Console.Write(text);
}

ConcurrentDictionary<int, Dictionary<string, string>> translation = new();
ConcurrentDictionary<int, string> batchStatus = new();
List<Task<bool>> translationTasks = new();

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
    if (index != oxtLines.Count && keys.Count < linesPerBatch)
    {
        continue;
    }

    // Cleaning up batch
    var batchKeys = new List<string>(keys);
    var batchValues = new List<string>(values);
    keys.Clear();
    values.Clear();

    int taskBatchIndex = batchIndex;
    batchIndex++;
    translationTasks.Add(Task.Run<bool>(async () =>
    {
        bool failed = false;

        // Sending batch
        string batchText = string.Join("\r\n", batchValues);
        batchText = HttpUtility.UrlEncode(batchText);
        VacuumResult result = await VacuumTranslator.Translate(
            batchText,
            "English",
            "English",
            steps: numSteps,
            onProgress: (step, from, to, text) =>
            {
                batchStatus[taskBatchIndex] =
                    $"[{taskBatchIndex}] - {(double)step / numSteps:0%} {step}/{numSteps} {from}-{to}";
            },
            onError: (attemptsLeft) =>
            {
                batchStatus[taskBatchIndex] =
                    $"[{taskBatchIndex}] - Connection failed. Attempts left: {attemptsLeft}";
            },
            onFailed: () =>
            {
                failed = true;
            });

        // Unpacking batch
        string[] results = result.Text.Split("\r\n");
        translation[taskBatchIndex] = new();
        for (int i = 0; i < results.Length; i++)
        {
            //WriteOn(taskBatchIndex, 0, $"[{taskBatchIndex}] - {results[i]}");
            translation[taskBatchIndex][batchKeys[i]] = results[i];
        }

        return !failed;
    }));
}

void printStatus()
{
    int numTasks = translationTasks.Count;
    int doneTasks = translationTasks.Count(x => x.IsCompleted);

    DateTime workTime = new((DateTime.Now - startTime).Ticks);
    WriteOn(0, 0, $"{doneTasks}/{numTasks}, It's been: {workTime:HH:mm:ss}.");

    foreach (var item in batchStatus)
    {
        var task = translationTasks[item.Key];

        //WriteOn(0, item.Key + 1, $"[{task.IsCompleted}]\t" + item.Value);
    }
};

// Awaiting tasks to be finished
while (!translationTasks.All(x => x.IsCompleted))
{
    printStatus();
    await Task.Delay(150);
}
printStatus();

Console.WriteLine($"\n\nWriting {outPath}");

StringBuilder sb = new();
sb.AppendLine("Version 2 30");
sb.AppendLine("{");
foreach (var batchEntryIndex in translation.Keys.OrderBy(x => x))
{
    foreach (var entry in translation[batchEntryIndex])
    {
        sb.AppendLine($"\t{entry.Key} = {entry.Value}");
    }
}
sb.AppendLine("}");

File.WriteAllText(outPath, sb.ToString());

Console.WriteLine("We hope you enjoyed Best Quality Vacuum Phone OXT Translation services!");