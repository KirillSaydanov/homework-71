using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace homework_71;

public class Bot
{
    private readonly TelegramBotClient _bot;

    public Bot(string token)
    {
        _bot = new TelegramBotClient(token);
    }

    public void StartBot()
    {
        _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        while (true)
        {
            Console.WriteLine("Bot running...");
            Thread.Sleep(int.MaxValue);
        }
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message != null)
        {
            await HandleMessageAsync(update.Message);
        }
        else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            await HandleCallbackQueryAsync(update.CallbackQuery);
        }
    }

    private async Task HandleMessageAsync(Message message)
    {
        var text = message.Text ?? string.Empty;

        switch (text.ToLower())
        {
            case "/start":
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Добро пожаловать в игру \"Камень, ножницы, бумага\"!\n\n" +
                          "Доступные команды:\n" +
                          "/start - начать игру\n" +
                          "/help - узнать правила\n" +
                          "/game - сыграть!",
                    parseMode: ParseMode.Markdown);
                break;

            case "/help":
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Правила игры просты:\n" +
                          "1. Выбирайте одну из трёх опций: камень, ножницы или бумага.\n" +
                          "2. Бот сделает свой выбор случайно.\n" +
                          "3. Побеждает тот, чья комбинация сильнее:\n" +
                          "   - Камень бьёт ножницы.\n" +
                          "   - Ножницы режут бумагу.\n" +
                          "   - Бумага покрывает камень.\n" +
                          "Удачи!",
                    parseMode: ParseMode.Markdown);
                break;

            case "/game":
                await StartGameAsync(message.Chat.Id);
                break;

            default:
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Неизвестная команда. Используйте /start, /help или /game.");
                break;
        }
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        var userChoice = callbackQuery.Data;
        if (userChoice == "repeat")
        {
            await StartGameAsync(callbackQuery.Message.Chat.Id);
            return;
        }
        else if (userChoice == "end")
        {
            await _bot.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "Спасибо за игру! До встречи!");
            return;
        }
        var botChoice = GetRandomChoice();
        var result = RockPaperScissors(userChoice, botChoice);

        await _bot.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: $"Вы выбрали: {TranslateChoice(userChoice)}\n" +
                  $"Бот выбрал: {TranslateChoice(botChoice)}\n\n" +
                  $"Результат: {result}");
        var repeatButtons = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Повторить", "repeat"),
                InlineKeyboardButton.WithCallbackData("Завершить", "end")
            }
        });

        await _bot.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: "Хотите сыграть ещё раз?",
            replyMarkup: repeatButtons);
    }

    private async Task StartGameAsync(long chatId)
    {
        var buttons = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Камень", "rock"),
                InlineKeyboardButton.WithCallbackData("Ножницы", "scissors"),
                InlineKeyboardButton.WithCallbackData("Бумага", "paper")
            }
        });

        await _bot.SendTextMessageAsync(
            chatId: chatId,
            text: "Сделайте свой выбор:",
            replyMarkup: buttons);
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
    }

    private static string GetRandomChoice()
    {
        var choices = new[] { "rock", "scissors", "paper" };
        var random = new Random();
        return choices[random.Next(choices.Length)];
    }

    public static string RockPaperScissors(string first, string second)
        => (first, second) switch
        {
            ("rock", "paper") => "Бумага накрывает камень. Победила бумага!",
            ("rock", "scissors") => "Камень ломает ножницы. Победил камень!",
            ("paper", "rock") => "Бумага накрывает камень. Победила бумага!",
            ("paper", "scissors") => "Ножницы режут бумагу. Победили ножницы!",
            ("scissors", "rock") => "Камень ломает ножницы. Победил камень!",
            ("scissors", "paper") => "Ножницы режут бумагу. Победили ножницы!",
            (_, _) => "Ничья!"
        };

    private static string TranslateChoice(string choice) => choice switch
    {
        "rock" => "Камень",
        "scissors" => "Ножницы",
        "paper" => "Бумага",
        _ => "Неизвестный выбор"
    };
}
