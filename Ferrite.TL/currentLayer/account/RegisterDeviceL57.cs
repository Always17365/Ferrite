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
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.account;
public class RegisterDeviceL57 : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _account;
    private bool serialized = false;
    public RegisterDeviceL57(ITLObjectFactory objectFactory, IAccountService account)
    {
        factory = objectFactory;
        _account = account;
    }

    public int Constructor => unchecked((int)0x637ea878);
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_tokenType, true);
            writer.WriteTLString(_token);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }
    
    private int _tokenType;
    public int TokenType
    {
        get => _tokenType;
        set
        {
            serialized = false;
            _tokenType = value;
        }
    }

    private string _token;
    public string Token
    {
        get => _token;
        set
        {
            serialized = false;
            _token = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var success = await _account.RegisterDevice(new DeviceInfoDTO()
        {
            AuthKeyId = ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId,
            Secret = Array.Empty<byte>(),
            Token = _token,
            TokenType = _tokenType,
            OtherUserIds = Array.Empty<long>()
        });
        result.Result = success ? new BoolTrue() : new BoolFalse();
        return result;*/
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _tokenType = buff.ReadInt32(true);
        _token = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}