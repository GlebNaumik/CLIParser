namespace HandHistoryParser;

public record
Database(
    ImmutableList<HandHistory> HandHistories,
    ImmutableList<long> DeletedHandIds) {

    public static Database
    Empty => new Database(
        HandHistories: [],
        DeletedHandIds: []);

    public int HandHistoryCount => HandHistories.Count;

    public IEnumerable<string>
    PlayerNicknames => HandHistories
        .SelectMany(hand => hand.Players)
        .Select(player => player.Nickname)
        .Distinct();

    public int PlayerCount => PlayerNicknames.Count();
    public int GetPlayerHandCount(string nickname) => HandHistories.Count(hand => hand.ContainsPlayer(nickname));
    public IEnumerable<HandHistory> GetPlayerHandHistories(string nickname) => HandHistories.Where(hand => hand.ContainsPlayer(nickname));

}