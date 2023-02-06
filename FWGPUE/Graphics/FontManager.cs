//#define MANUAL_FONT_LOAD

using FWGPUE.IO;
#if MANUAL_FONT_LOAD
using Pie.Freetype;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
#endif

using GL = Silk.NET.OpenGL.GL;

namespace FWGPUE.Graphics;

class FontManager {
#if MANUAL_FONT_LOAD
    public static readonly string PrintableAsciiCharacters = """ !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~""";
    public const int BetweenCharacterPadding = 2;
    public string LoadedCharacters { get; protected set; }
    public SpriteAtlasFile Atlas { get; protected set; }
    
    Shader Shader { get; }
    uint vao;
    uint vbo;

    float[] quadVertices = {
     // positions           uvs
        0f,  1f, 1.0f, 0f, 0f, // B
        0f, 0f, 1.0f, 0f, 1f, // A
         1f, 0f, 1.0f, 1f, 1f, // D
                                    
        0f,  1f, 1.0f, 0f, 0f, // B
         1f, 0f, 1.0f, 1f, 1f, // D
         1f,  1f, 1.0f, 1f, 0f, // C
    };

    #region font data

    public record CharacterData(int X, int Y, int Width, int Height, int BearingX, int BearingY, int Advance);
    public Dictionary
        <string, Dictionary //  font
        <int, Texture>> //      atlases by size 
        Fonts { get; } = new();

    public Dictionary
        <string, Dictionary //      font
        <int, Dictionary //         character locations by size
            <char, CharacterData>>> //   character rects by char
        CharacterLocations { get; } = new();
    public Dictionary<string, Dictionary<int, SpriteAtlasFile>> Atlases { get; } = new();

    void SetFontAtlas(string font, int size, SpriteAtlasFile atlas) {
        if (!Atlases.ContainsKey(font)) {
            Atlases[font] = new();
        }
        Atlases[font][size] = atlas;
    }
    public SpriteAtlasFile? GetFontAtlas(string font, int size) {
        if (Atlases.ContainsKey(font)) {
            if (Atlases[font].ContainsKey(size)) {
                return Atlases[font][size];
            }
            Log.Error($"no atlases for {size}/{font}");
            return null;
        }
        Log.Error($"no atlases for {font}");
        return null;
    }

    void SetFontTexture(string font, int size, Texture texture) {
        if (!Fonts.ContainsKey(font)) {
            Fonts[font] = new();
        }

        Fonts[font][size] = texture;
    }
    Texture? GetFontTexture(string font, int size) {
        if (Fonts.ContainsKey(font) && Fonts[font].ContainsKey(size)) {
            return Fonts[font][size];
        }
        return null;
    }

    void SetCharacterRect(string font, int size, char character, CharacterData rect) {
        if (!CharacterLocations.ContainsKey(font)) {
            CharacterLocations[font] = new();
        }
        if (!CharacterLocations[font].ContainsKey(size)) {
            CharacterLocations[font][size] = new();
        }

        CharacterLocations[font][size][character] = rect;
    }
    public CharacterData GetCharacterData(string font, int size, char character) {
        if (CharacterLocations.ContainsKey(font) &&
            CharacterLocations[font].ContainsKey(size) &&
            CharacterLocations[font][size].ContainsKey(character)) {
            return CharacterLocations[font][size][character];
        }
        return default;
    }

    void SetupFontAtlas(string font, int size) {
        if (!Atlases.ContainsKey(font)) {
            Atlases[font] = new Dictionary<int, SpriteAtlasFile>();
        }

        SpriteAtlasFile atlas = new() {
            Texture = GetFontTexture(font, size)
        };
        foreach (char c in LoadedCharacters) {
            CharacterData cd = CharacterLocations[font][size][c];
            atlas.SpriteDefinitions[$"{c}"] = new SpriteAtlasFile.SpriteRect(cd.X, cd.Y, cd.Advance, cd.Height + cd.BearingY);
        }
        SetFontAtlas(font, size, atlas);
    }
    #endregion font data
#endif

    public string? DefaultFont { get; protected set; }
    public FontFile? FontFile { get; protected set; }

    public byte[] GetFontData(string font) {
        return FontFile?.FontData[font].Data ?? Array.Empty<byte>();
    }

