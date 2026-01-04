namespace HandHistoryParser;

public interface ICommand;

[Name("showallinfo"),Description("Вывести общее количество раздач и игроков в базе данных")]
public record ShowAllHandsInformationCommand : ICommand;

[Name("showplayer"), Description("Вывести количество раздач в в базе данных на игрока и последние 10 раздач")]
public record ShowPlayerInformationCommand : ICommand {
    [Alias("n")]
    public string PlayerNickname { get; init; }
    public ShowPlayerInformationCommand(string playerNickname) {
        PlayerNickname = playerNickname;
    }
}

[Name("deletehand"), Description("Удалить раздачу с заданным ID")]
public record DeleteHandCommand : ICommand {
    [Alias("h")]
    public long HandId { get; init; }
    public DeleteHandCommand(long handId) {
        HandId = handId;
    }
}

[Name("showdeletedhands"), Description("Показать список удаленных раздач")]
public record ShowDeletedHandsCommand : ICommand;

[Name("importfile"), Description("Импортировать раздачи из файла")]
public record ImportFileCommand : ICommand {
    [Alias("f")]
    public string Path { get; init; }
    public ImportFileCommand(string path) {
        Path = path;
    }
}

public class
InvalidUserInputException : Exception {
    public InvalidUserInputException(string message) : base(message) { }
}


