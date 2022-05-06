﻿/*
 *   Project Ferrite is an Implementation of the Telegram Server API
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
namespace Ferrite.Data;
public interface IPersistentStore
{
    public Task<bool> SaveAuthKeyAsync(long authKeyId, byte[] authKey);
    public Task<byte[]?> GetAuthKeyAsync(long authKeyId);
    public Task<bool> SaveExportedAuthorizationAsync(AuthInfo info, int previousDc, int nextDc, byte[] data);
    public Task<ExportedAuthInfo?> GetExportedAuthorizationAsync(long user_id, long auth_key_id);
    public Task<bool> SaveAuthorizationAsync(AuthInfo info);
    public Task<AuthInfo?> GetAuthorizationAsync(long authKeyId);
    public Task<ICollection<AuthInfo>> GetAuthorizationsAsync(string phone);
    public Task<bool> DeleteAuthorizationAsync(long authKeyId);
    public Task<bool> DeleteAuthKeyAsync(long authKeyId);
    public Task<bool> SaveServerSaltAsync(long authKeyId, long serverSalt, long validSince, int TTL);
    public Task<ICollection<ServerSalt>> GetServerSaltsAsync(long authKeyId, int count);
    public Task<bool> SaveUserAsync(User user);
    public Task<bool> UpdateUserAsync(User user);
    public Task<User?> GetUserAsync(long userId);
    public Task<User?> GetUserAsync(string phone);
    public Task<User?> GetUserByUsernameAsync(string username);
    public Task<bool> SaveAppInfoAsync(AppInfo appInfo);
    public Task<AppInfo?> GetAppInfoAsync(long authKeyId);
    public Task<bool> SaveDeviceInfoAsync(DeviceInfo deviceInfo);
    public Task<DeviceInfo?> GetDeviceInfoAsync(long authKeyId);
    public Task<bool> DeleteDeviceInfoAsync(long authKeyId, string token, ICollection<long> otherUserIds);

}

