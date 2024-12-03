namespace Markdown.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Производит проверку наличия строки в строке на позиции i без копирования
    /// </summary>
    /// <param name="original">Исходная строка</param>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="i">Позиция в исходной строке</param>
    /// <returns>True, если строка содержится в исходной строке на позиции i, иначе false</returns>
    public static bool ContainsSubstringOnIndex(this string original, string str, int i)
    {
        for (var j = 0; j < str.Length; j++)
        {
            if (i + j >= original.Length || original[i + j] != str[j])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Проверяет, является ли символ экранированным
    /// </summary>
    /// <param name="str">Строка</param>
    /// <param name="index">Индекс символа</param>
    /// <returns>True, если символ экранирован, иначе false</returns>
    public static bool IsEscaped(this string str, int index)
    {
        var previousIndex = index - 1;
        var backslashCount = 0;
        while (previousIndex >= 0 && str[previousIndex] == '\\')
        {
            backslashCount++;
            previousIndex--;
        }

        return backslashCount % 2 == 1;
    }
}