using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

public class StreamWriter<T> where T : class
{
    public Stream BaseStream { get; private set; }

    private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

    public StreamWriter(Stream stream)
    {
        BaseStream = stream;
    }

    private byte[] Serialize(T obj)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                _binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }
        catch(Exception e)
        {
            UnityEngine.Debug.Log(e);
            return null;
        }
    }

    private void WriteLength(int len)
    {
        var lenbuf = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(len));
        BaseStream.Seek(0, SeekOrigin.End);
        BaseStream.Write(lenbuf, 0, lenbuf.Length);
    }

    private void WriteObject(byte[] data)
    {
        BaseStream.Seek(0, SeekOrigin.End);
        BaseStream.Write(data, 0, data.Length);
    }

    private void Flush()
    {
        BaseStream.Flush();
    }

    public void WriteObject(T obj)
    {
        var data = Serialize(obj);
        WriteLength(data.Length);
        WriteObject(data);
        Flush();
    }
}
