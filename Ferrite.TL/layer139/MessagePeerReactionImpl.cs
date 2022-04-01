/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Utils;

namespace Ferrite.TL.layer139;
public class MessagePeerReactionImpl : MessagePeerReaction
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MessagePeerReactionImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1370914559;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_peerId.TLBytes, false);
            writer.WriteTLString(_reaction);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Flags _flags;
    public Flags Flags
    {
        get => _flags;
        set
        {
            serialized = false;
            _flags = value;
        }
    }

    public bool Big
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Unread
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    private Peer _peerId;
    public Peer PeerId
    {
        get => _peerId;
        set
        {
            serialized = false;
            _peerId = value;
        }
    }

    private string _reaction;
    public string Reaction
    {
        get => _reaction;
        set
        {
            serialized = false;
            _reaction = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        buff.Skip(4); _peerId  =  factory . Read < Peer > ( ref  buff ) ; 
        _reaction = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}