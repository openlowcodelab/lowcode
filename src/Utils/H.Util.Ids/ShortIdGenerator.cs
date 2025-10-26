using Sqids;

namespace H.Util.Ids;

public class ShortIdGenerator
{
    public static string Generate(int minLength = 8, bool isLower = true)
    {
        var sqids = new SqidsEncoder<int>(new()
        {
            MinLength = minLength,
            Alphabet = "k3G7QAe51FCsPW92uEOyq4Bg6Sp8YzVTmnU0liwDdHXLajZrfxNhobJIRcMvKt",
        });
        int[] numbers = GenerateUniqueRandom(1, 200, 3);
        var id = sqids.Encode(numbers);

        if (isLower)
        {
            id = id.ToLower();
        }

        return id;
    }

    /// <summary>
    /// 生成随机数组
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    private static int[] GenerateUniqueRandom(int minValue, int maxValue, int n)
    {
        //如果生成随机数个数大于指定范围的数字总数，则最多只生成该范围内数字总数个随机数
        if (n > maxValue - minValue + 1)
            n = maxValue - minValue + 1;

        int maxIndex = maxValue - minValue + 2;// 索引数组上限
        int[] indexArr = new int[maxIndex];
        for (int i = 0; i < maxIndex; i++)
        {
            indexArr[i] = minValue - 1;
            minValue++;
        }

        Random ran = new Random();
        int[] randNum = new int[n];
        int index;
        for (int j = 0; j < n; j++)
        {
            index = ran.Next(1, maxIndex - 1);// 生成一个随机数作为索引

            //根据索引从索引数组中取一个数保存到随机数数组
            randNum[j] = indexArr[index];

            // 用索引数组中最后一个数取代已被选作随机数的数
            indexArr[index] = indexArr[maxIndex - 1];
            maxIndex--; //索引上限减 1
        }
        return randNum;
    }
}
