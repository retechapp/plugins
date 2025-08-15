using System;
using System.Buffers;

namespace Retech.Network;

public class PacketWriter(int capacity = 256) : IDisposable
{
  private byte[] _buffer = ArrayPool<byte>.Shared.Rent(capacity);
  private int _position = 0;

  public int Length => _position;
  public int Capacity => _buffer.Length;
  public void Reset() => _position = 0;
  public ArraySegment<byte> AsSegment() => new(_buffer, 0, _position);

  private void Ensure(int extra)
  {
    int needed = _position + extra;
    if (needed <= _buffer.Length)
      return;

    int next = _buffer.Length << 1;
    while (next < needed)
      next <<= 1;

    byte[] grown = ArrayPool<byte>.Shared.Rent(next);
    Buffer.BlockCopy(_buffer, 0, grown, 0, _position);
    ArrayPool<byte>.Shared.Return(_buffer);
    _buffer = grown;
  }

  public void WriteByte(byte value)
  {
    Ensure(1);
    _buffer[_position] = value;
    _position++;
  }

  public void WriteUInt16(ushort value)
  {
    Ensure(2);
    _buffer[_position + 0] = (byte)value;
    _buffer[_position + 1] = (byte)(value >> 8);
    _position += 2;
  }

  public void WriteUInt32(uint value)
  {
    Ensure(4);
    _buffer[_position + 0] = (byte)value;
    _buffer[_position + 1] = (byte)(value >> 8);
    _buffer[_position + 2] = (byte)(value >> 16);
    _buffer[_position + 3] = (byte)(value >> 24);
    _position += 4;
  }

  public void WriteUInt64(ulong value)
  {
    Ensure(8);
    _buffer[_position + 0] = (byte)value;
    _buffer[_position + 1] = (byte)(value >> 8);
    _buffer[_position + 2] = (byte)(value >> 16);
    _buffer[_position + 3] = (byte)(value >> 24);
    _buffer[_position + 4] = (byte)(value >> 32);
    _buffer[_position + 5] = (byte)(value >> 40);
    _buffer[_position + 6] = (byte)(value >> 48);
    _buffer[_position + 7] = (byte)(value >> 56);
    _position += 8;
  }

  public void WriteFloat(float value)
  {
    Ensure(4);
    byte[] bytes = BitConverter.GetBytes(value);
    Buffer.BlockCopy(bytes, 0, _buffer, _position, bytes.Length);
    _position += bytes.Length;
  }

  public void WriteBytes(byte[] value)
  {
    if (value == null || value.Length == 0)
    {
      WriteUInt32(0);
      return;
    }

    WriteUInt32((uint)value.Length);
    Ensure(value.Length);
    Buffer.BlockCopy(value, 0, _buffer, _position, value.Length);
    _position += value.Length;
  }

  public void WriteString(string value)
  {
    if (value == null || value.Length == 0)
    {
      WriteUInt32(0);
      return;
    }

    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
    WriteUInt32((uint)bytes.Length);
    Ensure(bytes.Length);
    Buffer.BlockCopy(bytes, 0, _buffer, _position, bytes.Length);
    _position += bytes.Length;
  }

  public void Dispose()
  {
    ArrayPool<byte>.Shared.Return(_buffer);
    _position = 0;
  }
}

