﻿/*The contents of this file are subject to the Mozilla Public License Version 1.1
(the "License"); you may not use this file except in compliance with the
License. You may obtain a copy of the License at http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

The Original Code is the TSOClient.

The Initial Developer of the Original Code is
Afr0. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSOClient.Lot
{
    /// <summary>
    /// Segments that can exist at any given tile.
    /// </summary>
    public enum TileSegment
    {
        NoWalls = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 4,
        BottomRight = 8,
        HorizDiag = 16,
        VertDiag = 32,
        AllWalls = 255
    }

    public class Tile
    {
        private List<Wall> m_Walls = new List<Wall>();

        /// <summary>
        /// Does this tile's segment contain a wall?
        /// </summary>
        /// <param name="InSegment">The segment to check against.</param>
        /// <returns>True if the segment contains a wall, false otherwise.</returns>
        public bool HasWall(TileSegment InSegment)
        {
            foreach (Wall Wll in m_Walls)
            {
                if (Wll.Segment == InSegment)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a wall that occupies a given segment.
        /// Assumes that HasWall() has been called to
        /// find out if the given segment has a wall,
        /// otherwise this will return null.
        /// </summary>
        /// <param name="InSegment">The segment for which to return a wall.</param>
        /// <returns>A wall for the given segment, null if no wall occupied the given segment.</returns>
        public Wall GetWall(TileSegment InSegment)
        {
            foreach (Wall Wll in m_Walls)
            {
                if (Wll.Segment == InSegment)
                    return Wll;
            }

            return null;
        }

        /// <summary>
        /// Is it possible to add a wall at the specified segment for this tile?
        /// </summary>
        /// <param name="InSegment">The segment to check against.</param>
        /// <returns>True if a wall can be added at this segment, false otherwise.</returns>
        public bool CanAdd(TileSegment InSegment)
        {
            foreach (Wall Wll in m_Walls)
            {
                if (Wll.Segment == InSegment)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Adds a wall at the given segment of this tile.
        /// </summary>
        /// <param name="Segment">The segment of this tile at which to add a wall.</param>
        /// <returns>True if the wall was successfully added, false otherwise.</returns>
        public bool AddWall(TileSegment Segment)
        {
            if (CanAdd(Segment))
            {
                m_Walls.Add(new Wall(this, Segment));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a wall at the given segment of this tile.
        /// </summary>
        /// <param name="InSegment">The segment of this tile at which to remove a wall.</param>
        /// <returns>True if the wall was successfully added, false otherwise.</returns>
        public bool RemoveWall(TileSegment InSegment)
        {
            foreach (Wall Wll in m_Walls)
            {
                if (Wll.Segment == InSegment)
                {
                    m_Walls.Remove(Wll);
                    return true;
                }
            }

            return false;
        }
    }
}
