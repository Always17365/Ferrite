﻿//
//  Project Ferrite is an Implementation Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Buffers;
using Ferrite.Core.Connection;
using Ferrite.Core.Execution;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.currentLayer.photos;
using Ferrite.TL.currentLayer.upload;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;
using Ferrite.Utils;
using MessagePack;
using Message = Ferrite.TL.mtproto.Message;
using VectorOfLong = Ferrite.TL.VectorOfLong;

namespace Ferrite.Core.RequestChain;

public class MTProtoRequestProcessor : ILinkedHandler
{
    private readonly ITLObjectFactory _factory;
    private readonly ISessionService _sessionManager;
    private readonly IMessagePipe _pipe;
    private readonly IExecutionEngine _api;
    private readonly ILogger _log;
    public MTProtoRequestProcessor(ITLObjectFactory factory, 
        ISessionService sessionManager, IMessagePipe pipe, 
        IExecutionEngine api, ILogger log)
    {
        _factory = factory;
        _sessionManager = sessionManager;
        _pipe = pipe;
        _api = api;
        _log = log;
    }
    
    public ILinkedHandler SetNext(ILinkedHandler value)
    {
        Next = value;
        return Next;
    }

    public ILinkedHandler? Next { get; set; }

    public async ValueTask Process(object? sender, ITLObject input, TLExecutionContext ctx)
    {
        if (input is ITLMethod method)
        {
            try
            {
                if (input is SaveFilePart or SaveBigFilePart or UploadProfilePhoto)
                {
                    var ack = _factory.Resolve<MsgsAck>();
                    ack.MsgIds = new VectorOfLong(1);
                    ack.MsgIds.Add(ctx.MessageId);
                    Services.MTProtoMessage message = new Services.MTProtoMessage();
                    message.SessionId = ctx.SessionId;
                    message.IsResponse = true;
                    message.IsContentRelated = true;
                    message.Data = ack.TLBytes.ToArray();
                    if(sender is MTProtoConnection connection)
                    {
                        await connection.SendAsync(message);
                    }
                }
                var result = await method.ExecuteAsync(ctx);
                if (result != null)
                {
                    _log.Debug($"Result for {input} is {result} Processed with AuthKeyId: {ctx.AuthKeyId}");
                    Services.MTProtoMessage message = new Services.MTProtoMessage();
                    message.SessionId = ctx.SessionId;
                    message.IsResponse = true;
                    message.IsContentRelated = true;
                    message.Data = result.TLBytes.ToArray();
                    if (sender != null)
                    {
                        await ((MTProtoConnection)sender).SendAsync(message);
                    }
                    else if (await _sessionManager.GetSessionStateAsync(ctx.SessionId)
                             is RemoteSession session)
                    {
                        var bytes = MessagePackSerializer.Serialize(message);
                        _ = _pipe.WriteMessageAsync(session.NodeId.ToString(), bytes);
                    }

                    Console.WriteLine("-->" + result.ToString());
                }
                else
                {
                    _log.Debug($"Result for {input} was NULL Processed with AuthKeyId: {ctx.AuthKeyId}");
                }
            }
            catch (Exception e)
            {
                _log.Error(e, $"😭 => {this} => {input} => {e.Message}");
            }
        }
        if (input is Message { Body: ITLMethod encMethod } msg)
        {
            var _context = new TLExecutionContext(ctx.SessionData)
            {
                MessageId = msg.MsgId,
                AuthKeyId = ctx.AuthKeyId,
                PermAuthKeyId = ctx.PermAuthKeyId,
                SequenceNo = msg.Seqno,
                Salt = ctx.Salt,
                SessionId = ctx.SessionId
            };
            try
            {
                var result = await encMethod.ExecuteAsync(_context);
                if (result != null)
                {
                    _log.Debug($"Result for {encMethod} is {result} Processed with AuthKeyId: {ctx.AuthKeyId}");
                    Services.MTProtoMessage message = new Services.MTProtoMessage();
                    message.SessionId = ctx.SessionId;
                    message.IsResponse = true;
                    message.IsContentRelated = true;
                    message.Data = result.TLBytes.ToArray();

                    if (sender != null)
                    {
                        await ((MTProtoConnection)sender).SendAsync(message);
                    }
                    else if (await _sessionManager.GetSessionStateAsync(ctx.SessionId)
                             is RemoteSession session)
                    {
                        var bytes = MessagePackSerializer.Serialize(message);
                        _ = _pipe.WriteMessageAsync(session.NodeId.ToString(), bytes);
                    }
                }
                else
                {
                    _log.Debug($"Result for {encMethod} was NULL Processed with AuthKeyId: {ctx.AuthKeyId}");
                }
            }
            catch (Exception e)
            {
                _log.Error(e, $"😭 => {this} => {msg.Body} => {e.Message}");
            }
        }
        else if (input is GetFile getFileRequest)
        {
            var result = await getFileRequest.ExecuteAsync(ctx);
            _log.Debug($"Result for {input} is {result} Processed with AuthKeyId: {ctx.AuthKeyId}");
            
            if (result.Success && sender != null)
            {
                await ((MTProtoConnection)sender).SendAsync(result.File);
            }
            else if (sender != null)
            {
                MTProtoMessage message = new()
                {
                    MessageType = MTProtoMessageType.Encrypted,
                    SessionId = ctx.SessionId,
                    IsResponse = true,
                    IsContentRelated = true,
                    Data = result.Error?.TLBytes.ToArray()
                };
                await ((MTProtoConnection)sender).SendAsync(message);
            }
        }
        else if (input is Message { Body: GetFile getFileRequest2 })
        {
            var result = await getFileRequest2.ExecuteAsync(ctx);
            _log.Debug($"Result for {input} is {result} Processed with AuthKeyId: {ctx.AuthKeyId}");
            
            if (result.Success && sender != null)
            {
                await ((MTProtoConnection)sender).SendAsync(result.File);
            }
            else if (sender != null)
            {
                MTProtoMessage message = new()
                {
                    MessageType = MTProtoMessageType.Encrypted,
                    SessionId = ctx.SessionId,
                    IsResponse = true,
                    IsContentRelated = true,
                    Data = result.Error?.TLBytes.ToArray()
                };
                await ((MTProtoConnection)sender).SendAsync(message);
            }
        }
        else
        {
            if (Next != null) await Next.Process(sender, input, ctx);
        }
    }

    public async ValueTask Process(object? sender, TLBytes input, TLExecutionContext ctx)
    {
        var result = await _api.Invoke(input, ctx);
        if (result!=null && sender != null)
        {
            MTProtoMessage message = new()
            {
                MessageType = MTProtoMessageType.Encrypted,
                SessionId = ctx.SessionId,
                IsResponse = true,
                IsContentRelated = true,
                Data = result.Value.AsSpan().ToArray()
            };
            await ((MTProtoConnection)sender).SendAsync(message);
        }
        if (Next != null) await Next.Process(sender, input, ctx);
        else input.Dispose();
    }
}

