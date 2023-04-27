using System.IO;
using System.Collections.Generic;

namespace uOSC
{

public class Bundle
{
    private Timestamp timestamp;
    private List<(Message message, Bundle bundle)> elements_ = new();

    public Bundle()
    {
        this.timestamp = Timestamp.Immediate;
    }

    public Bundle(Timestamp timestamp)
    {
        this.timestamp = timestamp;
    }

    public void Add(Message message)
    {
        elements_.Add((message, null));
    }

    public void Add(Bundle bundle)
    {
        elements_.Add((Message.none, bundle));
    }

    public void Write(MemoryStream stream)
    {
        Writer.Write(stream, Identifier.Bundle);
        Writer.Write(stream, timestamp);

        for (int i = 0; i < elements_.Count; ++i)
        {
            var (message, bundle) = elements_[i];
            if (bundle != null)
            {
                Write(stream, bundle);
            }
            else
            {
                Write(stream, message);
            }
        }
    }

    void Write(MemoryStream stream, Message message)
    {
        using (var tmpStream = new MemoryStream())
        {
            message.Write(tmpStream);
            Writer.Write(stream, tmpStream);
        }
    }

    void Write(MemoryStream stream, Bundle bundle)
    {
        using (var tmpStream = new MemoryStream())
        {
            bundle.Write(tmpStream);
            Writer.Write(stream, tmpStream);
        }
    }
}

}