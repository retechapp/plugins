using System.IO;

namespace RetechLibrary.Network;

public class BinaryPacket
{
    private readonly MemoryStream _stream;
    private readonly BinaryWriter _writer;
    private readonly BinaryReader _reader;

    public BinaryPacket(byte[]? bytes = null)
    {
        _stream = new MemoryStream();
        _writer = new BinaryWriter(_stream);
        _reader = new BinaryReader(_stream);

        if (bytes != null)
        {
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Position = 0;
        }
    }

    public void WriteByte(byte value) => _writer.Write(value);
    public byte ReadByte() => _reader.ReadByte();

    public void WriteUInt16(ushort value) => _writer.Write(value);
    public ushort ReadUInt16() => _reader.ReadUInt16();

    public void WriteUInt32(uint value) => _writer.Write(value);
    public uint ReadUInt32() => _reader.ReadUInt32();

    public void WriteUInt64(ulong value) => _writer.Write(value);
    public void ReadUInt64() => _reader.ReadUInt64();

    public void WriteFloat(float value) => _writer.Write(value);
    public float ReadFloat() => _reader.ReadSingle();

    public void WriteVarBytes(byte[] value)
    {
        if (value.Length == 0)
        {
            WriteUInt32(0);
            return;
        }

        WriteUInt32((uint)value.Length);
        _writer.Write(value, 0, value.Length);
    }

    public byte[] ReadVarBytes()
    {
        uint size = ReadUInt32();

        if (size == 0)
        {
            return [];
        }

        return _reader.ReadBytes((int)size);
    }

    public void WriteVarString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            WriteVarBytes([]);
            return;
        }

        WriteVarBytes(System.Text.Encoding.UTF8.GetBytes(value));
    }

    public void ReadVarString() => System.Text.Encoding.UTF8.GetString(ReadVarBytes());

    public byte[] ToArray() => _stream.ToArray();

    public long Remaining() => _stream.Length - _stream.Position;
}
