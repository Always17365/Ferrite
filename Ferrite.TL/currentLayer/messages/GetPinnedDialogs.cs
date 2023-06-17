﻿/*
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
using Ferrite.Services;
using Ferrite.TL.currentLayer.updates;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.messages;
public class GetPinnedDialogs : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IUpdatesService _updates;
    private bool serialized = false;
    public GetPinnedDialogs(ITLObjectFactory objectFactory, IUpdatesService updates)
    {
        factory = objectFactory;
        _updates = updates;
    }

    public int Constructor => -692498958;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_folderId, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _folderId;
    public int FolderId
    {
        get => _folderId;
        set
        {
            serialized = false;
            _folderId = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var resp = factory.Resolve<PeerDialogsImpl>();
        resp.Chats = new Vector<Chat>(factory);
        resp.Dialogs = new Vector<Dialog>(factory);
        resp.Messages = new Vector<Message>(factory);
        resp.Users = new Vector<User>(factory);
        var state = await _updates.GetState(ctx.CurrentAuthKeyId);
        var currentState = factory.Resolve<StateImpl>();
        currentState.Date = state.Date;
        currentState.Pts = state.Pts;
        currentState.Qts = state.Qts;
        currentState.Seq = state.Seq;
        currentState.UnreadCount = state.UnreadCount;
        resp.State = currentState;
        result.Result = resp;
        return result;*/
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _folderId = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}