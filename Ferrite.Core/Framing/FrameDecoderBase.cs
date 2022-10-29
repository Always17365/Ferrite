// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;
using Ferrite.Services;
using Ferrite.TL.currentLayer;

namespace Ferrite.Core;

public abstract class FrameDecoderBase : IFrameDecoder
{
    private const int StreamChunkSize = 1024;
    protected readonly byte[] LengthBytes = new byte[4];
    protected int Length;
    /// <summary>
    /// Number of length bytes to be skipped at the beginning of the frame before decoding it.
    /// </summary>
    protected int SkipLength;
    /// <summary>
    /// Number of bytes to be skipped at the beginning of the frame.
    /// </summary>
    protected int Header;
    /// <summary>
    /// Number of bytes to be skipped at the end of the frame.
    /// </summary>
    protected int Tail;
    private int _remaining;
    private bool _isStream;
    protected readonly Aes256Ctr? Decryptor;
    private readonly IMTProtoService _mtproto;
    private readonly byte[] _headerBytes = new byte[72];

    protected FrameDecoderBase(IMTProtoService mtproto)
    {
        _mtproto = mtproto;
    }

    protected FrameDecoderBase(Aes256Ctr decryptor, IMTProtoService mtproto)
    {
        Decryptor = decryptor;
        _mtproto = mtproto;
    }

    public bool Decode(ReadOnlySequence<byte> bytes, out ReadOnlySequence<byte> frame,
        out bool isStream, out bool requiresQuickAck, out SequencePosition position)
    {
        var reader = new SequenceReader<byte>(bytes);
        isStream = _isStream;
        requiresQuickAck = false;
        if (Length == 0)
        {
            requiresQuickAck = DecodeLength(ref reader, out var emptyFrame);
            if (emptyFrame) return EmptyFrame(out frame, out position, reader);
            _remaining = Length;
        }
        
        if (reader.Remaining >= 72 && !_isStream)
        {
            _isStream = CheckIfStream(reader.UnreadSequence.Slice(0, 72));
            isStream = _isStream;
        }
        
        int toBeWritten = Math.Min(_remaining, StreamChunkSize);
        if (_isStream && reader.Remaining >= toBeWritten)
        {
            return HandleStream(out frame, out position, reader, toBeWritten);
        }
        if (reader.Remaining < Length)
        {
            frame = new ReadOnlySequence<byte>();
            position = reader.Position;
            return false;
        }
        return HandleFrame(out frame, out position, reader);
    }

    /// <summary>
    /// Decodes the frame length.
    /// </summary>
    /// <param name="reader">Reader for the buffer.</param>
    /// <param name="emptyFrame">Is set if the received data length is too short.</param>
    /// <returns>If a quick ack is required.</returns>
    protected abstract bool DecodeLength(ref SequenceReader<byte> reader, out bool emptyFrame);

    protected bool CheckRequiresQuickAck(byte[] arr, int pos)
    {
        bool requiresQuickAck = false;
        if ((arr[pos] & 1 << 7) == 1 << 7)
        {
            requiresQuickAck = true;
            arr[pos] &= 0x7f;
        }

        return requiresQuickAck;
    }

    private static bool EmptyFrame(out ReadOnlySequence<byte> frame, out SequencePosition position, SequenceReader<byte> reader)
    {
        frame = new ReadOnlySequence<byte>();
        position = reader.Position;
        return false;
    }

    private bool HandleStream(out ReadOnlySequence<byte> frame, out SequencePosition position, SequenceReader<byte> reader,
        int toBeWritten)
    {
        ReadOnlySequence<byte> chunk = reader.UnreadSequence.Slice(0, toBeWritten);
        reader.Advance(toBeWritten);
        _remaining -= toBeWritten;
        if (Decryptor != null)
        {
            var chunkDecrypted = new byte[toBeWritten];
            Decryptor.Transform(chunk, chunkDecrypted);
            frame = new ReadOnlySequence<byte>(chunkDecrypted);
        }
        else
        {
            frame = chunk;
        }

        if (_remaining == 0)
        {
            Length = 0;
            _isStream = false;
            Array.Clear(LengthBytes);
            position = reader.Position;
            return false;
        }

        position = reader.Position;
        return true;
    }

    private bool HandleFrame(out ReadOnlySequence<byte> frame, out SequencePosition position, SequenceReader<byte> reader)
    {
        ReadOnlySequence<byte> data = reader.UnreadSequence.Slice(0, Length);
        reader.Advance(Length);
        Length = 0;
        Array.Clear(LengthBytes);
        if (Decryptor != null)
        {
            var frameDecrypted = new byte[data.Length];
            Decryptor.Transform(data, frameDecrypted.AsSpan()[SkipLength..]);
            frame = new ReadOnlySequence<byte>(frameDecrypted[(SkipLength + Header)..^Tail]);
        }
        else
        {
            frame = data;
        }

        if (_remaining != 0)
        {
            position = reader.Position;
            return true;
        }

        position = reader.Position;
        return false;
    }
    private bool CheckIfStream(ReadOnlySequence<byte> header)
    {
        SequenceReader reader;
        if (Decryptor != null)
        {
            Decryptor.TransformPeek(header, _headerBytes);
            reader = IAsyncBinaryReader.Create(_headerBytes);
        }
        else
        {
            header.CopyTo(_headerBytes);
            reader = IAsyncBinaryReader.Create(_headerBytes);
        }
        long authKeyId = reader.ReadInt64(true);
        var authKey = (_mtproto.GetAuthKey(authKeyId) ?? 
                       _mtproto.GetTempAuthKey(authKeyId));
        if (authKey is { Length: > 0 })
        {
            Span<byte> messageKey = stackalloc byte[16];
            reader.Read(messageKey);
            AesIge aesIge = new AesIge(authKey, messageKey);
            Span<byte> messageHeader = stackalloc byte[48];
            reader.Read(messageHeader);
            aesIge.Decrypt(messageHeader);
            SpanReader<byte> sr = new SpanReader<byte>(messageHeader);
            sr.Advance(32);
            int constructor = sr.ReadInt32(true);
            if (constructor == TLConstructor.Upload_SaveFilePart ||
                constructor == TLConstructor.Upload_SaveBigFilePart)
            {
                return true;
            }
        }
        return false;
    }
}