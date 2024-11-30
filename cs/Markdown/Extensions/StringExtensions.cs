namespace Markdown.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Производит проверку наличия строки в строке на позиции i без копирования
    /// </summary>
    /// <param name="original">Исходная строка</param>
    /// <param name="str">Проверяемая строка</param>
    /// <param name="i">Позиция в исходной строке</param>
    /// <returns></returns>
    public static bool ContainsSubstringOnIndex(this string original, string str, int i)
    {
        for (var j = 0; j < str.Length; j++)
        {
            if (i + j >= original.Length || original[i + j] != str[j])
                return false;
        }

        return true;
    }
}