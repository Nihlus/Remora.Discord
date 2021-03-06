//
//  UserMention.cs
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

using JetBrains.Annotations;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;

namespace Remora.Discord.API.Objects
{
    /// <inheritdoc cref="IUserMention"/>
    [PublicAPI]
    public record UserMention : User, IUserMention
    {
        /// <inheritdoc />
        public Optional<IPartialGuildMember> Member { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMention"/> class.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="username">The user's username.</param>
        /// <param name="discriminator">The user's discriminator.</param>
        /// <param name="avatar">The user's avatar.</param>
        /// <param name="isBot">Whether the user is a bot.</param>
        /// <param name="isSystem">Whether the user is a system user.</param>
        /// <param name="isMFAEnabled">Whether MFA is enabled.</param>
        /// <param name="locale">The user's locale.</param>
        /// <param name="isVerified">Whether the user is verified.</param>
        /// <param name="email">The user's email.</param>
        /// <param name="flags">The user's flags.</param>
        /// <param name="premiumType">The user's premium type.</param>
        /// <param name="publicFlags">The user's public flags.</param>
        /// <param name="member">The member information.</param>
        public UserMention
        (
            Snowflake id,
            string username,
            ushort discriminator,
            IImageHash? avatar,
            Optional<bool> isBot = default,
            Optional<bool> isSystem = default,
            Optional<bool> isMFAEnabled = default,
            Optional<string> locale = default,
            Optional<bool> isVerified = default,
            Optional<string?> email = default,
            Optional<UserFlags> flags = default,
            Optional<PremiumType> premiumType = default,
            Optional<UserFlags> publicFlags = default,
            Optional<IPartialGuildMember> member = default
        )
            : base
            (
                id,
                username,
                discriminator,
                avatar,
                isBot,
                isSystem,
                isMFAEnabled,
                locale,
                isVerified,
                email,
                flags,
                premiumType,
                publicFlags
            )
        {
            this.Member = member;
        }
    }
}
