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
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;
using Ferrite.TL.Exceptions;
using Ferrite.Utils;

namespace Ferrite.TL.mtproto;
public class ReqDhParams : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public int Constructor => -686627650;
    private IKeyProvider keyProvider;
    private ILogger log;
    private IRandomGenerator random;
    //private readonly int[] gs = new int[] { 2, 3, 4, 5, 6, 7 };
    private readonly int[] gs = new int[] { 3, 4, 7 };
    //TODO: Maybe change the DH_PRIME
    private const string dhPrime = "C71CAEB9C6B1C9048E6C522F70F13F73980D40238E3E21C14934D037563D930F48198A0AA7C14058229493D22530F4DBFA336F6E0AC925139543AED44CCE7C3720FD51F69458705AC68CD4FE6B6B13ABDC9746512969328454F18FAF8C595F642477FE96BB2A941D5BCD1D4AC8CC49880708FA9B378E3C4F3A9060BEE67CF9A4A4A695811051907E162753B56B0F6B410DBA74D8A84B2A14B3144E0EF1284754FD17ED950D5965B4B9DD46582DB1178D169C6BC465B0D6FF9CA3928FEF5B9AE4E418FC15E83EBEA0F87FA9FF5EED70050DED2849F47BF959D956850CE929851F0D8115F635B105EE2E4E15D04B2454BF6F4FADF034B10403119CD8E3B92FCC5B";
    public ReqDhParams(ITLObjectFactory objectFactory, IKeyProvider provider,
        IRandomGenerator generator, ILogger logger)
    {
        factory = objectFactory;
        keyProvider = provider;
        random = generator;
        this.log = logger;
    }
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(nonce.TLBytes, false);
            writer.Write(serverNonce.TLBytes, false);
            writer.WriteTLBytes(p);
            writer.WriteTLBytes(q);
            writer.WriteInt64(publicKeyFingerprint, true);
            writer.WriteTLBytes(encryptedData);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Int128 nonce;
    public Int128 Nonce
    {
        get => nonce;
        set
        {
            serialized = false;
            nonce = value;
        }
    }

    private Int128 serverNonce;
    public Int128 ServerNonce
    {
        get => serverNonce;
        set
        {
            serialized = false;
            serverNonce = value;
        }
    }

    private byte[] p;
    public byte[] P
    {
        get => p;
        set
        {
            serialized = false;
            p = value;
        }
    }

    private byte[] q;
    public byte[] Q
    {
        get => q;
        set
        {
            serialized = false;
            q = value;
        }
    }

    private long publicKeyFingerprint;
    public long PublicKeyFingerprint
    {
        get => publicKeyFingerprint;
        set
        {
            serialized = false;
            publicKeyFingerprint = value;
        }
    }

    private byte[] encryptedData;
    public byte[] EncryptedData
    {
        get => encryptedData;
        set
        {
            serialized = false;
            encryptedData = value;
        }
    }
    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        RpcError rpcError;
        ServerDhParamsOk serverDhParamsOk = factory.Resolve<ServerDhParamsOk>();
        var rsaKey = keyProvider.GetKey(this.publicKeyFingerprint);
        if (rsaKey == null)
        {
            log.Debug("Could not obtain the RSA Key.");
            rpcError = factory.Resolve<RpcError>();
            rpcError.ErrorCode = -404;
            rpcError.ErrorMessage = "";
            return rpcError;
        }
        if(!ctx.SessionData.ContainsKey("nonce") || 
                !ctx.SessionData.ContainsKey("server_nonce"))
        {
            log.Debug("Could not obtain the RSA Key.");
            rpcError = factory.Resolve<RpcError>();
            rpcError.ErrorCode = -404;
            rpcError.ErrorMessage = "";
            return rpcError;
        }
        Memory<byte> data;
        byte[] sha256;
        RSAPad(rsaKey, out data, out sha256);

        if (!sha256.SequenceEqual(data.Slice(224).ToArray()))
        {
            log.Debug("SHA256 did not match.");
            rpcError = factory.Resolve<RpcError>();
            rpcError.ErrorCode = -404;
            return rpcError;
        }

        SequenceReader reader = IAsyncBinaryReader.Create(data.Slice(32, 192));

        int constructor = reader.ReadInt32(true);
        var sessionNonce = (Int128)(byte[])ctx.SessionData["nonce"];
        var sessionServerNonce = (Int128)(byte[])ctx.SessionData["server_nonce"];
        if (constructor == TLConstructor.PQInnerData)
        {
            var obj = factory.Read<PQInnerDataDc>(ref reader);
            ctx.SessionData.Add("new_nonce", (byte[])obj.NewNonce);
            if (nonce != obj.Nonce ||
                nonce != sessionNonce ||
                serverNonce != obj.ServerNonce ||
                serverNonce != sessionServerNonce)
            {
                rpcError = factory.Resolve<RpcError>();
                rpcError.ErrorCode = -404;
                rpcError.ErrorMessage = "Nonce values did not match.";
                return rpcError;
            }
            var newNonceServerNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var newNonceNewNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat(((byte[])obj.NewNonce).SkipLast(28)).ToArray();
            ctx.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
            byte[] answer = GenerateEncryptedAnswer(ctx, serverDhParamsOk, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            serverDhParamsOk.EncryptedAnswer = answer;

            return serverDhParamsOk;
        }
        else if (constructor == TLConstructor.PQInnerDataDc)
        {
            var obj = factory.Read<PQInnerDataDc>(ref reader);
            ctx.SessionData.Add("new_nonce", (byte[])obj.NewNonce);
            if (nonce != obj.Nonce ||
                nonce != sessionNonce ||
                serverNonce != obj.ServerNonce ||
                serverNonce != sessionServerNonce)
            {
                rpcError = factory.Resolve<RpcError>();
                rpcError.ErrorCode = -404;
                return rpcError;
            }
            var newNonceServerNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var newNonceNewNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat(((byte[])obj.NewNonce).SkipLast(28)).ToArray();
            ctx.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
            byte[] answer = GenerateEncryptedAnswer(ctx, serverDhParamsOk, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            serverDhParamsOk.EncryptedAnswer = answer;

            return serverDhParamsOk;
        }
        else if (constructor == TLConstructor.PQInnerDataTempDc)
        {
            ctx.SessionData.Add("temp_auth_key", true);
            var obj = factory.Read<PQInnerDataTempDc>(ref reader);
            ctx.SessionData.Add("temp_auth_key_expires_in", obj.ExpiresIn);
            ctx.SessionData.Add("new_nonce", (byte[])obj.NewNonce);
            if (nonce != obj.Nonce ||
                nonce != sessionNonce ||
                serverNonce != obj.ServerNonce ||
                serverNonce != sessionServerNonce)
            {
                rpcError = factory.Resolve<RpcError>();
                rpcError.ErrorCode = -404;
                return rpcError;
            }
            var newNonceServerNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var newNonceNewNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat(((byte[])obj.NewNonce).Take(4)).ToArray();
            ctx.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
            byte[] answer = GenerateEncryptedAnswer(ctx, serverDhParamsOk, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            serverDhParamsOk.EncryptedAnswer = answer;
            ctx.SessionData.Add("valid_until", DateTime.Now.AddSeconds(obj.ExpiresIn));
            return serverDhParamsOk;
        }
        else if (constructor == TLConstructor.PQInnerDataTemp)
        {
            ctx.SessionData.Add("temp_auth_key", true);
            var obj = factory.Read<PQInnerDataTemp>(ref reader);
            ctx.SessionData.Add("temp_auth_key_expires_in", obj.ExpiresIn);
            ctx.SessionData.Add("new_nonce", (byte[])obj.NewNonce);
            if (nonce != obj.Nonce ||
                nonce != sessionNonce ||
                serverNonce != obj.ServerNonce ||
                serverNonce != sessionServerNonce)
            {
                rpcError = factory.Resolve<RpcError>();
                rpcError.ErrorCode = -404;
                return rpcError;
            }
            var newNonceServerNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])sessionServerNonce).ToArray());
            var serverNonceNewNonce = SHA1.HashData(((byte[])sessionServerNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var newNonceNewNonce = SHA1.HashData(((byte[])obj.NewNonce)
                .Concat((byte[])obj.NewNonce).ToArray());
            var tmpAesKey = newNonceServerNonce
                .Concat(serverNonceNewNonce.SkipLast(8)).ToArray();
            var tmpAesIV = serverNonceNewNonce.Skip(12)
                .Concat(newNonceNewNonce).Concat(((byte[])obj.NewNonce).Take(4)).ToArray();
            ctx.SessionData.Add("temp_aes_key", tmpAesKey.ToArray());
            ctx.SessionData.Add("temp_aes_iv", tmpAesIV.ToArray());
            byte[] answer = GenerateEncryptedAnswer(ctx, serverDhParamsOk, sessionNonce, sessionServerNonce, tmpAesKey, tmpAesIV);
            serverDhParamsOk.EncryptedAnswer = answer;
            ctx.SessionData.Add("valid_until", DateTime.Now.AddSeconds(obj.ExpiresIn));
            return serverDhParamsOk;
        }

        return serverDhParamsOk;
    }
    
    private byte[] GenerateEncryptedAnswer(TLExecutionContext ctx, ServerDhParamsOk serverDhParamsOk, Int128 sessionNonce, Int128 sessionServerNonce, byte[] tmpAesKey, byte[] tmpAesIV)
    {
        serverDhParamsOk.Nonce = sessionNonce;
        serverDhParamsOk.ServerNonce = sessionServerNonce;

        var serverDhInnerData = factory.Resolve<ServerDhInnerData>();
        BigInteger prime = BigInteger.Parse("0"+dhPrime, NumberStyles.HexNumber);
        BigInteger min = BigInteger.Pow(new BigInteger(2), 2048 - 64);
        BigInteger max = BigInteger.Subtract(prime, min);
        BigInteger a = random.GetRandomInteger(2, BigInteger.Subtract(prime, 2));
        BigInteger g = new BigInteger(gs[random.GetRandomNumber(gs.Length)]);
        BigInteger g_a = BigInteger.ModPow(g, a, prime);
        while (g_a.CompareTo(min) <= 0 || g_a.CompareTo(max) >= 0)
        {
            a = random.GetRandomInteger(2, BigInteger.Subtract(prime, 2));
            g_a = BigInteger.ModPow(g, a, prime);
        }
        
        serverDhInnerData.Nonce = sessionNonce;
        serverDhInnerData.ServerNonce = sessionServerNonce;
        serverDhInnerData.DhPrime = prime.ToByteArray(true,true);
        serverDhInnerData.G = (int)g;
        serverDhInnerData.GA = g_a.ToByteArray(true, true);
        serverDhInnerData.ServerTime = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
        
        ctx.SessionData.Add("g", serverDhInnerData.G);
        ctx.SessionData.Add("a", a.ToByteArray(true,true));
        ctx.SessionData.Add("g_a", serverDhInnerData.GA);

        var buff = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
        buff.Write(SHA1.HashData(serverDhInnerData.TLBytes.IsSingleSegment ?
            serverDhInnerData.TLBytes.FirstSpan : serverDhInnerData.TLBytes.ToArray()));
        buff.Write(serverDhInnerData.TLBytes, false);
        if (buff.WrittenCount % 16 != 0)
        {
            int rem = 16 - ((int)buff.WrittenCount % 16);
            buff.Write(random.GetRandomBytes(rem));
        }

        Aes aes = Aes.Create();
        aes.Key = tmpAesKey;
        var answerWithHash = new byte[buff.WrittenCount];
        buff.ToReadOnlySequence().CopyTo(answerWithHash);
        aes.EncryptIge(answerWithHash, tmpAesIV);
        return answerWithHash;
    }

    private void RSAPad(IRSAKey rsaKey, out Memory<byte> data,
        out byte[] sha256)
    {
        data = rsaKey.DecryptBlock(encryptedData).AsMemory();
        // data: |-temp_key_xor(32)-|-|-aes_encrypted(224)-| 256 bytes
        Span<byte> tempKey = data.Slice(0, 32).Span;
        Span<byte> aesEncrypted = data.Slice(32).Span;

        byte[] sha256AesEncrypted = SHA256.HashData(aesEncrypted);
        for (int i = 0; i < 32; i++)
        {
            tempKey[i] = (byte)(tempKey[i] ^ sha256AesEncrypted[i]);
        }
        // data: |-temp_key(32)+aes_encrypted(224)-| 256 bytes
        Aes aes = Aes.Create();
        aes.Key = tempKey.ToArray();
        aes.DecryptIge(aesEncrypted, stackalloc byte[32]);
        // data: |-temp_key(32)+data_with_hash(224)-| 256 bytes
        // data_with_hash: |-data_pad_reversed(192)+
        //                   SHA256(temp_key+data_pad)(32)-| 256 bytes
        Span<byte> dataPadReversed = aesEncrypted.Slice(0, 192);
        dataPadReversed.Reverse();
        // data: |-temp_key(32)+data_pad(192)+
        //                   SHA256(temp_key+data_pad)(32)-| 256 bytes
        sha256 = SHA256.HashData(data.Slice(0, 224).Span);
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        nonce = factory.Read<Int128>(ref buff);
        serverNonce = factory.Read<Int128>(ref buff);
        p = buff.ReadTLBytes().ToArray();
        q = buff.ReadTLBytes().ToArray();
        publicKeyFingerprint = buff.ReadInt64(true);
        encryptedData = buff.ReadTLBytes().ToArray();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}