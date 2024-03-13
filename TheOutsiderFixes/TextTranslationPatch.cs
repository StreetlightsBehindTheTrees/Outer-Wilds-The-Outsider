using HarmonyLib;
using OWML.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using TheOutsider;
using UnityEngine;

namespace TheOutsiderFixes
{
    [HarmonyPatch]
    public static class TextTranslationPatch
    {
        static bool _no_postfix_translate;
        static Dictionary<string, string> _color_table;
        static Dictionary<string, List<string>> _color_table_for_duplicate;
        static Dictionary<string, string> _disc_table;
        static string _bramble_power_station;
        static bool _english;
        static Font _japanese_dynamic_font;
        static Font _japanese_font;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation.SetLanguage))]
        public static void TextTranslation_SetLanguage_Postfix(ref TextTranslation.Language lang, TextTranslation __instance)
        {
            _color_table = null;
            _color_table_for_duplicate = null;
            _disc_table = null;
            _bramble_power_station = null;
            _english = false;
            _japanese_dynamic_font = null;
            _japanese_font = null;

            if (lang == TextTranslation.Language.ENGLISH)
            {
                Log.Print($"the language is English, so not translated.");
                _english = true;
                return;
            }
            if (lang == TextTranslation.Language.JAPANESE)
            {
                _japanese_dynamic_font = TextTranslation.GetFont(true);
                if (_japanese_dynamic_font == null)
                {
                    Log.Print($"failed to get japanese dynamic font.");
                }
                else
                {
                    Log.Print($"get japanese dynamic font.");
                }
                _japanese_font = TextTranslation.GetFont(false);
                if (_japanese_font == null)
                {
                    Log.Print($"failed to get japanese font.");
                }
                else
                {
                    Log.Print($"get japanese font.");
                }
            }

            var path = ModMain.Instance.ModHelper.Manifest.ModFolderPath + $"assets/{lang.GetName().ToLower()}.xml";
            ModMain.Instance.ModHelper.Console.WriteLine($"path to xml file: {path}");
            if (!File.Exists(path))
            {
                ModMain.Instance.ModHelper.Console.WriteLine($"this xml file is not found, so not translated.");
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(ReadAndRemoveByteOrderMarkFromPath(path));
            var translationTableNode = xmlDoc.SelectSingleNode("TranslationTable_XML");

            foreach (XmlNode node in translationTableNode.SelectNodes("entry"))
            {
                var key = node.SelectSingleNode("key").InnerText;
                var value = node.SelectSingleNode("value").InnerText;

                __instance.m_table.theTable[key] = value;
                //TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"key: {key}, value: {value}");

                if (_bramble_power_station == null && key == "Bramble Power Station")
                {
                    _bramble_power_station = value;
                }
            }

            foreach (XmlNode node in translationTableNode.SelectNodes("table_shipLog"))
            {
                var key = node.SelectSingleNode("key").InnerText;
                var value = node.SelectSingleNode("value").InnerText;

                __instance.m_table.theShipLogTable[key] = value;
                //TranslationForTheOutsider.Instance.ModHelper.Console.WriteLine($"key: {key}, value: {value}");
            }

            _color_table = new Dictionary<string, string>();
            foreach (XmlNode node in translationTableNode.SelectNodes("color_table"))
            {
                var key = node.SelectSingleNode("key").InnerText;
                var value = node.SelectSingleNode("value").InnerText;

                if (_color_table.ContainsKey(key))
                {
                    if (_color_table_for_duplicate == null)
                    {
                        _color_table_for_duplicate = new Dictionary<string, List<string>>();
                    }
                    if (!_color_table_for_duplicate.ContainsKey(key))
                    {
                        _color_table_for_duplicate[key] = new List<string>();
                    }
                    _color_table_for_duplicate[key].Add(value);
                }
                else
                {
                    _color_table[key] = value;
                }
            }

            _disc_table = new Dictionary<string, string>();
            foreach (XmlNode node in translationTableNode.SelectNodes("disc_table"))
            {
                var key = node.SelectSingleNode("key").InnerText;
                var value = node.SelectSingleNode("value").InnerText;
                _disc_table[key] = value;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation._Translate))]
        public static void TextTranslation_Translate_Prefix(string key, TextTranslation __instance)
        {
            _no_postfix_translate = key.Contains("_"); // See https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/17149bad3786f9aa68aed9eaf8ec94e62ee5ba7e/TheOutsider/OuterWildsHandling/OWPatches.cs#L119

            if (__instance.m_table == null || _english)
            {
                return;
            }
            var text = __instance.m_table.Get(key);
            if (text == null)
            {
                Log.Print($"key not contained in m_table: {key}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation._Translate))]
        public static void TextTranslation_Translate_Postfix(string key, TextTranslation __instance, ref string __result)
        {
            if (_no_postfix_translate)
            {
                return;
            }

            if (_color_table == null)
            {
                return;
            }

            var text = __result;

            var english_key_color_value_table = new Dictionary<string, string>() {
                {"Eye of the Universe", "lightblue"},
                {"Eye", "lightblue"},
                {"Brittle Hollow", "lightblue"},
                {"Ember Twin", "lightblue"},
                {"Dark Bramble", "lightblue"},
                {"Vessel", "lightblue"},
                {"Datura", "lightblue"},
                {"DATURA", "lightblue"},
                {"Friend", "lime"},
                {"FRIEND", "lime"},

                {"Solanum", "orange"},
                {"SOLANUM", "orange"},
                {"Filix", "cyan"},
                {"FILIX", "cyan"},
                {"Bells", "magenta"},
                {"BELLS", "magenta"},
                {"Yarrow", "yellow"},
                {"YARROW", "yellow"},
            };

            foreach (var key_value in english_key_color_value_table)
            {
                if (!_color_table.ContainsKey(key_value.Key))
                {
                    continue;
                }
                var translated_key = _color_table[key_value.Key];
                text = text.Replace(translated_key, $"<color={key_value.Value}>{translated_key.Replace("`", "")}</color>");

                if (_color_table_for_duplicate != null && _color_table_for_duplicate.ContainsKey(key_value.Key))
                {
                    foreach (var duplicated_translated_key in _color_table_for_duplicate[key_value.Key])
                    {
                        text = text.Replace(duplicated_translated_key, $"<color={key_value.Value}>{duplicated_translated_key.Replace("`", "")}</color>");
                    }
                }
            }

            if (text.Contains("#####"))
            { // this code is from https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/17149bad3786f9aa68aed9eaf8ec94e62ee5ba7e/TheOutsider/OuterWildsHandling/OWPatches.cs#L136
                int errorLength = Random.Range(5, 11);
                string hash = "";
                for (int i = 0; i < errorLength; i++) hash += '#';

                text = text.Replace("#####", $"[<color=red>{hash}</color>]");

                string l = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!?";
                string newText = "";
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c == '#') newText += l[Random.Range(0, l.Length)];
                    else newText += c;
                }

                text = newText;
            }

            __result = text;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation._Translate_ShipLog))]
        public static bool TextTranslation_Translate_ShipLog_Prefix(string key, TextTranslation __instance)
        {
            if (__instance.m_table.GetShipLog(key) == null && !_english)
            {
                Log.Print($"key not contained in m_table(ship): {key}");
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NomaiConversationStone), nameof(NomaiConversationStone.GetDisplayName))]
        public static void NomaiConversationStone_GetDisplayName_Postfix(NomaiConversationStone __instance, ref string __result)
        {
            // See https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/17149bad3786f9aa68aed9eaf8ec94e62ee5ba7e/TheOutsider/OuterWildsHandling/OWPatches.cs#L703-L722
            if (Locator.GetDreamWorldController().IsInDream() && _disc_table != null)
            {
                var english_array = new string[] { "'Explain' Disc", "'Prisoner' Disc", "'Friend' Disc", "'Me' Disc", "'Datura' Disc" };
                foreach (var english_key in english_array)
                {
                    if (__result == english_key)
                    {
                        if (!_disc_table.ContainsKey(english_key))
                        {
                            break;
                        }
                        __result = _disc_table[english_key];
                        break;
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogAstroObject), nameof(ShipLogAstroObject.GetName))]
        public static void ShipLogAstroObject_GetName(ShipLogAstroObject __instance, ref string __result)
        {
            if (__instance._id == "POWER_STATION" && _bramble_power_station != null)
            {
                __result = _bramble_power_station;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogFactListItem), nameof(ShipLogFactListItem.UpdateTextReveal))]
        public static void ShipLogFactListItem_UpdateTextReveal_Postfix(ShipLogFactListItem __instance)
        {
            if (_japanese_dynamic_font == null || _japanese_font == null)
            {
                return;
            }

            if (__instance._text.text.Contains("<i>"))
            {
                __instance._text.font = _japanese_dynamic_font;
                //TranslationForTheOutsider.Instance.Log($"font is changed to dynamic font in: {__instance._text.text}");
            }
            else
            {
                __instance._text.font = _japanese_font;
            }
            //TranslationForTheOutsider.Instance.Log($"ship log text in update text reveal: {__instance._text.text}");
        }

        //// this code change the font of titles of each card in ship log
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(FontAndLanguageController), nameof(FontAndLanguageController.InitializeFont))]
        //public static void FontAndLanguageController_InitializeFont_Postfix(FontAndLanguageController __instance) {
        //    if(__instance.name != "ShipLogFontAndLanguageController") {
        //        return;
        //    }
        //    if(_japanese_dynamic_font == null) {
        //        TranslationForTheOutsider.Instance.Log("oh, null.");
        //        return;
        //    }
        //    if(__instance._textContainerList.Count < 2) {
        //        TranslationForTheOutsider.Instance.Log("count of text container is less than 2.");
        //        return;
        //    }

        //    int countOfConvertedToDynamic = 0;
        //    for(int i = 90; i < __instance._textContainerList.Count; ++i) {
        //        var container = __instance._textContainerList[i];
        //        container.originalFont = _japanese_dynamic_font;
        //        __instance._textContainerList[i] = container;
        //        ++countOfConvertedToDynamic;
        //    }

        //    TranslationForTheOutsider.Instance.Log($"set dynamic font in japanese {countOfConvertedToDynamic} ship log.");

        //    //var textItemList = __instance._textItemList[0];
        //    //__instance._textContainerList.Add(new FontAndLanguageController.TextContainer {
        //    //    textElement = 
        //    //})
        //}


        public static string ReadAndRemoveByteOrderMarkFromPath(string path)
        {
            // this code is from https://github.com/xen-42/outer-wilds-localization-utility/blob/6cf4eb784c06237820d318b4ce22ac30da4acac1/LocalizationUtility/Patches/TextTranslationPatches.cs#L198-L209
            byte[] bytes = File.ReadAllBytes(path);
            byte[] preamble1 = Encoding.UTF8.GetPreamble();
            byte[] preamble2 = Encoding.Unicode.GetPreamble();
            byte[] preamble3 = Encoding.BigEndianUnicode.GetPreamble();
            if (bytes.StartsWith(preamble1))
                return Encoding.UTF8.GetString(bytes, preamble1.Length, bytes.Length - preamble1.Length);
            if (bytes.StartsWith(preamble2))
                return Encoding.Unicode.GetString(bytes, preamble2.Length, bytes.Length - preamble2.Length);
            return bytes.StartsWith(preamble3) ? Encoding.BigEndianUnicode.GetString(bytes, preamble3.Length, bytes.Length - preamble3.Length) : Encoding.UTF8.GetString(bytes);
        }
    }


}
