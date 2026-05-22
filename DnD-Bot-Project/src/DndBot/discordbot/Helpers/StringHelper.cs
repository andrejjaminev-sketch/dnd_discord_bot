using System.Collections.Generic;
using System.Text;

namespace DndBot.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Разбивает строку на аргументы, учитывая кавычки (двойные и одинарные).
        /// Аргументы без кавычек разделяются пробелами.
        /// </summary>
        public static List<string> ParseArgs(string input)
        {
            var args = new List<string>();
            if (string.IsNullOrWhiteSpace(input)) return args;

            var current = new StringBuilder();
            char? quoteChar = null;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (quoteChar.HasValue)
                {
                    if (c == quoteChar.Value)
                    {
                        quoteChar = null; // конец кавычек
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"' || c == '\'')
                    {
                        quoteChar = c; // начало кавычек
                    }
                    else if (c == ' ')
                    {
                        if (current.Length > 0)
                        {
                            args.Add(current.ToString());
                            current.Clear();
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            if (current.Length > 0) args.Add(current.ToString());
            return args;
        }

        /// <summary>
        /// Убирает внешние кавычки (двойные и одинарные) и пробелы по краям.
        /// Используется для одиночных текстовых аргументов.
        /// </summary>
        public static string CleanQuotes(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input?.Trim() ?? string.Empty;
            input = input.Trim();
            if (input.Length >= 2 && (input[0] == '"' && input[^1] == '"' || input[0] == '\'' && input[^1] == '\''))
                input = input[1..^1];
            return input;
        }
    }
}