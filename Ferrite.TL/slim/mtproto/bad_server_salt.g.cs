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

public readonly ref struct bad_server_salt
{
    private readonly Span<byte> _buff;
    private readonly IMemoryOwner<byte>? _memory;
    public bad_server_salt(long bad_msg_id, int bad_msg_seqno, int error_code, long new_server_salt)
    {
        var length = GetRequiredBufferSize();
        _memory = UnmanagedMemoryPool<byte>.Shared.Rent(length);
        _memory.Memory.Span.Clear();
        _buff = _memory.Memory.Span[..length];
        SetConstructor(unchecked((int)0xedab447b));
        Set_bad_msg_id(bad_msg_id);
        Set_bad_msg_seqno(bad_msg_seqno);
        Set_error_code(error_code);
        Set_new_server_salt(new_server_salt);
    }public bad_server_salt(Span<byte> buff)
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
        var bytesRead = GetOffset(5, data[offset..]);
        if (bytesRead > data.Length + offset)
        {
            return Span<byte>.Empty;
        }
        return data.Slice(offset, bytesRead);
    }

    public static int GetRequiredBufferSize()
    {
        return 4 + 8 + 4 + 4 + 8;
    }
    public static int ReadSize(Span<byte> data, int offset)
    {
        return GetOffset(5, data[offset..]);
    }
    public readonly long bad_msg_id => MemoryMarshal.Read<long>(_buff[GetOffset(1, _buff)..]);
    private void Set_bad_msg_id(long value)
    {
        MemoryMarshal.Write(_buff[GetOffset(1, _buff)..], ref value);
    }
    public readonly int bad_msg_seqno => MemoryMarshal.Read<int>(_buff[GetOffset(2, _buff)..]);
    private void Set_bad_msg_seqno(int value)
    {
        MemoryMarshal.Write(_buff[GetOffset(2, _buff)..], ref value);
    }
    public readonly int error_code => MemoryMarshal.Read<int>(_buff[GetOffset(3, _buff)..]);
    private void Set_error_code(int value)
    {
        MemoryMarshal.Write(_buff[GetOffset(3, _buff)..], ref value);
    }
    public readonly long new_server_salt => MemoryMarshal.Read<long>(_buff[GetOffset(4, _buff)..]);
    private void Set_new_server_salt(long value)
    {
        MemoryMarshal.Write(_buff[GetOffset(4, _buff)..], ref value);
    }
    private static int GetOffset(int index, Span<byte> buffer)
    {
        int offset = 4;
        if(index >= 2) offset += 8;
        if(index >= 3) offset += 4;
        if(index >= 4) offset += 4;
        if(index >= 5) offset += 8;
        return offset;
    }
    public void Dispose()
    {
        _memory?.Dispose();
    }
}
