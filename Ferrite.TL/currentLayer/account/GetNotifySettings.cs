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
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.TL.ObjectMapper;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.account;
public class GetNotifySettings : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _account;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public GetNotifySettings(ITLObjectFactory objectFactory, IAccountService account, IMapperContext mapper)
    {
        factory = objectFactory;
        _account = account;
        _mapper = mapper;
    }

    public int Constructor => 313765169;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_peer.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputNotifyPeer _peer;
    public InputNotifyPeer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        InputNotifyPeerDTO notifyPeer = _mapper.MapToDTO<InputNotifyPeer, InputNotifyPeerDTO>(_peer);
        
        var settings = await _account.GetNotifySettings(ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId, notifyPeer);
        var resp = factory.Resolve<PeerNotifySettingsImpl>();
        resp.ShowPreviews = settings.ShowPreviews;
        resp.Silent = settings.Silent;
        if (settings.MuteUntil > 0)
        {
            resp.MuteUntil = settings.MuteUntil;
        }

        if (settings.DeviceType == DeviceType.Android)
        {
            if (settings.NotifySoundType == NotifySoundType.Default)
            {
                resp.AndroidSound = factory.Resolve<NotificationSoundDefaultImpl>();
            }
            else if (settings.NotifySoundType == NotifySoundType.Ringtone)
            {
                var sound = factory.Resolve<NotificationSoundRingtoneImpl>();
                sound.Id = settings.Id;
                resp.AndroidSound = sound;
            }
            else if (settings.NotifySoundType != NotifySoundType.None)
            {
                resp.AndroidSound = factory.Resolve<NotificationSoundNoneImpl>();
            }
            else if (settings.NotifySoundType == NotifySoundType.Local)
            {
                var sound = factory.Resolve<NotificationSoundLocalImpl>();
                sound.Title = settings.Title;
                sound.Data = settings.Data;
                resp.AndroidSound = sound;
            }
        }
        else if (settings.DeviceType == DeviceType.iOS)
        {
            if (settings.NotifySoundType == NotifySoundType.Default)
            {
                resp.iOSSound = factory.Resolve<NotificationSoundDefaultImpl>();
            }
            else if (settings.NotifySoundType == NotifySoundType.Ringtone)
            {
                var sound = factory.Resolve<NotificationSoundRingtoneImpl>();
                sound.Id = settings.Id;
                resp.iOSSound = sound;
            }
            else if (settings.NotifySoundType != NotifySoundType.None)
            {
                resp.iOSSound = factory.Resolve<NotificationSoundNoneImpl>();
            }
            else if (settings.NotifySoundType == NotifySoundType.Local)
            {
                var sound = factory.Resolve<NotificationSoundLocalImpl>();
                sound.Title = settings.Title;
                sound.Data = settings.Data;
                resp.iOSSound = sound;
            }
        }
        else if (settings.DeviceType == DeviceType.Other)
        {
            if (settings.NotifySoundType == NotifySoundType.Default)
            {
                resp.OtherSound = factory.Resolve<NotificationSoundDefaultImpl>();
            }
            else if (settings.NotifySoundType == NotifySoundType.Ringtone)
            {
                var sound = factory.Resolve<NotificationSoundRingtoneImpl>();
                sound.Id = settings.Id;
                resp.OtherSound = sound;
            }
            else if (settings.NotifySoundType != NotifySoundType.None)
            {
                resp.OtherSound = factory.Resolve<NotificationSoundNoneImpl>();
            }
            else if (settings.NotifySoundType == NotifySoundType.Local)
            {
                var sound = factory.Resolve<NotificationSoundLocalImpl>();
                sound.Title = settings.Title;
                sound.Data = settings.Data;
                resp.OtherSound = sound;
            }
        }
        result.Result = resp;
        return result;*/
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _peer = (InputNotifyPeer)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}