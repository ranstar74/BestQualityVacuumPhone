using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace BestQualityVacuumPhone;

public struct VacuumResult
{
    public bool Success { get; }
    public string Text { get; }

    public VacuumResult(bool success, string text)
    {
        Success = success;
        Text = text;
    }
}

public delegate void VacuumProgressCallback(
    int step,
    string from,
    string to,
    string text);

public delegate void VacuumErrorCallback(
    int attemptsLeft);

public delegate void VacuumFailedCallback();

public static class VacuumTranslator
{
    private static readonly HttpClient _http = new();
    private const string _url = "}2{=q&t=td&}1{=lt&}0{=ls&xtg=tneilc?elgnis/a_etalsnart/moc.sipaelgoog.etalsnart//:sptth";
    private static readonly Random _rand = new();
    private static readonly string[] _languageCodes;

    static VacuumTranslator()
    {
        _languageCodes = LanguageCode.Values.ToArray();
    }

    public static async Task<VacuumResult> Translate(
        string text,
        string from,
        string to,
        int steps = 1,
        int numAttempts = 50,
        VacuumProgressCallback onProgress = null,
        VacuumErrorCallback onError = null,
        VacuumFailedCallback onFailed = null)
    {
        if (steps < 1)
            throw new ArgumentException("Number of translations must be 1 and more.");

        string url = string.Concat(_url.Reverse());

        from = LanguageCode[from];
        to = LanguageCode[to];

        int attemptsLeft = numAttempts;
        string translatedText = text;
        string fromLanguage = from;
        for (int i = 0; i < steps; i++)
        {
            string toLanguage;
            if (i + 1 == steps)
                toLanguage = to;
            else
                toLanguage = GetRandomLangCode();

            string requestUrl = string.Format(url,
                fromLanguage,
                toLanguage,
                translatedText);

            HttpResponseMessage responce = await _http.GetAsync(requestUrl);
            if (!responce.IsSuccessStatusCode)
            {
                onError?.Invoke(attemptsLeft);

                attemptsLeft--;
                i--;

                if (attemptsLeft == 0)
                {
                    onFailed?.Invoke();
                    return new VacuumResult(false, translatedText);
                }

                continue;
            }
            attemptsLeft = numAttempts;

            string jsonContent = await responce.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<object[]>(jsonContent);

            translatedText = string.Empty;
            foreach (var entry in (JArray)json[0])
            {
                translatedText += entry[0];
            }

            string fromLang = GetLanguageFromCode(fromLanguage);
            string toLang = GetLanguageFromCode(toLanguage);

            onProgress?.Invoke(i + 1, fromLang, toLang, translatedText);

            fromLanguage = toLanguage;
        }

        return new VacuumResult(true, translatedText);
    }

    private static string GetRandomLangCode()
    {
        return _languageCodes[_rand.Next(0, _languageCodes.Length)];
    }

    private static string GetLanguageFromCode(
        string code)
    {
        int index = LanguageCode.Values
            .ToList()
            .IndexOf(code);

        return LanguageCode
            .ElementAt(index)
            .Key;
    }

