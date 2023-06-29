using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FWGPUE.Graphics
{
    class TextManager {
        public const string DefaultFont = "default";

        public Dictionary<string, VectorFont> Fonts = new();

        public bool FontExists(string font) {
            return Fonts.ContainsKey(font);
        }

        /// <summary>
        /// Attempts to load the requested font from the default vectorfont location
        /// <para>( assets/vectorfonts/* )</para>
        /// </summary>
        public void LoadFont(string name) {
            VectorFont newFont = VectorFont.Load($"assets/vectorfonts/{name}");
            Fonts[name] = newFont;
        }

        public float GetAspectRatio(string font) {
            if (FontExists(font)) {
                return Fonts[font].AspectRatio;
            }
            return 1;
        }

        public IEnumerable<Vector2[]> GetTextPolygons(string font, string text) {
            if (FontExists(font)) {
                var currentFont = Fonts[font];

                float xOffset = 0;
                float yOffset = 0;
                foreach (char c in text) {
                    if (c == '\n') {
                        xOffset = 0;
                        yOffset += 1.1f;
                        continue;
                    }

                    if (currentFont.CharsToLetterDefinitions.ContainsKey(c)) {
                        foreach (var polygon in currentFont.CharsToLetterDefinitions[c].Polygons) {
                            yield return polygon.TransformAll(new Vector2(xOffset, yOffset)).ToArray();
                        }
                    }

                    xOffset += 1.1f;
                }
            }
        }
    }

    class VectorFont {
        public float AspectRatio = 1;
        public Dictionary<char, PolygonSet> CharsToLetterDefinitions = new();

        public static VectorFont Load(EngineFileLocation fontDirectory) {
            VectorFont result = new();

            DataMarkupFile fontInfo = new($"{fontDirectory.LocalPath}/!fontinfo.fwgm");
            fontInfo.Load();

            foreach (string s in fontInfo.GetToken("SupportedCharacters").Contents.Collection) {
                string casing = char.IsUpper(s[0]) ? "capital" : "lowercase";
                char c = char.ToLower(s[0]);

                PolygonSet character = new($"assets/vectorfonts/default/{casing}{c}.fwvf");
                character.Load();
                result.CharsToLetterDefinitions[s[0]] = character;
            }

            if(!float.TryParse(fontInfo.GetToken("AspectRatio").Contents.Value, out result.AspectRatio)){
                Log.Warn("fontinfo has no default aspect ratio");
            }

            return result;
        }
    }
}
