//  <auto-generated>
//  This file was auto-generated by the Ferrite TL Generator.
//  Please do not modify as all changes will be lost.
//  <auto-generated/>

#nullable enable

using System.Buffers;
using System.Runtime.InteropServices;
using Ferrite.Utils;
using DotNext.Buffers;

namespace Ferrite.TL.slim.mtproto;

public readonly ref struct future_salt
{
    private readonly Span<byte> _buff;
    private readonly IMemoryOwner<byte>? _memory;
    public future_salt(int valid_since, int valid_until, long salt)
    {
        var length = GetRequiredBufferSize();
        _memory = UnmanagedMemoryPool<byte>.Shared.Rent(length);
        _memory.Memory.Span.Clear();
        _buff = _memory.Memory.Span[..length];
        SetConstructor(unchecked((int)0x0949d9dc));
        Set_valid_since(valid_since);
        Set_valid_until(valid_until);
        Set_salt(salt);
    }public future_salt(Span<byte> buff)
    {
        _buff = buff;
    }
    
    public readonly int Constructor => MemoryMarshal.Read<int>(_buff);

    private void SetConstructor(int constructor)
    {
        MemoryMarshal.Write(_buff.Slice(0, 4), ref constructor);
    }
    public int Length => _buff.Length;
    public ReadOnlySpan<byte> ToReadOnlySpan() => _buff;
    public TLBytes? TLBytes => _memory != null ? new TLBytes(_memory, 0, _buff.Length) : null;
    public static Span<byte> Read(Span<byte> data, int offset)
    {
        var bytesRead = GetOffset(4, data[offset..]);
        if (bytesRead > data.Length + offset)
        {
            return Span<byte>.Empty;
        }
        return data.Slice(offset, bytesRead);
    }

    public static int GetRequiredBufferSize()
    {
        return 4 + 4 + 4 + 8;
    }
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(4, data[offset..]);
    }
    public readonly int valid_since => MemoryMarshal.Read<int>(_buff[GetOffset(1, _buff)..]);
    private void Set_valid_since(int value)
    {
        MemoryMarshal.Write(_buff[GetOffset(1, _buff)..], ref value);
    }
    public readonly int valid_until => MemoryMarshal.Read<int>(_buff[GetOffset(2, _buff)..]);
    private void Set_valid_until(int value)
    {
        MemoryMarshal.Write(_buff[GetOffset(2, _buff)..], ref value);
    }
    public readonly long salt => MemoryMarshal.Read<long>(_buff[GetOffset(3, _buff)..]);
    private void Set_salt(long value)
    {
        MemoryMarshal.Write(_buff[GetOffset(3, _buff)..], ref value);
    }
    private static int GetOffset(int index, Span<byte> buffer)
    {
        int offset = 4;
        if(index >= 2) offset += 4;
        if(index >= 3) offset += 4;
        if(index >= 4) offset += 8;
        return offset;
    }
    public void Dispose()
    {
        _memory?.Dispose();
    }
}
