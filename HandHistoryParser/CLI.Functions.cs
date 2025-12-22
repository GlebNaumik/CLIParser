using HandHistoryParser;
using HandHistoryParser.Common;

namespace HandHistoryParser;

public static class
CLIFunctions {

    public static void
    RunCLI() {
        var database = new Database(ImmutableList<HandHistory>.Empty, ImmutableList<long>.Empty);
        while (true) {
            "Введите команду:".WriteToConsole();
            var userInput = Console.ReadLine();
            if (userInput == null)
                continue;
            if (!userInput.TryParseCommand(out ICommand command)) {
                "Неизвестная команда или неверные параметры команды.".WriteToConsole();
                continue;
            }
            database = command.ExecuteCommand(database);
        }
    }

    public static Database
    ExecuteCommand(this ICommand userCommand, Database database) {
        return userCommand switch {
            ShowPlayerInformationCommand showPlayer => database.ExecuteShowPlayerInformationCommand(showPlayer.PlayerNickname),
            DeleteHandCommand deleteHand => database.ExecuteDeleteHandCommand(deleteHand.HandId),
            ShowDeletedHandsCommand showDeleted => database.ExecuteShowDeletedHandsCommand(),
            ImportFileCommand importFile => database.ExecuteImportFileCommand(importFile.Path),
            ShowAllHandsInformationCommand showAll => database.ExecuteShowAllHandsInformationCommand(),
            _ => database
        };
    }

    public static Database
    ExecuteShowPlayerInformationCommand(this Database database, string playerNickname) {
        var handsCount = database.GetPlayerHandsCount(playerNickname);
        var lastHands = database.GetLastTenPlayerHands(playerNickname);
        $"Игрок с ником {playerNickname} сыграл {handsCount} раздач.".WriteToConsole();
        "Последние десять раздач:".WriteToConsole();
        foreach (var hand in lastHands) hand.HandPlayerToString(playerNickname).WriteToConsole();
        return database;
    }

    public static Database
    ExecuteDeleteHandCommand(this Database database, long handId) {
        database = database.DeleteHand(handId);
        $"Рука {handId} удалена".WriteToConsole();
        return database;
    }

    public static Database
    ExecuteShowDeletedHandsCommand(this Database database) {
        "Удаленные раздачи:".WriteToConsole();
        foreach (var handId in database.GetDeletedHandsIds()) handId.ToString().WriteToConsole();
        return database;
    }

    public static Database
    ExecuteImportFileCommand(this Database database, string path) {
        var handHistories = path.GetAllTextFromFile().GetHandHistoriesFromFile();
        foreach (var handHistory in handHistories) database = database.AddHand(handHistory);
        $"Файл импортирован".WriteToConsole();
        return database;
    }

    public static Database
    ExecuteShowAllHandsInformationCommand(this Database database) {
        $"Всего раздач в базе: {database.GetHandHistoriesCount()}. Игроков в базе: {database.GetPlayersCount()}".WriteToConsole();
        return database;
    }

    public static bool
    TryParseCommand(this string userInput, out ICommand command) {
        command = null;
        if (userInput.IsNullOrWhiteSpace() || !userInput.TryParseCommandName(out string name, out string[] parameters)) return false;
        var commandType = name.TryParseCommandType();
        if (commandType == null) return false;
        var instance = Activator.CreateInstance(commandType);
        if (!TryParseOptions(instance, parameters)) return false;
        command = (ICommand)instance;
        return true;
    }

    public static bool
    TryParseCommandName(this string userInput, out string name, out string[] parameters) {
        name = null;
        parameters = null;
        if (string.IsNullOrWhiteSpace(userInput)) return false;

        var commandParts = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        name = commandParts[0];
        parameters = commandParts.Skip(1).ToArray();
        return true;
    }

    public static Type
    TryParseCommandType(this string name) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(entities => entities.GetTypes())
            .Where(types => typeof(ICommand).IsAssignableFrom(types))
            .Select(types => new { Type = types, Attr = types.GetCustomAttribute<CommandAttribute>() })
            .Where(attrbiute => attrbiute.Attr != null && attrbiute.Attr.Name == name)
            .Select(attrbiute => attrbiute.Type)
            .FirstOrDefault()!;

    public static bool
    TryParseOptions(object instance, string[] parameters) {
        var properties = instance.GetType()
            .GetProperties()
            .Where(parameters => parameters.GetCustomAttribute<CommandParameterAttribute>() != null)
            .ToArray();

        for (int i = 0; i < parameters.Length; i++) {
            var commandEntuty = parameters[i];
            var property = properties.FirstOrDefault(p => {
                var option = p.GetCustomAttribute<CommandParameterAttribute>();
                return commandEntuty == "--" + option.Name || commandEntuty == "-" + option.ShortName;
            });
            if (property == null)
                return false;
            if (i + 1 >= parameters.Length)
                return false;
            var parameterValue = parameters[++i];
            object convertedValue;

            if (!TryConvertToParameterValue(parameterValue, property.PropertyType, out convertedValue))
                return false;
            property.SetValue(instance, convertedValue);
        }
        return true;
    }

    public static bool
    TryConvertToParameterValue(string input, Type targetType, out object value) {
        value = null;
        if (targetType == typeof(string)) {
            value = input;
            return true;
        }
        if (targetType == typeof(long)) {
            if (long.TryParse(input, out long longValue)) {
                value = longValue;
                return true;
            }
            return false;
        }
        return false;
    }
}
