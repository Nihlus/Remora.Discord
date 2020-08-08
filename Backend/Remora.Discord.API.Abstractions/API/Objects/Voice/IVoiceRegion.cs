//
//  IVoiceRegion.cs
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

namespace Remora.Discord.API.Abstractions.Objects
{
    /// <summary>
    /// Represents a voice region.
    /// </summary>
    public interface IVoiceRegion
    {
        /// <summary>
        /// Gets a unique ID for the region.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the region is VIP-only (i.e, offers larger bitrates).
        /// </summary>
        bool IsVIP { get; }

        /// <summary>
        /// Gets a value indicating whether this is a deprecated region. Avoid switching to these.
        /// </summary>
        bool IsDeprecated { get; }

        /// <summary>
        /// Gets a value indicating whether this is a custom region (used for events, etc).
        /// </summary>
        bool IsCustom { get; }
    }
}
