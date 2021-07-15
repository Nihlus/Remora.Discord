//
//  Optional.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Remora.Discord.Core
{
    /// <summary>
    /// Represents an optional value. This is mainly used for JSON de/serializalization where a value can be either
    /// present, null, or completely missing.
    ///
    /// While a <see cref="Nullable{T}"/> allows for a value to be logically present but contain a null value,
    /// <see cref="Optional{TValue}"/> allows for a value to be logically missing. For example, an optional without a
    /// value would never be serialized, but a nullable with a null value would (albeit as "null").
    /// </summary>
    /// <typeparam name="TValue">The inner type.</typeparam>
    [PublicAPI]
    public readonly struct Optional<TValue> : IOptional
    {
        private readonly TValue _value;

        /// <summary>
        /// Gets the value contained in the optional.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the optional does not contain a value.</exception>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue Value
        {
            get
            {
                if (this.HasValue)
                {
                    return _value;
                }

                throw new InvalidOperationException("The optional did not contain a valid value.");
            }
        }

        /// <inheritdoc />
        public bool HasValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Optional{TValue}"/> struct.
        /// </summary>
        /// <param name="value">The contained value.</param>
        public Optional(TValue value)
        {
            _value = value;
            this.HasValue = true;
        }

        /// <summary>
        /// Implicitly converts actual values into an optional.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The created optional.</returns>
        public static implicit operator Optional<TValue>(TValue value)
        {
            return new(value);
        }

        /// <summary>
        /// Compares two optionals, for equality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>true if the operands are equal, false otherwise.</returns>
        public static bool operator ==(Optional<TValue> left, Optional<TValue> right)
            => left.Equals(right);

        /// <summary>
        /// Compares two optionals, for inequality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>false if the operands are equal, true otherwise.</returns>
        public static bool operator !=(Optional<TValue> left, Optional<TValue> right)
            => !left.Equals(right);

        /// <summary>
        /// Compares this instance for equality with another instance.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>true if the instances are considered equal; otherwise, false.</returns>
        public bool Equals(Optional<TValue> other)
        {
            return EqualityComparer<TValue>.Default.Equals(_value, other._value) && this.HasValue == other.HasValue;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Optional<TValue> other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(_value, this.HasValue);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.HasValue
                ? $"{{{_value?.ToString() ?? "null"}}}"
                : "Empty";
        }
    }
}
