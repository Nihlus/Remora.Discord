//
//  CacheKey.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Diagnostics;
using System.Linq;

namespace Remora.Discord.Caching
{
    /// <summary>
    /// Represents a cache key.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Key) + ",nq}")]
    public readonly struct CacheKey : IEquatable<CacheKey>
    {
        private readonly int _argCount;
        private readonly string _name;
        private readonly object? _arg1;
        private readonly object? _arg2;
        private readonly object? _arg3;
        private readonly object[]? _args;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> struct.
        /// </summary>
        /// <param name="name">The first value.</param>
        public CacheKey(string name)
        {
            _argCount = 0;
            _name = name;
            _arg1 = null;
            _arg2 = null;
            _arg3 = null;
            _args = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> struct.
        /// </summary>
        /// <param name="name">The first value.</param>
        /// <param name="arg1">The second value.</param>
        public CacheKey(string name, object arg1)
        {
            _argCount = 1;
            _name = name;
            _arg1 = arg1;
            _arg2 = null;
            _arg3 = null;
            _args = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> struct.
        /// </summary>
        /// <param name="name">The first value.</param>
        /// <param name="arg1">The second value.</param>
        /// <param name="arg2">The third value.</param>
        public CacheKey(string name, object arg1, object arg2)
        {
            _argCount = 2;
            _name = name;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = null;
            _args = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> struct.
        /// </summary>
        /// <param name="name">The first value.</param>
        /// <param name="arg1">The second value.</param>
        /// <param name="arg2">The third value.</param>
        /// <param name="arg3">The fourth value.</param>
        public CacheKey(string name, object arg1, object arg2, object arg3)
        {
            _argCount = 3;
            _name = name;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
            _args = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> struct.
        /// </summary>
        /// <param name="name">The first value.</param>
        /// <param name="args">The second value.</param>
        public CacheKey(string name, params object[] args)
        {
            _argCount = 3;
            _name = name;
            _arg1 = null;
            _arg2 = null;
            _arg3 = null;
            _args = args;
        }

        /// <summary>
        /// Gets the key as a string.
        /// </summary>
        public string Key
        {
            get
            {
                return _argCount switch
                {
                    0 => _name,
                    1 => _name + ":" + _arg1,
                    2 => _name + ":" + _arg1 + ":" + _arg2,
                    3 => _name + ":" + _arg1 + ":" + _arg2 + ":" + _arg3,
                    _ => _name + ":" + string.Join(":", _args!)
                };
            }
        }

        /// <inheritdoc />
        public bool Equals(CacheKey other)
        {
            return _argCount == other._argCount && _name == other._name && Equals(_arg1, other._arg1) && Equals(_arg2, other._arg2) && Equals(_arg3, other._arg3) && Equals(_args, other._args);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is CacheKey other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _argCount switch
            {
                0 => _name!.GetHashCode(),
                1 => HashCode.Combine(_name, _arg1),
                2 => HashCode.Combine(_name, _arg1, _arg2),
                3 => HashCode.Combine(_name, _arg1, _arg2, _arg3),
                _ => _args!.Aggregate(_name.GetHashCode(), HashCode.Combine)
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Key;
        }

        /// <summary>
        /// Compares two cache keys, for equality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>true if the operands are equal, false otherwise.</returns>
        public static bool operator ==(CacheKey left, CacheKey right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two cache keys, for inequality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>false if the operands are equal, true otherwise.</returns>
        public static bool operator !=(CacheKey left, CacheKey right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Implicitly converts actual values into a string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The string.</returns>
        public static implicit operator string(CacheKey key)
        {
            return key.Key;
        }
    }
}
