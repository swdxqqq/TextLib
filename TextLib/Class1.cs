using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TLib
{

    /// Методы расширения для работы со строками.   
    public static class TStringExtensions
    {
        #region Безопасные операции и валидация

        /// Возвращает подстроку, безопасно обрабатывая выход за границы.
        public static string SafeSubstring(this string str, int startIndex, int length = -1)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex), "Ошибка: startIndex должен быть неотрицательным. Текущее значение: " + startIndex);
            if (startIndex >= str.Length) return string.Empty;
            int available = str.Length - startIndex;
            if (length < 0) length = available;
            else if (length > available) length = available;
            return str.Substring(startIndex, length);
        }

        /// Проверяет, является ли строка null, пустой или состоящей из пробелов.
        public static bool IsNullOrBlank(this string str) => string.IsNullOrWhiteSpace(str);

        /// Проверяет, не является ли строка null, пустой или состоящей из пробелов.
        public static bool IsNotBlank(this string str) => !string.IsNullOrWhiteSpace(str);

        #endregion

        #region Поиск

        /// Ищет первое вхождение любой из указанных подстрок с поддержкой StringComparison.
        public static int IndexOfAny(this string str, StringComparison comparisonType, params string[] searchValues)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (searchValues == null) throw new ArgumentNullException(nameof(searchValues), "Ошибка: аргумент не может быть null. Имя параметра: searchValues");
            int best = -1;
            foreach (var value in searchValues)
            {
                if (value == null) continue;
                int idx = str.IndexOf(value, comparisonType);
                if (idx != -1 && (best == -1 || idx < best))
                    best = idx;
            }
            return best;
        }

        /// Подсчитывает количество вхождений подстроки с возможностью перекрытия. 
        public static int CountOccurrences(this string str, string search, StringComparison comparisonType = StringComparison.Ordinal, bool overlap = false)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (search == null) throw new ArgumentNullException(nameof(search), "Ошибка: аргумент не может быть null. Имя параметра: search");
            if (search.Length == 0) return 0;
            int count = 0;
            int pos = 0;
            while (true)
            {
                int idx = str.IndexOf(search, pos, comparisonType);
                if (idx == -1) break;
                count++;
                pos = overlap ? idx + 1 : idx + search.Length;
                if (pos >= str.Length) break;
            }
            return count;
        }

        #endregion

        #region Замена и удаление

        /// Заменяет первое вхождение подстроки.
        public static string ReplaceFirst(this string str, string oldValue, string newValue, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (oldValue == null) throw new ArgumentNullException(nameof(oldValue), "Ошибка: аргумент не может быть null. Имя параметра: oldValue");
            if (oldValue.Length == 0) return str;
            int idx = str.IndexOf(oldValue, comparisonType);
            if (idx == -1) return str;
            return str.Substring(0, idx) + newValue + str.Substring(idx + oldValue.Length);
        }


        /// Заменяет последнее вхождение подстроки.
        public static string ReplaceLast(this string str, string oldValue, string newValue, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (oldValue == null) throw new ArgumentNullException(nameof(oldValue), "Ошибка: аргумент не может быть null. Имя параметра: oldValue");
            if (oldValue.Length == 0) return str;
            int idx = str.LastIndexOf(oldValue, comparisonType);
            if (idx == -1) return str;
            return str.Substring(0, idx) + newValue + str.Substring(idx + oldValue.Length);
        }

        /// Удаляет из строки все вхождения указанных подстрок.
        public static string Remove(this string str, params string[] substrings)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (substrings == null) throw new ArgumentNullException(nameof(substrings), "Ошибка: аргумент не может быть null. Имя параметра: substrings");
            var result = str;
            foreach (var s in substrings)
            {
                if (!string.IsNullOrEmpty(s))
                    result = result.Replace(s, string.Empty);
            }
            return result;
        }

        /// Удаляет символы, удовлетворяющие предикату.
        public static string RemoveWhere(this string str, Func<char, bool> predicate)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (predicate == null) throw new ArgumentNullException(nameof(predicate), "Ошибка: аргумент не может быть null. Имя параметра: predicate");
            return new string(str.Where(c => !predicate(c)).ToArray());
        }

        #endregion

        #region Преобразования регистра и именования

        /// Преобразует строку в camelCase.
        public static string ToCamelCase(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (str.Length == 0) return str;
            var words = SplitWords(str);
            if (words.Count == 0) return string.Empty;
            var first = words[0].ToLowerInvariant();
            var rest = words.Skip(1).Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant());
            return first + string.Concat(rest);
        }

        /// Преобразует строку в PascalCase.
        public static string ToPascalCase(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (str.Length == 0) return str;
            var words = SplitWords(str);
            if (words.Count == 0) return string.Empty;
            return string.Concat(words.Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));
        }

        /// Преобразует строку в snake_case.
        public static string ToSnakeCase(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (str.Length == 0) return str;
            var words = SplitWords(str);
            return string.Join("_", words).ToLowerInvariant();
        }

        /// Преобразует строку в kebab-case.
        public static string ToKebabCase(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (str.Length == 0) return str;
            var words = SplitWords(str);
            return string.Join("-", words).ToLowerInvariant();
        }

        // Вспомогательный метод для разбиения строки на слова по границам слов.
        private static List<string> SplitWords(string str)
        {
            var result = new List<string>();
            var pattern = @"\p{L}+(?:'\p{L}+)?";
            foreach (Match m in Regex.Matches(str, pattern, RegexOptions.IgnoreCase))
                result.Add(m.Value);
            if (result.Count == 0) result.Add(str);
            return result;
        }

        /// Делает первый символ заглавным.
        public static string Capitalize(this string str, bool lowerRest = false)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (str.Length == 0) return str;
            var first = char.ToUpperInvariant(str[0]);
            var rest = lowerRest ? str.Substring(1).ToLowerInvariant() : str.Substring(1);
            return first + rest;
        }

        /// Делает первый символ строчным.
        public static string Uncapitalize(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (str.Length == 0) return str;
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        #endregion

        #region Выравнивание

        /// Выравнивает строку по левому, правому или центральному краю. 
        public static string Align(this string str, int width, char paddingChar = ' ', Alignment alignment = Alignment.Left, bool truncate = false)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (width < 0) throw new ArgumentOutOfRangeException(nameof(width), "Ошибка: width должен быть неотрицательным. Текущее значение: " + width);
            if (truncate && str.Length > width) str = str.Substring(0, width);
            switch (alignment)
            {
                case Alignment.Left: return str.PadRight(width, paddingChar);
                case Alignment.Right: return str.PadLeft(width, paddingChar);
                case Alignment.Center:
                    int left = (width - str.Length) / 2;
                    int right = width - str.Length - left;
                    return new string(paddingChar, left) + str + new string(paddingChar, right);
                default: return str;
            }
        }

        #endregion

        #region Вспомогательные

        /// Возвращает строку в обратном порядке.
        public static string Reverse(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            char[] arr = str.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// Повторяет строку указанное количество раз.
        public static string Repeat(this string str, int count)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Ошибка: count должен быть неотрицательным. Текущее значение: " + count);
            if (count == 0) return string.Empty;
            return string.Concat(Enumerable.Repeat(str, count));
        }

        /// Проверяет, является ли строка палиндромом.
        public static bool IsPalindrome(this string str, bool ignoreCase = true, bool ignoreNonAlphanumeric = true)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Ошибка: аргумент не может быть null. Имя параметра: str");
            var filtered = str;
            if (ignoreNonAlphanumeric)
                filtered = new string(str.Where(char.IsLetterOrDigit).ToArray());
            if (ignoreCase)
                filtered = filtered.ToLowerInvariant();
            int left = 0, right = filtered.Length - 1;
            while (left < right)
            {
                if (filtered[left] != filtered[right]) return false;
                left++;
                right--;
            }
            return true;
        }

        #endregion
    }

    /// Направление выравнивания.
    public enum Alignment
    {
        Left,
        Center,
        Right
    }
}