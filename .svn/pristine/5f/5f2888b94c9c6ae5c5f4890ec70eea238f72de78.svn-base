
using System;
using System.Collections.Generic;

namespace GadgetLoader
{    
    /// <summary>
    /// Represents the Peano-Hilbert identifier.
    /// </summary>
    public class PeanoHilbertID
    {
        public static int GetPeanoHilbertID(int bits, int x, int y, int z)
        {
            PeanoHilbertSetup setup = PeanoHilbertSetup.Instance;

            int i, bitx, bity, bitz, mask, quad, rotation;
            sbyte rotx, roty;
            sbyte sense;

            
            mask = 1 << (bits - 1);
            Int64 Key = 0;
            rotation = 0;
            sense = 1;

            for (i = 0; i < bits; i++, mask >>= 1)
            {
                bitx = (x & mask) != 0 ? 1 : 0;
                bity = (y & mask) != 0 ? 1 : 0;
                bitz = (z & mask) != 0 ? 1 : 0;

                quad = setup.quadrants[rotation, bitx, bity, bitz];

                Key <<= 3;
                Key += (sense == 1) ? (quad) : (7 - quad);

                rotx = setup.rotx_table[quad];
                roty = setup.roty_table[quad];
                sense *= setup.sense_table[quad];

                while (rotx > 0)
                {
                    rotation = setup.rotxmap_table[rotation];
                    rotx--;
                }

                while (roty > 0)
                {
                    rotation = setup.rotymap_table[rotation];
                    roty--;
                }
            }
            return (int)Key;
        }

        /// <summary>
        /// Calculates the position by the inverse formulas.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public void GetPosition(int Bits, int Key, out int x, out int y, out int z)
        {
            PeanoHilbertSetup setup = PeanoHilbertSetup.Instance;
            x = 0;
            y = 0;
            z = 0;
            int i, mask, quad, rotation, shift;
            int keypart;
            sbyte sense, rotx, roty;

            shift = 3 * (Bits - 1);
            mask = 7 << shift;

            rotation = 0;
            sense = 1;

            for (i = 0; i < Bits; i++, mask >>= 3, shift -= 3)
            {
                keypart = (int)(Key & mask) >> shift;

                quad = (sense == 1) ? (keypart) : (7 - keypart);

                x = (x << 1) + setup.quadrants_inverse_x[rotation, quad];
                y = (y << 1) + setup.quadrants_inverse_y[rotation, quad];
                z = (z << 1) + setup.quadrants_inverse_z[rotation, quad];

                rotx = setup.rotx_table[quad];
                roty = setup.roty_table[quad];
                sense *= setup.sense_table[quad];

                while (rotx > 0)
                {
                    rotation = setup.rotxmap_table[rotation];
                    rotx--;
                }

                while (roty > 0)
                {
                    rotation = setup.rotymap_table[rotation];
                    roty--;
                }
            }
        }

    }
}
