namespace HandHistoryParser;

public static class DatabaseFunctions {

    public static Database
    AddHand(this Database database, HandHistory handHistory) =>
        database with { HandHistories = database.HandHistories.Add(handHistory) };


    public static Database
    DeleteHand(this Database database, long handId) =>
        database with { 
            HandHistories = database.HandHistories.RemoveAll(hand => hand.HandId == handId),
            DeletedHandIds = database.DeletedHandIds.Add(handId) };
          
    public static ImmutableList<HandHistory>
    GetLastPlayerHands (this Database database, string playerNickname, int handCount) =>
        database.HandHistories
            .Where(hand => hand.Players.Any(player => player.Nickname == playerNickname))
            .Take(handCount)
            .ToImmutableList();

}

