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
public class RecentMeUrlStickerSetImpl : RecentMeUrl
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public RecentMeUrlStickerSetImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1140172836;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_url);
            writer.Write(_set.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _url;
    public string Url
    {
        get => _url;
        set
        {
            serialized = false;
            _url = value;
        }
    }

    private StickerSetCovered _set;
    public StickerSetCovered Set
    {
        get => _set;
        set
        {
            serialized = false;
            _set = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _url = buff.ReadTLString();
        buff.Skip(4); _set  =  factory . Read < StickerSetCovered > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}