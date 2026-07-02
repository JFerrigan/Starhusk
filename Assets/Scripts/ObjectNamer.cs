using System.Collections.Generic;
using UnityEngine;

public static class ObjectNamer
{
    private static readonly string[] StarNames =
    {
        "Sirius",
        "Vega",
        "Altair",
        "Rigel",
        "Aldebaran",
        "Antares",
        "Deneb",
        "Arcturus",
        "Bellatrix",
        "Procyon",
        "Capella",
        "Spica",
        "Regulus",
        "Fomalhaut",
        "Pollux",
        "Castor"
    };

    private static readonly string[] PlanetNames =
    {
        "Betelgeuse",
        "Mirach",
        "Schedar",
        "Alpheratz",
        "Alnitak",
        "Mintaka",
        "Saiph",
        "Alhena",
        "Elnath",
        "Hamal",
        "Menkar",
        "Mira",
        "Algol",
        "Polaris",
        "Mirfak",
        "Dubhe",
        "Merak",
        "Phecda",
        "Mizar",
        "Alkaid"
    };

    private static readonly Dictionary<string, int> ManMadeCounters = new Dictionary<string, int>();

    public static void ResetManMadeCounters()
    {
        ManMadeCounters.Clear();
    }

    public static string StarNameForSeed(int seed)
    {
        return StarNames[PositiveIndex(seed, StarNames.Length)];
    }

    public static string PlanetNameFor(int seed, int index)
    {
        int start = PositiveIndex(seed * 31, PlanetNames.Length);
        return PlanetNames[(start + index) % PlanetNames.Length];
    }

    public static string NumberedManMadeName(string baseName)
    {
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "Object";
        }

        int count;
        ManMadeCounters.TryGetValue(baseName, out count);
        count++;
        ManMadeCounters[baseName] = count;

        return baseName + " " + count.ToString("000");
    }

    public static ObjectIdentity AssignIdentity(GameObject target, string displayName, ObjectIdentityCategory category)
    {
        if (target == null)
        {
            return null;
        }

        ObjectIdentity identity = target.GetComponent<ObjectIdentity>();
        if (identity == null)
        {
            identity = target.AddComponent<ObjectIdentity>();
        }

        identity.Configure(displayName, category);
        return identity;
    }

    private static int PositiveIndex(int value, int length)
    {
        if (length <= 0)
        {
            return 0;
        }

        return Mathf.Abs(value == int.MinValue ? 0 : value) % length;
    }
}
