
using System;
using System.Threading;

namespace HistoryTracker
{
    internal sealed class HistIdGenerator
    {
        const string _base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static readonly DateTime s_epoch = new (2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static int s_sequence;
        
        public static string NewId(int machineId)
        {
            var now = DateTime.UtcNow;
            var ms = (long)(now - s_epoch).TotalMilliseconds;
            var seq = Interlocked.Increment(ref s_sequence) & 0xFFFF;
            var timePart = ms & 0xFFFFFFFFFFFFL;
            var machinePart = (long)(machineId & 0xFFFF);
            var seqPart = (long)(seq & 0xFFFF);
            var value = (timePart << 32) | (machinePart << 16) | seqPart;
            return ToBase62(value);
        }
        
        static string ToBase62(long value)
        {
            if (value == 0)
            {
                return "0";
            }
            var buffer = new char[16];
            var index = buffer.Length;
            var v = value;
            while (v > 0)
            {
                var r = (int)(v % 62);
                v /= 62;
                buffer[--index] = _base62Chars[r];
            }
            return new string(buffer, index, buffer.Length - index);
        }
    }
}