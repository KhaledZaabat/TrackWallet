using Serilog.Core;
using Serilog.Events;

namespace Expense_Tracker.App.Logging;

/// <summary>
/// Serilog enricher that masks properties whose names indicate token or cookie
/// values so nothing emitted through ILogger leaks raw or hashed auth material
/// (R10.4, R18.4). Also walks into nested <see cref="StructureValue"/> payloads
/// (e.g. scopes that expose AuthCookieOptions or HTTP request objects) and
/// rewrites the offending properties in place.
/// </summary>
public sealed class AuthTokenScrubber : ILogEventEnricher
{
    private const string RedactedMarker = "***REDACTED***";

    /// <summary>
    /// Property names (case-insensitive) whose values MUST NOT be emitted in any form.
    /// Covers the literal property names mentioned in tasks.md (TokenHash, accessToken,
    /// refreshToken, xsrf, Cookie, Set-Cookie) and the header / cookie-option
    /// payloads that might surface through ASP.NET Core scopes.
    /// </summary>
    private static readonly HashSet<string> ForbiddenNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Explicit task names.
        "TokenHash",
        "accessToken",
        "refreshToken",
        "xsrf",
        "Cookie",
        "Set-Cookie",
        // Common cookie/header variants and AuthCookieOptions sub-properties.
        "XSRF-TOKEN",
        "X-XSRF-TOKEN",
        "Authorization",
        "rawRefresh",
        "rawRefreshToken",
        "NewRawToken",
        "access_token",
        "refresh_token",
        "AccessCookieName",
        "RefreshCookieName",
        "CsrfCookieName",
    };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Snapshot property names first — iterating logEvent.Properties directly would throw
        // if we mutate during enumeration.
        List<string> topLevelNames = new(logEvent.Properties.Keys);

        foreach (string name in topLevelNames)
        {
            if (ForbiddenNames.Contains(name))
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(name, RedactedMarker));
            }
            else if (logEvent.Properties.TryGetValue(name, out LogEventPropertyValue? value))
            {
                LogEventPropertyValue? scrubbed = Scrub(value);
                if (!ReferenceEquals(scrubbed, value))
                {
                    logEvent.AddOrUpdateProperty(new LogEventProperty(name, scrubbed));
                }
            }
        }
    }

    private static LogEventPropertyValue Scrub(LogEventPropertyValue value) =>
        value switch
        {
            StructureValue structure => ScrubStructure(structure),
            DictionaryValue dict => ScrubDictionary(dict),
            SequenceValue seq => ScrubSequence(seq),
            _ => value,
        };

    private static StructureValue ScrubStructure(StructureValue structure)
    {
        List<LogEventProperty> rewritten = new(structure.Properties.Count);
        bool changed = false;

        foreach (LogEventProperty prop in structure.Properties)
        {
            if (ForbiddenNames.Contains(prop.Name))
            {
                rewritten.Add(new LogEventProperty(prop.Name, new ScalarValue(RedactedMarker)));
                changed = true;
            }
            else
            {
                LogEventPropertyValue scrubbed = Scrub(prop.Value);
                if (!ReferenceEquals(scrubbed, prop.Value))
                {
                    rewritten.Add(new LogEventProperty(prop.Name, scrubbed));
                    changed = true;
                }
                else
                {
                    rewritten.Add(prop);
                }
            }
        }

        return changed ? new StructureValue(rewritten, structure.TypeTag) : structure;
    }

    private static DictionaryValue ScrubDictionary(DictionaryValue dict)
    {
        List<KeyValuePair<ScalarValue, LogEventPropertyValue>> rewritten = new(dict.Elements.Count);
        bool changed = false;

        foreach (KeyValuePair<ScalarValue, LogEventPropertyValue> kv in dict.Elements)
        {
            string? key = kv.Key.Value?.ToString();
            if (key is not null && ForbiddenNames.Contains(key))
            {
                rewritten.Add(
                    new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                        kv.Key,
                        new ScalarValue(RedactedMarker)
                    )
                );
                changed = true;
            }
            else
            {
                LogEventPropertyValue scrubbed = Scrub(kv.Value);
                if (!ReferenceEquals(scrubbed, kv.Value))
                {
                    rewritten.Add(
                        new KeyValuePair<ScalarValue, LogEventPropertyValue>(kv.Key, scrubbed)
                    );
                    changed = true;
                }
                else
                {
                    rewritten.Add(kv);
                }
            }
        }

        return changed ? new DictionaryValue(rewritten) : dict;
    }

    private static SequenceValue ScrubSequence(SequenceValue seq)
    {
        List<LogEventPropertyValue> rewritten = new(seq.Elements.Count);
        bool changed = false;

        foreach (LogEventPropertyValue element in seq.Elements)
        {
            LogEventPropertyValue scrubbed = Scrub(element);
            if (!ReferenceEquals(scrubbed, element))
                changed = true;
            rewritten.Add(scrubbed);
        }

        return changed ? new SequenceValue(rewritten) : seq;
    }
}
