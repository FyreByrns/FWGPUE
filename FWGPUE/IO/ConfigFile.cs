using System.Text;
using System.Reflection;

namespace FWGPUE.IO {
    class ConfigFile : EngineFile {
        static ConfigFile? _instance;
        public static ConfigFile Config {
            get {
                if (_instance == null) {
                    _instance = new ConfigFile();
                }
                return _instance;
            }
        }

        #region attributes for serialization
        class ConfigValue : Attribute { }
        class Comment : Attribute {
            public string Content { get; set; }
            public Comment(string content) {
                Content = content;
            }
        }
        #endregion attributes for serialization

        public static char CommentPrefix = '#';
        public const char ValueSeparator = '=';

        #region values
        [ConfigValue]
        [Comment("width of window")]
        public int ScreenWidth { get; set; } = 800;
        [ConfigValue]
        [Comment("height of window")]
        public int ScreenHeight { get; set; } = 450;

        [ConfigValue]
        [Comment("how many times a second the engine ticks")]
        public int TickRate { get; set; } = 240;

        [ConfigValue]
        [Comment("log level (Inane = -1, All = 0, Warn = 1, Error = 2, None = 3)")]
        public Log.Severity LogSeverity { get; set; } = Log.Severity.All;
        #endregion values

        #region save / load
        protected override void ReadData(byte[] data) {
            string contents = Encoding.ASCII.GetString(data);
            string[] lines = contents.Split(Environment.NewLine);

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++) {
                string line = lines[lineNumber];

                // if the current line is blank
                if (string.IsNullOrEmpty(line)) {
                    continue;
                }
                // if the current line is a comment
                if (line.StartsWith(CommentPrefix)) {
                    continue; // no further parsing this line
                }

                // if none of the above conditions are met, the current line contains a named value
                string[] lineSplit = line.Split(ValueSeparator);
                string valueName = lineSplit[0].Trim();
                string valueValue = lineSplit[1];

                PropertyInfo valueProperty = GetType()!.GetProperty(valueName)!;
                if (valueProperty != null) {
                    var t = valueProperty.PropertyType; // for smaller ifs
                    if (t == typeof(int)) { valueProperty.SetValue(this, int.Parse(valueValue)); }
                    if (t == typeof(string)) { valueProperty.SetValue(this, valueValue); }
                    if (t == typeof(Log.Severity)) { valueProperty.SetValue(this, Enum.Parse<Log.Severity>(valueValue)); }

                    Log.Info($"config value {valueName} loaded as {valueProperty.GetValue(this)}");
                }
            }
        }
        protected override byte[] SaveData() {
            // because I don't know the total length of the data beforehand, a dynamic collection is required instead of just byte[] data = new byte[30];
            List<byte> data = new List<byte>();

            // loop through all properties of this config object
            foreach (PropertyInfo propertyInfo in GetType().GetProperties()) {
                // if the property has the ConfigValue attribute
                if (propertyInfo.GetCustomAttribute<ConfigValue>() != null) {
                    // write all comments
                    foreach (Comment comment in propertyInfo.GetCustomAttributes<Comment>()) {
                        data.AddRange(Encoding.ASCII.GetBytes($"{CommentPrefix}{comment.Content} {Environment.NewLine}"));
                    }
                    // write the data
                    data.AddRange(Encoding.ASCII.GetBytes($"{propertyInfo.Name} {ValueSeparator} {propertyInfo.GetValue(this)} {Environment.NewLine}"));
                }
            }

            // convert dynamic collection to array
            return data.ToArray();
        }
        #endregion save / load

        ConfigFile() : base("config.ini") { }
    }
}