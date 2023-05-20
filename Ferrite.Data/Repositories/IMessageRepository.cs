// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using Ferrite.TL.slim.baseLayer;
using Ferrite.TL.slim.baseLayer.dto;

namespace Ferrite.Data.Repositories;

public interface IMessageRepository
{
    public bool PutMessage(long userId, TLMessage message, int pts);
    public IReadOnlyCollection<TLSavedMessage> GetMessages(long userId, TLInputPeer? peerId = null);
    public ValueTask<IReadOnlyCollection<TLSavedMessage>> GetMessagesAsync(long userId, TLInputPeer? peerId = null);
    public IReadOnlyCollection<TLSavedMessage> GetMessages(long userId, int pts, int maxPts, DateTimeOffset date);
    public ValueTask<IReadOnlyCollection<TLSavedMessage>> GetMessagesAsync(long userId, int pts, int maxPts, DateTimeOffset date);
    public TLSavedMessage? GetMessage(long userId, int messageId);
    public ValueTask<TLSavedMessage?> GetMessageAsync(long userId, int messageId);
    public bool DeleteMessage(long userId, int id);
    public ValueTask<bool> DeleteMessageAsync(long userId, int id);
}