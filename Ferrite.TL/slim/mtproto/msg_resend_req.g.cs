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

public readonly ref struct msg_resend_req
{
    private readonly Span<byte> _buff;
    private readonly IMemoryOwner<byte>? _memory;
    public msg_resend_req(VectorOfLong msg_ids)
    {
        var length = GetRequiredBufferSize(msg_ids.Length);
        _memory = UnmanagedMemoryPool<byte>.Shared.Rent(length);
        _memory.Memory.Span.Clear();
        _buff = _memory.Memory.Span[..length];
        SetConstructor(unchecked((int)0x7d861a08));
        Set_msg_ids(msg_ids.ToReadOnlySpan());
    }public msg_resend_req(Span<byte> buff)
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
        var bytesRead = GetOffset(2, data[offset..]);
        if (bytesRead > data.Length + offset)
        {
            return Span<byte>.Empty;
        }
        return data.Slice(offset, bytesRead);
    }

    public static int GetRequiredBufferSize(int len_msg_ids)
    {
        return 4 + len_msg_ids;
    }
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(2, data[offset..]);
    }
    public VectorOfLong msg_ids => new VectorOfLong(_buff.Slice(GetOffset(1, _buff)));
    private void Set_msg_ids(ReadOnlySpan<byte> value)
    {
        value.CopyTo(_buff[GetOffset(1, _buff)..]);
    }
    private static int GetOffset(int index, Span<byte> buffer)
    {
        int offset = 4;
        if(index >= 2) offset += VectorOfLong.ReadSize(buffer, offset);
        return offset;
    }
    public void Dispose()
    {
        _memory?.Dispose();
    }
}
