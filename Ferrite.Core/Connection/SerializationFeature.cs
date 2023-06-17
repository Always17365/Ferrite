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

using DotNext.IO;
using Ferrite.TL;

namespace Ferrite.Core.Connection;

public class SerializationFeature : ITLObjectFactory
{
    private readonly ITLObjectFactory _factory;
    public SerializationFeature(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    public ITLObject Read(int constructor, ref SequenceReader buff)
    {
        return _factory.Read(constructor, ref buff);
    }

    public T Read<T>(ref SequenceReader buff) where T : ITLObject
    {
        return _factory.Read<T>(ref buff);
    }

    public T Resolve<T>() where T : ITLObject
    {
        return _factory.Resolve<T>();
    }
}