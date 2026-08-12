namespace H.Util.Ids;

/// <summary>
/// 雪花ID生成器
/// </summary>
public static class SnowflakeIdGenerator
{
    // 起始时间戳 (2024-01-01 00:00:00 UTC)
    private const long Twepoch = 1704067200000L;

    // 机器标识位数
    private const int WorkerIdBits = 5;
    // 数据中心标识位数
    private const int DatacenterIdBits = 5;
    // 序列号标识位数
    private const int SequenceBits = 12;

    // 机器ID最大值
    private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
    // 数据中心ID最大值
    private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);
    // 序列号掩码
    private const long SequenceMask = -1L ^ (-1L << SequenceBits);

    // 机器ID偏左移12位
    private const int WorkerIdShift = SequenceBits;
    // 数据中心ID左移17位
    private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
    // 时间毫秒左移22位
    private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

    private static long _workerId = 1;
    private static long _datacenterId = 1;
    private static long _sequence = 0L;
    private static long _lastTimestamp = -1L;

    private static readonly object _lock = new object();

    /// <summary>
    /// 生成下一个ID
    /// </summary>
    /// <returns></returns>
    public static long NextId()
    {
        lock (_lock)
        {
            var timestamp = TimeGen();

            if (timestamp < _lastTimestamp)
            {
                throw new Exception($"Clock moved backwards. Refusing to generate id for {_lastTimestamp - timestamp} milliseconds");
            }

            if (_lastTimestamp == timestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    timestamp = TilNextMillis(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            return ((timestamp - Twepoch) << TimestampLeftShift) |
                   (_datacenterId << DatacenterIdShift) |
                   (_workerId << WorkerIdShift) |
                   _sequence;
        }
    }

    private static long TilNextMillis(long lastTimestamp)
    {
        var timestamp = TimeGen();
        while (timestamp <= lastTimestamp)
        {
            timestamp = TimeGen();
        }
        return timestamp;
    }

    private static long TimeGen()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
