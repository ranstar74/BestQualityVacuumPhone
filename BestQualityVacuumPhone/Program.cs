using BestQualityVacuumPhone;
using System.Text;

Console.OutputEncoding = Encoding.Unicode;

string text = "When enabled player doesn't have to collect trash/plutonium. You must still refuel the Time Machine, however!";

VacuumResult result = await VacuumTranslator.Translate(text, "English", "English", 50);
Console.WriteLine(result.Text);