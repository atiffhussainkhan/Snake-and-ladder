using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const string fixturePath = "docs/REPLAY_FIXTURES.md";
if (!File.Exists(fixturePath))
{
    Console.Error.WriteLine($"Fixture file not found: {fixturePath}");
    return 2;
}

var source = File.ReadAllText(fixturePath, Encoding.UTF8);
var expected = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["Configuration"] = "422adc7d5011a3206ee4a10c43e97567a4a8648b63e7834895a2b480237d8dcc",
    ["Initial state"] = "89a657e2b56467b5ab3a5ddb2f649cabfc0c6439cd946bfe6fdcf0784584b6a7",
    ["Action"] = "547ca657ace388605ef1297509e4f0032eda8cd48cef88b87fd62b2cf8e43753",
    ["Resulting state"] = "82f88de95c5a57134018439674608c8adef536685dab77a936d565be831a2f3c",
    ["Event"] = "b4f9548ae12cb9784956e7117d54de74c9f3edf788d0eb103018769865f045d6"
};

var failures = new List<string>();
foreach (var pair in expected)
{
    var section = Regex.Match(
        source,
        $"## {Regex.Escape(pair.Key)}\\s+SHA-256: `(?<hash>[0-9a-f]{{64}})`.*?```json\\s*(?<json>\\{{.*?\\}})\\s*```",
        RegexOptions.Singleline | RegexOptions.CultureInvariant);

    if (!section.Success)
    {
        failures.Add($"{pair.Key}: canonical JSON block or declared hash not found");
        continue;
    }

    var declared = section.Groups["hash"].Value;
    var json = section.Groups["json"].Value;
    var actual = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();

    if (!string.Equals(declared, actual, StringComparison.Ordinal))
    {
        failures.Add($"{pair.Key}: declared {declared}, computed {actual}");
    }

    try
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            failures.Add($"{pair.Key}: canonical JSON root is not an object");
        }
    }
    catch (JsonException exception)
    {
        failures.Add($"{pair.Key}: invalid JSON ({exception.Message})");
    }

    Console.WriteLine($"PASS {pair.Key}: {actual}");
}

var f002 = Regex.Match(source, "F-002 canonical byte check:.*?`(?<bytes>\\{\\\"x\\\":\\\".*?\\\"\\})`", RegexOptions.Singleline);
if (!f002.Success || f002.Groups["bytes"].Value != "{\"x\":\"\\u000a\"}")
{
    failures.Add("F-002: canonical control-character escape check failed");
}
else
{
    Console.WriteLine("PASS F-002: canonical LF escape is literal \\u000a");
}

if (failures.Count > 0)
{
    Console.Error.WriteLine("FAILURES:");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine($"- {failure}");
    }

    return 1;
}

Console.WriteLine("VALIDATION_RESULT=PASS");
Console.WriteLine("SCOPE=F-001 canonical fixture pack and F-002 escaping check");
Console.WriteLine("LIMITATION=This does not validate gameplay transitions or cross-runtime conformance");
return 0;