    public static readonly Dictionary<string, string> LanguageCode = new()
    {
        ["Afrikaans"] = "af",
        ["Albanian"] = "sq",
        ["Amharic"] = "am",
        ["Arabic"] = "ar",
        ["Armenian"] = "hy",
        ["Azerbaijani"] = "az",
        ["Basque"] = "eu",
        ["Belarusian"] = "be",
        ["Bengali"] = "bn",
        ["Bosnian"] = "bs",
        ["Bulgarian"] = "bg",
        ["Catalan"] = "ca",
        ["Cebuano"] = "ceb",
        ["Chinese (Simplified)"] = "zh",
        ["Chinese (Traditional)"] = "zh-TW",
        ["Corsican"] = "co",
        ["Croatian"] = "hr",
        ["Czech"] = "cs",
        ["Danish"] = "da",
        ["Dutch"] = "nl",
        ["English"] = "en",
        ["Esperanto"] = "eo",
        ["Estonian"] = "et",
        ["Ewe*"] = "ee",
        ["Filipino (Tagalog)"] = "fil",
        ["Finnish"] = "fi",
        ["French"] = "fr",
        ["Frisian"] = "fy",
        ["Galician"] = "gl",
        ["Georgian"] = "ka",
        ["German"] = "de",
        ["Greek"] = "el",
        ["Gujarati"] = "gu",
        ["Haitian Creole"] = "ht",
        ["Hausa"] = "ha",
        ["Hawaiian"] = "haw",
        ["Hebrew"] = "he",
        ["Hindi"] = "hi",
        ["Hmong"] = "hmn",
        ["Hungarian"] = "hu",
        ["Icelandic"] = "is",
        ["Igbo"] = "ig",
        ["Indonesian"] = "id",
        ["Irish"] = "ga",
        ["Italian"] = "it",
        ["Japanese"] = "ja",
        ["Javanese"] = "jv",
        ["Kazakh"] = "kk",
        ["Khmer"] = "km",
        ["Kinyarwanda"] = "rw",
        ["Korean"] = "ko",
        ["Krio*"] = "kri",
        ["Kurdish"] = "ku",
        ["Kurdish (Sorani)*"] = "ckb",
        ["Kyrgyz"] = "ky",
        ["Lao"] = "lo",
        ["Latin"] = "la",
        ["Latvian"] = "lv",
        ["Lithuanian"] = "lt",
        ["Luxembourgish"] = "lb",
        ["Macedonian"] = "mk",
        ["Malagasy"] = "mg",
        ["Malay"] = "ms",
        ["Malayalam"] = "ml",
        ["Maltese"] = "mt",
        ["Maori"] = "mi",
        ["Marathi"] = "mr",
        ["Meiteilon (Manipuri)*"] = "mni-Mtei",
        ["Mizo*"] = "lus",
        ["Mongolian"] = "mn",
        ["Myanmar (Burmese)"] = "my",
        ["Nepali"] = "ne",
        ["Norwegian"] = "no",
        ["Nyanja (Chichewa)"] = "ny",
        ["Odia (Oriya)"] = "or",
        ["Pashto"] = "ps",
        ["Persian"] = "fa",
        ["Polish"] = "pl",
        ["Portuguese (Portugal, Brazil)"] = "pt",
        ["Punjabi"] = "pa",
        ["Romanian"] = "ro",
        ["Russian"] = "ru",
        ["Samoan"] = "sm",
        ["Scots Gaelic"] = "gd",
        ["Serbian"] = "sr",
        ["Sesotho"] = "st",
        ["Shona"] = "sn",
        ["Sindhi"] = "sd",
        ["Sinhala (Sinhalese)"] = "si",
        ["Slovak"] = "sk",
        ["Slovenian"] = "sl",
        ["Somali"] = "so",
        ["Spanish"] = "es",
        ["Sundanese"] = "su",
        ["Swahili"] = "sw",
        ["Swedish"] = "sv",
        ["Tagalog (Filipino)"] = "tl",
        ["Tajik"] = "tg",
        ["Tamil"] = "ta",
        ["Tatar"] = "tt",
        ["Telugu"] = "te",
        ["Thai"] = "th",
        ["Tsonga*"] = "ts",
        ["Turkish"] = "tr",
        ["Turkmen"] = "tk",
        ["Ukrainian"] = "uk",
        ["Urdu"] = "ur",
        ["Uyghur"] = "ug",
        ["Uzbek"] = "uz",
        ["Vietnamese"] = "vi",
        ["Welsh"] = "cy",
        ["Xhosa"] = "xh",
        ["Yiddish"] = "yi",
        ["Yoruba"] = "yo",
        ["Zulu"] = "zu",
    };
}
