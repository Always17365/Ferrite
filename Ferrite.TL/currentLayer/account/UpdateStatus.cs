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
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.account;
public class UpdateStatus : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _account;
    private bool serialized = false;
    public UpdateStatus(ITLObjectFactory objectFactory, IAccountService account)
    {
        factory = objectFactory;
        _account = account;
    }

    public int Constructor => 1713919532;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(Bool.GetConstructor(_offline), true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private bool _offline;
    public bool Offline
    {
        get => _offline;
        set
        {
            serialized = false;
            _offline = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var success = await _account.UpdateStatus(ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId, !_offline);
        result.Result = success ? new BoolTrue() : new BoolFalse();
        return result;*/
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _offline = Bool.Read(ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}