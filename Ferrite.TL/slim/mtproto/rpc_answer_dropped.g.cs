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

public readonly ref struct rpc_answer_dropped
{
    private readonly Span<byte> _buff;
    private readonly IMemoryOwner<byte>? _memory;
    public rpc_answer_dropped(long msg_id, int seq_no, int bytes)
    {
        var length = GetRequiredBufferSize();
        _memory = UnmanagedMemoryPool<byte>.Shared.Rent(length);
        _memory.Memory.Span.Clear();
        _buff = _memory.Memory.Span[..length];
        SetConstructor(unchecked((int)0xa43ad8b7));
        Set_msg_id(msg_id);
        Set_seq_no(seq_no);
        Set_bytes(bytes);
    }public rpc_answer_dropped(Span<byte> buff)
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
        return 4 + 8 + 4 + 4;
    }
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(4, data[offset..]);
    }
    public readonly long msg_id => MemoryMarshal.Read<long>(_buff[GetOffset(1, _buff)..]);
    private void Set_msg_id(long value)
    {
        MemoryMarshal.Write(_buff[GetOffset(1, _buff)..], ref value);
    }
    public readonly int seq_no => MemoryMarshal.Read<int>(_buff[GetOffset(2, _buff)..]);
    private void Set_seq_no(int value)
    {
        MemoryMarshal.Write(_buff[GetOffset(2, _buff)..], ref value);
    }
    public readonly int bytes => MemoryMarshal.Read<int>(_buff[GetOffset(3, _buff)..]);
    private void Set_bytes(int value)
    {
        MemoryMarshal.Write(_buff[GetOffset(3, _buff)..], ref value);
    }
    private static int GetOffset(int index, Span<byte> buffer)
    {
        int offset = 4;
        if(index >= 2) offset += 8;
        if(index >= 3) offset += 4;
        if(index >= 4) offset += 4;
        return offset;
    }
    public void Dispose()
    {
        _memory?.Dispose();
    }
}
