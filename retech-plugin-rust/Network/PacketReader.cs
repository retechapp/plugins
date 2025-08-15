using System;

namespace Retech.Network;

public class PacketReader(byte[] buffer, int length)
{
  private readonly byte[] _buffer = buffer;
  private readonly int _limit = length;
  private int _position = 0;

  public int Remaining => _limit - _position;
  public bool End => _position >= _limit;
  public void Skip(int count) => _position += count;

  private void Need(int count)
  {
    if (Remaining < count)
      throw new InvalidOperationException("Not enough data to read.");
  }

  public byte ReadByte()
  {
    Need(1);
    int position = _position;
    _position += 1;
    return _buffer[position];
  }

  public ushort ReadUInt16()
  {
    Need(2);
    int position = _position;
    _position += 2;
    return (ushort)(_buffer[position]
      | (_buffer[position + 1] << 8));
  }

  public uint ReadUInt32()
  {
    Need(4);
    int position = _position;
    _position += 4;
    return (uint)(_buffer[position]
      | (_buffer[position + 1] << 8)
      | (_buffer[position + 2] << 16)
      | (_buffer[position + 3] << 24));
  }

  public ulong ReadUInt64()
  {
    Need(8);
    int position = _position;
    _position += 8;
    return (ulong)(_buffer[position]
      | (_buffer[position + 1] << 8)
      | (_buffer[position + 2] << 16)
      | (_buffer[position + 3] << 24)
      | (_buffer[position + 4] << 32)
      | (_buffer[position + 5] << 40)
      | (_buffer[position + 6] << 48)
      | (_buffer[position + 7] << 56));
  }

  public float ReadFloat()
  {
    Need(4);
    int position = _position;
    _position += 4;
    return BitConverter.ToSingle(_buffer, position);
  }

  public byte[] ReadBytes()
  {
    uint length = ReadUInt32();
    if (length == 0)
      return [];

    Need((int)length);
    byte[] result = new byte[length];
    Buffer.BlockCopy(_buffer, _position, result, 0, (int)length);
    _position += (int)length;
    return result;
  }

  public string ReadString()
  {
    uint length = ReadUInt32();
    if (length == 0)
      return string.Empty;

    Need((int)length);
    byte[] result = new byte[length];
    Buffer.BlockCopy(_buffer, _position, result, 0, (int)length);
    _position += (int)length;
    return System.Text.Encoding.UTF8.GetString(result);
  }
}
