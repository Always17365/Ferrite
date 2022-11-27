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
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.auth;
public class ResendCode : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAuthService _auth;
    private bool serialized = false;
    public ResendCode(ITLObjectFactory objectFactory, IAuthService auth)
    {
        factory = objectFactory;
        _auth = auth;
    }

    public int Constructor => 1056025023;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_phoneNumber);
            writer.WriteTLString(_phoneCodeHash);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _phoneNumber;
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            serialized = false;
            _phoneNumber = value;
        }
    }

    private string _phoneCodeHash;
    public string PhoneCodeHash
    {
        get => _phoneCodeHash;
        set
        {
            serialized = false;
            _phoneCodeHash = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var sent = await _auth.ResendCode(_phoneNumber, _phoneCodeHash);
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        if (sent == null)
        {
            var error = factory.Resolve<RpcError>();
            error.ErrorCode = 500;
            error.ErrorMessage = "Internal Server Error";
            result.Result = error;
            return result;
        }
        var sentCode = factory.Resolve<SentCodeImpl>();
        var codeType = factory.Resolve<SentCodeTypeSmsImpl>();
        var nextType = factory.Resolve<CodeTypeSmsImpl>();
        sentCode.NextType = nextType;
        sentCode.PhoneCodeHash = sent.PhoneCodeHash;
        sentCode.Timeout = sent.Timeout;
        sentCode.Type = codeType;
        result.Result = sentCode;
        return result;*/
        return null;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _phoneNumber = buff.ReadTLString();
        _phoneCodeHash = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}