using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace uOSC
{

public struct Message : IDisposable
{
    public static readonly ArrayPool<OCSValue> ValueArrayPool = ArrayPool<OCSValue>.Create(512, 1024);
    
    public string address { get; internal set; }
    public Timestamp timestamp { get; internal set; }
    public ReadOnlySpan<OCSValue> values =>
        _values == null ? ReadOnlySpan<OCSValue>.Empty : _values.AsSpan(0, _valuesLength);
    
    private OCSValue[] _values;
    private int _valuesLength;

    public static Message none
    {
        get { return new Message(""); } 
    }

    private Message(string address)
    {
        this.address = address;
        this.timestamp = new Timestamp();
        _values = null;
        _valuesLength = 0; 
    }

    public Message(string address, params OCSValue[] packet) : this(address)
    {
        SetValues(packet);
    }
    
    public Message(string address, ReadOnlySpan<OCSValue> packet) : this(address)
    {
        SetValues(packet);
    }
    
    public Message(string address, IList<OCSValue> packet) : this(address)
    {
        SetValues(packet);
    }

    private void SetValues(IList<OCSValue> valuesList)
    {
        DisposeValues();
        if (valuesList == null) return;

        _values = ValueArrayPool.Rent(valuesList.Count);
        valuesList.CopyTo(_values, 0);
        _valuesLength = valuesList.Count;
    }
    
    private void SetValues(OCSValue[] valuesArray)
    {
        SetValues(valuesArray.AsSpan());
    }
    
    private void SetValues(ReadOnlySpan<OCSValue> valuesSpan)
    {
        DisposeValues();
        if (valuesSpan.IsEmpty) return;

        _values = ValueArrayPool.Rent(valuesSpan.Length);
        valuesSpan.CopyTo(_values);
        _valuesLength = valuesSpan.Length;
    }
    
    private void DisposeValues()
    {
        if (_values == null) return;
        
        foreach (var value in values)
        {
            value.Dispose();
        }
        ValueArrayPool.Return(_values, true);
        _values = null;
        _valuesLength = 0;
    }

    public void Write(MemoryStream stream)
    {
        WriteAddress(stream);
        WriteTypes(stream);
        WriteValues(stream);
    }

    void WriteAddress(MemoryStream stream)
    {
        Writer.Write(stream, address);
    }

    void WriteTypes(MemoryStream stream)
    {
        string types = ",";
        for (int i = 0; i < values.Length; ++i)
        {
            var value = values[i];
            var type = value.Type;
            if      (type == OCSValue.ValueType.Int)    types += Identifier.Int;
            else if (type == OCSValue.ValueType.Float)  types += Identifier.Float;
            else if (type == OCSValue.ValueType.String) types += Identifier.String;
            else if (type == OCSValue.ValueType.Blob) types += Identifier.Blob;
            else if (type == OCSValue.ValueType.Bool)   types += (bool)value ? Identifier.True : Identifier.False;
        }
        Writer.Write(stream, types);
    }

    void WriteValues(MemoryStream stream)
    {
        for (int i = 0; i < values.Length; ++i)
        {
            Writer.Write(stream, values[i]);
        }
    }

    public override string ToString()
    {
        var str = new StringBuilder();

        str.Append(address);
        str.Append("\t");

        foreach (var value in values)
        {
            str.Append(value.GetString());
            str.Append(" ");
        }

        str.Append($"({timestamp.ToLocalTime()})");

        return str.ToString();
    }

    public void Dispose()
    {
        DisposeValues();
    }
}

}