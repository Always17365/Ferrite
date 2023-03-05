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
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.TL.ObjectMapper;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.contacts;
public class Unblock : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IContactsService _contacts;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public Unblock(ITLObjectFactory objectFactory, IContactsService contacts, IMapperContext mapper)
    {
        factory = objectFactory;
        _contacts = contacts;
        _mapper = mapper;
    }

    public int Constructor => -1096393392;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_id.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputPeer _id;
    public InputPeer Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var success =
            await _contacts.Unblock(ctx.PermAuthKeyId != 0 ? ctx.PermAuthKeyId : ctx.AuthKeyId,
                _mapper.MapToDTO<InputPeer, InputPeerDTO>(_id));
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = success ? new BoolTrue() : new BoolFalse();
        return result;*/
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _id = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}