    public void LoadFont(GL gl, FontFile fontFile, params int[] sizes) {
        FontFile = fontFile;
        if (!fontFile.Loaded) {
            fontFile.Load();
        }

        // use default font if no default font is specified
        DefaultFont = fontFile.DefaultFont;

#if MANUAL_FONT_LOAD
        LoadFont(gl, fontFile, fontFile.DefaultFont, PrintableAsciiCharacters, sizes);
    }
    public void LoadFont(GL gl, FontFile fontFile, string font, string characters, params int[] sizes) {
        if (!fontFile.Loaded) {
            fontFile.Load();
        }

        try {
            using FreeType freeType = new();

            for (int i = 0; i < sizes.Length; i++) {
                // get font face
                using Face face = freeType.CreateFace(fontFile.Default.Data, sizes[i]);

                // calculate total size of the character atlas size
                int totalSize = characters.Length * (face.Size * BetweenCharacterPadding).Squared();
                int size = (int)MathF.Sqrt(totalSize);
                Log.Info($"font texture for {sizes[i]}/{font}: {size}*{size}");

                int width = size;
                int height = size;

                Texture fontTexture = new Texture(gl, width, height);

                // fill atlas with character data
                int currentX = 0;
                int currentY = 0;
                int maxHeight = 0;
                for (int character = 0; character < characters.Length; character++) {
                    // generate character bitmap
                    Character c = face.Characters[characters[character]];

                    Log.Info($"{characters[character]}: {c.Width}*{c.Height}, ->{c.Advance}, {c.BitmapLeft}/{c.BitmapTop}");

                    // if at the end of a row, move to the next row
                    if (currentX + c.Advance + BetweenCharacterPadding >= width) {
                        currentX = 0;
                        currentY += maxHeight + BetweenCharacterPadding;

                        if (currentY >= height) {
                            Log.Error("spritesheet too small");
                            return;
                        }
                    }

                    // copy character bitmap to atlas
                    unsafe {
                        fixed (byte* d = c.Bitmap) {
                            fontTexture.Bind();
                            gl.TexSubImage2D(GLEnum.Texture2D, 0, currentX, currentY, (uint)c.Width, (uint)c.Height, PixelFormat.Rgba, PixelType.UnsignedByte, d);
                        }
                    }

                    SetCharacterRect(font, sizes[i], characters[character],
                        new CharacterData(currentX, currentY,
                                     c.Width, c.Height,
                                     c.BitmapLeft, c.BitmapTop,
                                     c.Advance));

                    // advance position trackers
                    currentX += c.Advance + BetweenCharacterPadding;
                    if (maxHeight < c.Height + c.BitmapTop) {
                        maxHeight = c.Height + c.BitmapTop;
                    }
                }

                // successfully loaded all desired characters
                LoadedCharacters = characters;

                // create texture from bitmap
                SetFontTexture(font, sizes[i], fontTexture);
                SetupFontAtlas(font, sizes[i]);
            }
        }
        catch (Exception e) {
            Log.Error($"error initializing fonts: {e}");
        }
    }



        public void DrawText(SpriteBatcher batcher, float x, float y, string text, int size, float screenWidth, float screenHeight, float scale = 1) {
        DrawText(batcher, x, y, text, size, DefaultFont, screenWidth, screenHeight, scale);
    }
    public void DrawText(SpriteBatcher batcher, float x, float y, string text, int size, string font, float screenWidth, float screenHeight, float scale = 1) {
        if (!Fonts.ContainsKey(font)) {
            Log.Error($"no such font: {font}");
            return;
        }

        // find closest size to requested size
        int minDifference = int.MaxValue;
        int actualSize = 1;
        foreach (int s in Fonts[font].Keys) {
            int currentDifference = Math.Abs(s - size);
            if (currentDifference < minDifference) {
                minDifference = currentDifference;
                actualSize = s;
            }
        }
        scale *= (float)size / actualSize;

        for (int i = 0; i < text.Length; i++) {
            CharacterData cd = GetCharacterData(font, actualSize, text[i]);
            Sprite cSprite = new(GetFontAtlas(font, actualSize)!) {
                Texture = $"{text[i]}",
                Transform = {
                    Position = new(x - cd.BearingX * scale/(screenWidth/screenHeight), y - (cd.Height - cd.BearingY) * scale/(screenWidth/screenHeight), 1),
                    Scale = new(scale, scale, 1),
                }
            };
            batcher.DrawSprite(cSprite);
            x += cd.Advance * scale;
        }
#endif
    }
}
