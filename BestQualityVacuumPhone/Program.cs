using BestQualityVacuumPhone;
using System.Text;

Console.OutputEncoding = Encoding.Unicode;

string text = "If enabled you will experience turbulence while hovering in bad weather.";

VacuumResult result = await VacuumTranslator.Translate(text, "English", "English", 100);
Console.WriteLine(result.Text);