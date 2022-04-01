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

namespace Ferrite.TL.layer139.phone;
public class GetGroupParticipants : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GetGroupParticipants(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -984033109;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_call.TLBytes, false);
            writer.Write(_ids.TLBytes, false);
            writer.Write(_sources.TLBytes, false);
            writer.WriteTLString(_offset);
            writer.WriteInt32(_limit, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputGroupCall _call;
    public InputGroupCall Call
    {
        get => _call;
        set
        {
            serialized = false;
            _call = value;
        }
    }

    private Vector<InputPeer> _ids;
    public Vector<InputPeer> Ids
    {
        get => _ids;
        set
        {
            serialized = false;
            _ids = value;
        }
    }

    private VectorOfInt _sources;
    public VectorOfInt Sources
    {
        get => _sources;
        set
        {
            serialized = false;
            _sources = value;
        }
    }

    private string _offset;
    public string Offset
    {
        get => _offset;
        set
        {
            serialized = false;
            _offset = value;
        }
    }

    private int _limit;
    public int Limit
    {
        get => _limit;
        set
        {
            serialized = false;
            _limit = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _call  =  factory . Read < InputGroupCall > ( ref  buff ) ; 
        buff.Skip(4); _ids  =  factory . Read < Vector < InputPeer > > ( ref  buff ) ; 
        buff.Skip(4); _sources  =  factory . Read < VectorOfInt > ( ref  buff ) ; 
        _offset = buff.ReadTLString();
        _limit = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}