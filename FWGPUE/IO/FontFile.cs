using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWGPUE.IO;
class FontFile : DataMarkupFile {
    public string DefaultFont { get; protected set; }
    public ByteFile Default => FontData[DefaultFont];
    public string[] Fonts { get; protected set; }

    public Dictionary<string, ByteFile> FontData { get; } = new();

    public override void Load() {
        // load and parse data using base class
        base.Load();

        if (TryGetToken("Default", out var defaultFont) && defaultFont is not null) {
            DefaultFont = defaultFont.Contents.Value;
            Log.Info($"default font: {DefaultFont}");
        }
        else {
            Log.Warn($"no default font in {Location!.Name}");
        }

        if (TryGetToken("Fonts", out var fonts) && fonts is not null) {
            Fonts = fonts.Contents.Collection;
            for (int i = 0; i < Fonts.Length; i += 2) {
                string fontName = Fonts[i];
                string fontPath = Fonts[i + 1];

                ByteFile fontFile = new ByteFile(new(Location!.Path, fontPath));
                if (fontFile.Location!.Exists()) {
                    fontFile.Load();
                    FontData[fontName] = fontFile;
                }
                else {
                    Log.Error($"font {fontName} missing file at {fontPath} in {Location!.Name}");
                }
            }
        }
        else {
            Log.Error($"no fonts defined in {Location!.Name}");
        }
    }

    public FontFile(EngineFileLocation header) : base(header) { }
}

