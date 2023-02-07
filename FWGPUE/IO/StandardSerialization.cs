namespace FWGPUE.IO {
    static class StandardSerialization {
        public class SerializerDeserializerPair {
            public delegate byte[] ToBytes(object value);
            public delegate object FromBytes(byte[] data, ref int startIndex);
            public ToBytes To { get; set; }
            public FromBytes From { get; set; }

            public SerializerDeserializerPair(ToBytes to, FromBytes from) {
                To = to;
                From = from;
            }

            public static implicit operator SerializerDeserializerPair((ToBytes to, FromBytes from) tuple) {
                return new SerializerDeserializerPair(tuple.to, tuple.from);
            }
        }
        static KeyValuePair<Type, SerializerDeserializerPair> m<T>(SerializerDeserializerPair.ToBytes to, SerializerDeserializerPair.FromBytes from) {
            return new KeyValuePair<Type, SerializerDeserializerPair>(typeof(T), new SerializerDeserializerPair(to, from));
        }
        public static Dictionary<Type, SerializerDeserializerPair> SerializerDeserializerPairs = new() {
            { typeof(int),
                new(i => BitConverter.GetBytes((int)i), (byte[] d, ref int s) => {
                    s+=sizeof(int);
                    return BitConverter.ToInt32(d, s);
            }) },
        };

        public static byte[] ToBytes<T>(T value) {
            if (value is not null) {
                if (SerializerDeserializerPairs.ContainsKey(typeof(T))) {
                    return SerializerDeserializerPairs[typeof(T)].To(value);
                }
            }
            return Array.Empty<byte>();
        }
        public static T? FromBytes<T>(byte[] bytes, ref int index) {
            if (SerializerDeserializerPairs.ContainsKey(typeof(T))) {
                return (T)SerializerDeserializerPairs[typeof(T)].From(bytes, ref index);
            }
            return default;
        }
    }
}