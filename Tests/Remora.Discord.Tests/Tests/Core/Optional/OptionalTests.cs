//
//  OptionalTests.cs
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
using Remora.Discord.Core;
using Xunit;

// ReSharper disable SA1600
#pragma warning disable 1591, SA1600

namespace Remora.Discord.Tests.Tests.Core
{
    /// <summary>
    /// Tests the <see cref="Remora.Discord.Core.Optional{T}"/> struct.
    /// </summary>
    public class OptionalTests
    {
        /// <summary>
        /// Tests the <see cref="Optional{TValue}.HasValue"/> property.
        /// </summary>
        public class HasValue
        {
            [Fact]
            public void ReturnsTrueWhenOptionalValueTypeContainsValue()
            {
                var optional = new Optional<int>(0);

                Assert.True(optional.HasValue);
            }

            [Fact]
            public void ReturnsFalseWhenOptionalValueTypeDoesNotContainValue()
            {
                Optional<int> optional = default;

                Assert.False(optional.HasValue);
            }

            [Fact]
            public void ReturnsTrueWhenOptionalNullableValueTypeContainsValue()
            {
                var optional = new Optional<int?>(0);

                Assert.True(optional.HasValue);
            }

            [Fact]
            public void ReturnsTrueWhenOptionalNullableValueTypeContainsNull()
            {
                var optional = new Optional<int?>(null);

                Assert.True(optional.HasValue);
            }

            [Fact]
            public void ReturnsFalseWhenOptionalNullableValueTypeDoesNotContainValue()
            {
                Optional<int?> optional = default;

                Assert.False(optional.HasValue);
            }

            [Fact]
            public void ReturnsTrueWhenOptionalReferenceTypeContainsValue()
            {
                var optional = new Optional<string>("Hello world!");

                Assert.True(optional.HasValue);
            }

            [Fact]
            public void ReturnsFalseWhenOptionalReferenceTypeDoesNotContainValue()
            {
                Optional<string> optional = default;

                Assert.False(optional.HasValue);
            }

            [Fact]
            public void ReturnsTrueWhenOptionalNullableReferenceTypeContainsValue()
            {
                var optional = new Optional<string?>(null);

                Assert.True(optional.HasValue);
            }

            [Fact]
            public void ReturnsFalseWhenOptionalNullableReferenceTypeDoesNotContainValue()
            {
                Optional<string?> optional = default;

                Assert.False(optional.HasValue);
            }
        }

        /// <summary>
        /// Tests the <see cref="Optional{TValue}.Value"/> property.
        /// </summary>
        public class Value
        {
            [Fact]
            public void ReturnsCorrectValueIfValueTypeOptionalContainsValue()
            {
                var optional = new Optional<int>(64);

                Assert.Equal(64, optional.Value);
            }

            [Fact]
            public void ThrowsIfValueTypeOptionalDoesNotContainValue()
            {
                Optional<int> optional = default;

                Assert.Throws<InvalidOperationException>(() => optional.Value);
            }

            [Fact]
            public void ReturnsCorrectValueIfNullableValueTypeOptionalContainsValue()
            {
                var optional = new Optional<int?>(64);

                Assert.Equal(64, optional.Value);
            }

            [Fact]
            public void ReturnsCorrectValueIfNullableValueTypeOptionalContainsNullValue()
            {
                var optional = new Optional<int?>(null);

                Assert.Null(optional.Value);
            }

            [Fact]
            public void ThrowsIfNullableValueTypeOptionalDoesNotContainValue()
            {
                Optional<int?> optional = default;

                Assert.Throws<InvalidOperationException>(() => optional.Value);
            }

            [Fact]
            public void ReturnsCorrectValueIfReferenceTypeOptionalContainsValue()
            {
                var optional = new Optional<string>("Hello world!");

                Assert.Equal("Hello world!", optional.Value);
            }

            [Fact]
            public void ThrowsIfReferenceTypeOptionalDoesNotContainValue()
            {
                Optional<string> optional = default;

                Assert.Throws<InvalidOperationException>(() => optional.Value);
            }

            [Fact]
            public void ReturnsCorrectValueIfNullableReferenceTypeOptionalContainsValue()
            {
                var optional = new Optional<string?>("Hello world!");

                Assert.Equal("Hello world!", optional.Value);
            }

