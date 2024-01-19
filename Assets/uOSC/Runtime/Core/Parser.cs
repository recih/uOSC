using System;
using System.Buffers;
using UnityEngine;
using System.Collections.Generic;

namespace uOSC
{

public static class Identifier
{
    public const string Bundle = "#bundle";

    public const char Int    = 'i';
    public const char Float  = 'f';
    public const char String = 's';
    public const char Blob   = 'b';
    public const char True   = 'T';
    public const char False  = 'F';
}

public class Parser
{
    private static readonly ArrayPool<OCSValue> ValueArrayPool = Message.ValueArrayPool;
    object lockObject_ = new object();
    Queue<Message> messages_ = new Queue<Message>();

    struct TempValues : IDisposable
    {
        private OCSValue[] _values;
        private int _length;

        public Span<OCSValue> AsSpan =>
            _values == null ? Span<OCSValue>.Empty : _values.AsSpan(0, _length);
        
        public TempValues(int length)
        {
            _values = ValueArrayPool.Rent(length);
            _length = length;
        }
        
        public OCSValue this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }

        public void Dispose()
        {
            if (_values != null) ValueArrayPool.Return(_values, true);
        }
    }
    
    public int messageCount
    {
        get { return messages_.Count; }
    }

    public void Parse(byte[] buf, ref int pos, int endPos, ulong timestamp = 0x1u)
    {
        var first = Reader.ParseString(buf, ref pos);

        if (first == Identifier.Bundle)
        {
            ParseBundle(buf, ref pos, endPos);
        }
        else
        {
            using var values = ParseData(buf, ref pos);
            lock (lockObject_)
            {
                messages_.Enqueue(new Message(first, values.AsSpan) 
                {
                    timestamp = new Timestamp(timestamp),
                });
            }
        }

        if (pos != endPos)
        {
            Debug.LogErrorFormat(
                "The parsed data size is inconsitent with the given size: {0} / {1}", 
                pos,
                endPos);
        }
    }

    public Message Dequeue()
    {
        if (messageCount == 0)
        {
            return Message.none;
        }

        lock (lockObject_)
        {
            return messages_.Dequeue();
        }
    }

    void ParseBundle(byte[] buf, ref int pos, int endPos)
    {
        var time = Reader.ParseTimetag(buf, ref pos);

        while (pos < endPos)
        {
            var contentSize = Reader.ParseInt(buf, ref pos);
            if (Util.IsMultipleOfFour(contentSize))
            {
                Parse(buf, ref pos, pos + contentSize, time);
            }
            else
            {
                Debug.LogErrorFormat("Given data is invalid (bundle size ({0}) is not a multiple of 4).", contentSize);
                pos += contentSize;
            }
        }
    }

    TempValues ParseData(byte[] buf, ref int pos)
    {
        // remove ','
        var types = Reader.ParseString(buf, ref pos).Substring(1);

        var n = types.Length;
        if (n == 0) return default;

        var data = new TempValues(n);

        for (int i = 0; i < n; ++i)
        {
            switch (types[i])
            {
                case Identifier.Int    : data[i] = Reader.ParseInt(buf, ref pos); break;
                case Identifier.Float  : data[i] = Reader.ParseFloat(buf, ref pos); break;
                case Identifier.String : data[i] = Reader.ParseString(buf, ref pos); break;
                case Identifier.Blob   : data[i] = Reader.ParseBlob(buf, ref pos); break;
                case Identifier.True   : data[i] = true; break;
                case Identifier.False  : data[i] = false; break;
                default:
                    // Add more types here if you want to handle them.
                    data[i] = OCSValue.Invalid;
                    break;
            }
        }

        return data;
    }
}

}