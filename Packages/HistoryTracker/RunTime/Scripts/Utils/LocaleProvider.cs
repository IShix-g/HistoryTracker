
using System.Globalization;
using UnityEngine;

namespace HistoryTracker
{
    public sealed class LocaleProvider
    {
        public static CultureInfo Culture { get; private set; }

        public static void Initialize()
        {
            if (Culture != null)
            {
                return;
            }
            var cultureName = GetCultureName();
            Culture = new CultureInfo(cultureName);
        }

        static string GetCultureName()
             => Application.systemLanguage switch
             {
                 SystemLanguage.Afrikaans => "af-ZA",
                 SystemLanguage.Arabic => "ar-SA",
                 SystemLanguage.Basque => "eu-ES",
                 SystemLanguage.Belarusian => "be-BY",
                 SystemLanguage.Bulgarian => "bg-BG",
                 SystemLanguage.Catalan => "ca-ES",
                 SystemLanguage.Chinese => "zh-CN",
                 SystemLanguage.ChineseSimplified => "zh-CN",
                 SystemLanguage.ChineseTraditional => "zh-TW",
                 SystemLanguage.Czech => "cs-CZ",
                 SystemLanguage.Danish => "da-DK",
                 SystemLanguage.Dutch => "nl-NL",
                 SystemLanguage.English => "en-US",
                 SystemLanguage.Estonian => "et-EE",
                 SystemLanguage.Faroese => "fo-FO",
                 SystemLanguage.Finnish => "fi-FI",
                 SystemLanguage.French => "fr-FR",
                 SystemLanguage.German => "de-DE",
                 SystemLanguage.Greek => "el-GR",
                 SystemLanguage.Hebrew => "he-IL",
                 SystemLanguage.Hungarian => "hu-HU",
                 SystemLanguage.Icelandic => "is-IS",
                 SystemLanguage.Indonesian => "id-ID",
                 SystemLanguage.Italian => "it-IT",
                 SystemLanguage.Japanese => "ja-JP",
                 SystemLanguage.Korean => "ko-KR",
                 SystemLanguage.Latvian => "lv-LV",
                 SystemLanguage.Lithuanian => "lt-LT",
                 SystemLanguage.Norwegian => "no-NO",
                 SystemLanguage.Polish => "pl-PL",
                 SystemLanguage.Portuguese => "pt-PT",
                 SystemLanguage.Romanian => "ro-RO",
                 SystemLanguage.Russian => "ru-RU",
                 SystemLanguage.SerboCroatian => "sr-Latn-RS",
                 SystemLanguage.Slovak => "sk-SK",
                 SystemLanguage.Slovenian => "sl-SI",
                 SystemLanguage.Spanish => "es-ES",
                 SystemLanguage.Swedish => "sv-SE",
                 SystemLanguage.Thai => "th-TH",
                 SystemLanguage.Turkish => "tr-TR",
                 SystemLanguage.Ukrainian => "uk-UA",
                 SystemLanguage.Vietnamese => "vi-VN",
                 _ => "en-US"
             };
    }
}
