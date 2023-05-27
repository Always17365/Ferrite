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

namespace Ferrite.TL.currentLayer.messages;
public class SendMessage : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IMessagesService _messages;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public SendMessage(ITLObjectFactory objectFactory, IMessagesService messages, IMapperContext mapper)
    {
        factory = objectFactory;
        _messages = messages;
        _mapper = mapper;
    }
    public int Constructor => 228423076;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_peer.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteInt32(_replyToMsgId, true);
            }

            writer.WriteTLString(_message);
            writer.WriteInt64(_randomId, true);
            if (_flags[2])
            {
                writer.Write(_replyMarkup.TLBytes, false);
            }

            if (_flags[3])
            {
                writer.Write(_entities.TLBytes, false);
            }

            if (_flags[10])
            {
                writer.WriteInt32(_scheduleDate, true);
            }

            if (_flags[13])
            {
                writer.Write(_sendAs.TLBytes, false);
            }

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

    public bool NoWebpage
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool Silent
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool Background
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    public bool ClearDraft
    {
        get => _flags[7];
        set
        {
            serialized = false;
            _flags[7] = value;
        }
    }

    public bool Noforwards
    {
        get => _flags[14];
        set
        {
            serialized = false;
            _flags[14] = value;
        }
    }

    private InputPeer _peer;
    public InputPeer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private int _replyToMsgId;
    public int ReplyToMsgId
    {
        get => _replyToMsgId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _replyToMsgId = value;
        }
    }

    private string _message;
    public string Message
    {
        get => _message;
        set
        {
            serialized = false;
            _message = value;
        }
    }

    private long _randomId;
    public long RandomId
    {
        get => _randomId;
        set
        {
            serialized = false;
            _randomId = value;
        }
    }

    private ReplyMarkup _replyMarkup;
    public ReplyMarkup ReplyMarkup
    {
        get => _replyMarkup;
        set
        {
            serialized = false;
            _flags[2] = true;
            _replyMarkup = value;
        }
    }

    private Vector<MessageEntity> _entities;
    public Vector<MessageEntity> Entities
    {
        get => _entities;
        set
        {
            serialized = false;
            _flags[3] = true;
            _entities = value;
        }
    }

    private int _scheduleDate;
    public int ScheduleDate
    {
        get => _scheduleDate;
        set
        {
            serialized = false;
            _flags[10] = true;
            _scheduleDate = value;
        }
    }

    private InputPeer _sendAs;
    public InputPeer SendAs
    {
        get => _sendAs;
        set
        {
            serialized = false;
            _flags[13] = true;
            _sendAs = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var peer = _mapper.MapToDTO<InputPeer, InputPeerDTO>(_peer);
        var entities = new List<MessageEntityDTO>();
        if (_entities != null)
        {
            foreach (var entity in _entities)
            {
                entities.Add(_mapper.MapToDTO<MessageEntity, MessageEntityDTO>(entity));
            }
        }
        var serviceResult = await _messages.SendMessage(ctx.CurrentAuthKeyId,
            NoWebpage, Silent, Background, ClearDraft, Noforwards,
            peer, _message, _randomId, _flags[0] ? _replyToMsgId : null,
            _flags[2] ? _mapper.MapToDTO<ReplyMarkup, ReplyMarkupDTO>(_replyMarkup): null, 
            _flags[3] ? entities : null, _flags[10] ? _scheduleDate : null, 
            _flags[13] ? _mapper.MapToDTO<InputPeer, InputPeerDTO>(_sendAs) : null);
        var rpcResult = factory.Resolve<RpcResult>();
        rpcResult.ReqMsgId = ctx.MessageId;
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            rpcResult.Result = err;
        }
        else
        {
            var updates = factory.Resolve<UpdateShortSentMessageImpl>();
            updates.Out = serviceResult.Result.Out;
            updates.Id = serviceResult.Result.Id;
            updates.Pts = serviceResult.Result.Pts;
            updates.PtsCount = serviceResult.Result.PtsCount;
            updates.Date = serviceResult.Result.Date;
            rpcResult.Result = updates;
        }
        return rpcResult;*/
        throw new NotImplementedException();
    }
    
    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _replyToMsgId = buff.ReadInt32(true);
        }

        _message = buff.ReadTLString();
        _randomId = buff.ReadInt64(true);
        if (_flags[2])
        {
            _replyMarkup = (ReplyMarkup)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[3])
        {
            buff.Skip(4);
            _entities = factory.Read<Vector<MessageEntity>>(ref buff);
        }

        if (_flags[10])
        {
            _scheduleDate = buff.ReadInt32(true);
        }

        if (_flags[13])
        {
            _sendAs = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}