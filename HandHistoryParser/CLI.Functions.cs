using HandHistoryParser;
using HandHistoryParser.Common;
using System.Data;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace HandHistoryParser;

public static class
CLIFunctions {
    public static void
    Main(string[] args) {
        RunCLI();
    }

    public static void
    RunCLI() {
        var database = Database.Empty;
        while (true) {
            try {
                "Введите команду:".WriteToConsole();
                var userInput = Console.ReadLine();
                var command = userInput.ParseCommand();
                database = command.ExecuteCommand(database);
            }
            catch (Exception ex) {
                $"Произошла ошибка: {ex.Message}".WriteToConsole();
            }
        }
    }

    public static Database
    ExecuteCommand(this ICommand userCommand, Database database) {
        return userCommand switch {
            ShowPlayerInformationCommand showPlayer => database.ExecuteShowPlayerInformationCommand(showPlayer.PlayerNickname),
            DeleteHandCommand deleteHand => database.ExecuteDeleteHandCommand(deleteHand.HandId),
            ShowDeletedHandsCommand showDeleted => database.ExecuteShowDeletedHandCommand(),
            ImportFileCommand importFile => database.ExecuteImportFileCommand(importFile.Path),
            ShowAllHandsInformationCommand showAll => database.ExecuteShowAllHandsInformationCommand(),
            _ => database
        };
    }

    public static Database
    ExecuteShowPlayerInformationCommand(this Database database, string playerNickname) {
        var handsCount = database.GetPlayerHandCount(playerNickname);
        var lastHands = database.GetLastPlayerHands(playerNickname, 10);
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
    ExecuteShowDeletedHandCommand(this Database database) {
        "Удаленные раздачи:".WriteToConsole();
        foreach (var handId in database.DeletedHandIds) handId.ToString().WriteToConsole();
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
        $"Всего раздач в базе: {database.HandHistoryCount}. Игроков в базе: {database.PlayerCount}".WriteToConsole();
        return database;
    }

    public static ICommand
    ParseCommand(this string userInput) {
        if (userInput.IsNullOrWhiteSpace())
            throw new InvalidUserInputException("No user input");

        var commandName = userInput.ParseCommandName();
        var constructor = commandName.ParseCommandType().GetMainConstructor();
        var parameterValues = userInput
            .ParseCommandParameters()
            .ToList()
            .GetMatchingParameterValues(constructor)
            .ToArray();
        return (ICommand)constructor.Invoke(parameterValues);
    }

    public static IEnumerable<object>
    GetMatchingParameterValues(this IList<(string name, string? value)> inputParameters, ConstructorInfo constructor) {
        foreach (var parameter in constructor.GetParameters())
            yield return inputParameters.GetParameterValue(parameter);
    }

    public static object
    GetParameterValue(this IList<(string name, string? value)> input, ParameterInfo parameter) {
        foreach (var (name, value) in input)
            return value.ParseParameterValue(parameter.ParameterType);
        throw new InvalidUserInputException($"Не удалось получить значение параметра '{parameter.Name}' для команды.");
    }

    public static string
    ParseCommandName (this string userInput) {
        if (string.IsNullOrWhiteSpace(userInput)) throw new InvalidOperationException("Команда не введена");
        return userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
    }

    public static IEnumerable<(string? name, string? value)>
    ParseCommandParameters(this string userInput) {
        var parts = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();
        for (int i = 0; i + 1 < parts.Count; i += 2) {
            yield return (parts[i].NormalizeString(), parts[i + 1]);
        }
    }

    public static Type
    ParseCommandType (this string name) {
        foreach (var commandType in typeof(ICommand).GetAllAssignableTypes()) {
            var attribute = commandType.GetCustomAttribute<NameAttribute>();
            if (attribute != null && attribute.Name == name) return commandType;
        }
        throw new InvalidOperationException($"Команда с именем '{name}' не найдена");
    }

    public static object
    ParseParameterValue(this string? value, Type targetType) {
        if (targetType == typeof(string))
            return value ?? string.Empty;

        if (targetType == typeof(bool)) {
            if (value.IsNullOrWhiteSpace() || value.EqualsIgnoreCase("true"))
                return true;
            return false;
        }

        if (targetType == typeof(long))
            if (long.TryParse(value, out var longResult))
                return longResult;
            else
                throw new InvalidUserInputException($"Не удалось преобразовать значение '{value}' в тип long.");
        if (targetType == typeof(int))
            if (int.TryParse(value, out var intResult))
                return intResult;
            else
                throw new InvalidUserInputException($"Не удалось преобразовать значение '{value}' в тип int.");

        throw new InvalidUserInputException($"Неизвестный тип параметра: {targetType.Name}");
    }
}
