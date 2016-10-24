using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

public class StreamWrapper<TReadWrite> where TReadWrite : class{

    public Stream BaseStream { get; private set; }
    public bool CanRead
    {
        get { return BaseStream.CanRead; }
    }
    public bool CanWrite
    {
        get { return BaseStream.CanWrite; }
    }

    private readonly StreamReader<TReadWrite> _reader;
    private readonly StreamWriter<TReadWrite> _writer;

    /// <summary>
    /// Constructs a new <c>PipeStreamWrapper</c> object that reads from and writes to the given <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">Stream to read from and write to</param>
    public StreamWrapper(Stream stream)
    {
        BaseStream = stream;
        _reader = new StreamReader<TReadWrite>(BaseStream);
        _writer = new StreamWriter<TReadWrite>(BaseStream);
    }

    public TReadWrite ReadObject()
    {
        return _reader.ReadObject();
    }
    public void WriteObject(TReadWrite obj)
    {
        _writer.WriteObject(obj);
    }
    public void Close()
    {
        BaseStream.Close();
    }
}
