using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SnakesAndLadders.Domain.Canonical
{
    // R-018 canonical JSON encoder. Produces UTF-8 bytes with:
    //   - keys sorted ordinal ASCII,
    //   - integers as decimal without leading zeros,
    //   - no whitespace, no floats, no null fields, no short escapes,
    //   - booleans lowercase,
    //   - slash unescaped,
    //   - quote and backslash escaped, control bytes (0x00-0x1f) escaped as
    //     lowercase 6-character \u00xx sequences.
    // Output is fully canonical so hashing yields identical bytes on every host.
    public static class CanonicalJson
    {
        public static byte[] EncodeObjectSorted(IEnumerable<KeyValuePair<string, object>> obj)
        {
            var sb = new StringBuilder();
            WriteObject(sb, obj);
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static void WriteObject(StringBuilder sb, IEnumerable<KeyValuePair<string, object>> obj)
        {
            sb.Append('{');
            bool first = true;
            // Sort ordinal ASCII.
            var entries = new List<KeyValuePair<string, object>>(obj);
            entries.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            foreach (var kv in entries)
            {
                if (!first) sb.Append(',');
                first = false;
                WriteString(sb, kv.Key);
                sb.Append(':');
                WriteValue(sb, kv.Value);
            }
            sb.Append('}');
        }

        private static void WriteValue(StringBuilder sb, object v)
        {
            switch (v)
            {
                case null:
                    throw new InvalidOperationException("null is not permitted in canonical JSON");
                case bool b:
                    sb.Append(b ? "true" : "false");
                    break;
                case long l:
                    sb.Append(l.ToString(CultureInfo.InvariantCulture));
                    break;
                case int i:
                    sb.Append(i.ToString(CultureInfo.InvariantCulture));
                    break;
                case ulong u:
                    sb.Append(u.ToString(CultureInfo.InvariantCulture));
                    break;
                case uint u32:
                    sb.Append(u32.ToString(CultureInfo.InvariantCulture));
                    break;
                case string s:
                    WriteString(sb, s);
                    break;
                case Dictionary<string, object> d:
                    WriteObject(sb, d);
                    break;
                case List<object> l:
                    WriteArray(sb, l);
                    break;
                case object[] arr:
                    {
                        var tmp = new List<object>(arr.Length);
                        for (int i = 0; i < arr.Length; i++) tmp.Add(arr[i]);
                        WriteArray(sb, tmp);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unsupported canonical JSON value type: " + v.GetType());
            }
        }

        private static void WriteArray(StringBuilder sb, List<object> arr)
        {
            sb.Append('[');
            for (int i = 0; i < arr.Count; i++)
            {
                if (i > 0) sb.Append(',');
                WriteValue(sb, arr[i]);
            }
            sb.Append(']');
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"') sb.Append("\\\"");
                else if (c == '\\') sb.Append("\\\\");
                else if (c < 0x20) sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:x4}", (int)c);
                else sb.Append(c);
            }
            sb.Append('"');
        }
    }
}