            [Fact]
            public void ReturnsCorrectValueIfNullableReferenceTypeOptionalContainsNullValue()
            {
                var optional = new Optional<string?>(null);

                Assert.Null(optional.Value);
            }

            [Fact]
            public void ThrowsIfNullableReferenceTypeOptionalDoesNotContainValue()
            {
                Optional<string?> optional = default;

                Assert.Throws<InvalidOperationException>(() => optional.Value);
            }
        }

        /// <summary>
        /// Tests the <see cref="Optional{TValue}.ToString"/> method.
        /// </summary>
        public new class ToString
        {
            [Fact]
            public void ResultContainsValueIfValueTypeOptionalContainsValue()
            {
                var optional = new Optional<int>(64);

                Assert.Contains(64.ToString(), optional.ToString());
            }

            [Fact]
            public void ResultIsEmptyIfValueTypeOptionalDoesNotContainValue()
            {
                var optional = default(Optional<int>);

                Assert.Equal("Empty", optional.ToString());
            }

            [Fact]
            public void ResultContainsValueIfNullableValueTypeOptionalContainsValue()
            {
                var optional = new Optional<int?>(64);

                Assert.Contains(64.ToString(), optional.ToString());
            }

            [Fact]
            public void ResultContainsNullIfNullableValueTypeOptionalContainsNullValue()
            {
                var optional = new Optional<int?>(null);

                Assert.Contains("null", optional.ToString());
            }

            [Fact]
            public void ResultIsEmptyIfNullableValueTypeOptionalDoesNotContainValue()
            {
                var optional = default(Optional<int?>);

                Assert.Equal("Empty", optional.ToString());
            }

            [Fact]
            public void ResultContainsValueIfReferenceTypeOptionalContainsValue()
            {
                var optional = new Optional<string>("Hello world!");

                Assert.Contains("Hello world!", optional.ToString());
            }

            [Fact]
            public void ResultIsEmptyIfReferenceTypeOptionalDoesNotContainValue()
            {
                var optional = default(Optional<string>);

                Assert.Equal("Empty", optional.ToString());
            }

            [Fact]
            public void ResultContainsValueIfNullableReferenceTypeOptionalContainsValue()
            {
                var optional = new Optional<string?>("Hello world!");

                Assert.Contains("Hello world!", optional.ToString());
            }

            [Fact]
            public void ResultContainsNullIfNullableReferenceTypeOptionalContainsNullValue()
            {
                var optional = new Optional<string?>(null);

                Assert.Contains("null", optional.ToString());
            }

            [Fact]
            public void ResultIsEmptyIfNullableReferenceTypeOptionalDoesNotContainValue()
            {
                var optional = default(Optional<string?>);

                Assert.Equal("Empty", optional.ToString());
            }
        }

        /// <summary>
        /// Tests the implicit operator.
        /// </summary>
        public class ImplicitOperator
        {
            [Fact]
            public void CanCreateValueTypeOptionalImplicitly()
            {
                Optional<int> optional = 64;

                Assert.True(optional.HasValue);
                Assert.Equal(64, optional.Value);
            }

            [Fact]
            public void CanCreateNullableValueTypeOptionalImplicitly()
            {
                Optional<int?> optional = 64;

                Assert.True(optional.HasValue);
                Assert.Equal(64, optional.Value);

                Optional<int?> nullOptional = null;

                Assert.True(nullOptional.HasValue);
                Assert.Null(nullOptional.Value);
            }

            [Fact]
            public void CanCreateReferenceTypeOptionalImplicitly()
            {
                Optional<string> optional = "Hello world!";

                Assert.True(optional.HasValue);
                Assert.Equal("Hello world!", optional.Value);
            }

            [Fact]
            public void CanCreateNullableReferenceTypeOptionalImplicitly()
            {
                Optional<string?> optional = "Hello world!";

                Assert.True(optional.HasValue);
                Assert.Equal("Hello world!", optional.Value);

                Optional<string?> nullOptional = null;

                Assert.True(nullOptional.HasValue);
                Assert.Null(nullOptional.Value);
            }
        }
    }
}
