namespace HRSystem.API.Services;

/// <summary>
/// Defines the relative management rank of each PeopleOS role so that
/// account actions (like deactivation) can only be performed by a
/// strictly higher-ranked role against a lower-ranked one.
/// </summary>
public static class RoleHierarchy
{
    private static readonly Dictionary<string, int> Ranks = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PeopleOS Super Admin"] = 100,
        ["Admin"] = 80,
        ["Company Administrator"] = 80,
        ["Manager"] = 50,
        ["HR"] = 40,
        ["Employee"] = 10
    };

    /// <summary>Unranked/custom roles default to just above Employee so they can still be managed by Managers and above.</summary>
    private const int DefaultRank = 20;

    public static int RankOf(string roleName) => Ranks.TryGetValue(roleName, out var rank) ? rank : DefaultRank;

    public static int HighestRankOf(IEnumerable<string> roleNames)
    {
        var rank = 0;
        foreach (var roleName in roleNames)
            rank = Math.Max(rank, RankOf(roleName));
        return rank;
    }

    /// <summary>True when the acting user's highest rank is strictly greater than the target user's highest rank.</summary>
    public static bool CanManage(IEnumerable<string> actingRoleNames, IEnumerable<string> targetRoleNames)
        => HighestRankOf(actingRoleNames) > HighestRankOf(targetRoleNames);
}
