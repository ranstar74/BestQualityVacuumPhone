using BestQualityVacuumPhone;
using System.Text;

Console.OutputEncoding = Encoding.Unicode;

string text = "Allows you to turn off remote control. In order to remote control this Time Machine step out of the car and open the Interaction Menu.";
int steps = 75;

VacuumResult result = await VacuumTranslator.Translate(
    text,
    "English",
    "English",
    steps: steps,
    onProgress: (step, from, to, text) =>
    {
        Console.WriteLine(
            $"{step}/{steps} {from}-{to}");
    },
    onError: (attemptsLeft) =>
    {
        Console.WriteLine(
            $"Failed to connect server... Attempts left: {attemptsLeft}");
    });
Console.WriteLine(result.Text);