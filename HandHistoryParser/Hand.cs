namespace HandHistoryParser;

public class
HandHistory {
    public long HandId { get; }
    public ImmutableList<HandHistoryPlayer> Players { get; }
    public HandHistory(long handId, ImmutableList<HandHistoryPlayer> players) {
        HandId = handId;
        Players = players;
    }
    public bool ContainsPlayer(string nickname) => Players.Any(player => player.Nickname == nickname);
}

public class
HandHistoryPlayer {
    public int SeatNumber { get; }
    public string Nickname { get; }
    public double StackSize { get; }
    public Currencies Currency { get; }
    public ImmutableList<Card> DealtCards { get; }  
    public HandHistoryPlayer(int seatNumber, string nickname, double stackSize, Currencies currency, ImmutableList<Card> dealtCards) {
        SeatNumber = seatNumber;
        Nickname = nickname;
        StackSize = stackSize;
        Currency = currency;
        DealtCards = dealtCards;
    }
}

public enum
Currencies {
    Dollar,
    Euro,
    Undefined
}