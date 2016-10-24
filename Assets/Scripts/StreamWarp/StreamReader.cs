using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class StreamReader<T> where T : class
{
    public Stream BaseStream { get; private set; }
    private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();
    const int lensize = sizeof(int);
    int position = 0;
    public StreamReader(Stream stream)
    {
        BaseStream = stream;
    }

    private int ReadLength()
    {
        BaseStream.Seek(position, SeekOrigin.Begin);

        var lenbuf = new byte[lensize];
        var bytesRead = BaseStream.Read(lenbuf, 0, lensize);

        if (bytesRead == 0)
        {
            return 0;
        }
        if (bytesRead != lensize)
            throw new IOException(string.Format("Expected {0} bytes but read {1}", lensize, bytesRead));

        position += lensize;
        return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenbuf, 0));
    }

    private T ReadObject(int len)
    {
        BaseStream.Seek(position, SeekOrigin.Begin);

        var data = new byte[len];
        BaseStream.Read(data, 0, len);

        position += len;

        ResetPosition(len);

        using (var memoryStream = new MemoryStream(data))
        {
            return (T)_binaryFormatter.Deserialize(memoryStream);
        }

    }

    private void ResetPosition(int dataLength)
    {
        if (BaseStream.Length == position)
        {
            position = 0;
            BaseStream.SetLength(0);
        }
    }

    public T ReadObject()
    {
        var len = ReadLength();
        return len == 0 ? default(T) : ReadObject(len);
    }
}
