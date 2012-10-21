#region Written by Tamas Budavari (budavari@jhu.edu)
/*
 * Millenium.PeanoHilbertSetup
 *
 * The project dedicated to fast spatial searches of the Millenium Run
 * simulations. Also targeting SQL Server 2005.
 * 
 * Please see the files COPYRIGHT and LICENSE for copyright and licensing 
 * information.
 * 
 * Derived from C and Java versions by Volker Springel and Gerard Lemson
 * Written by Tamas Budavari (budavari@jhu.edu)
 * See bottom of file for revision history
 *
 * Current revision:
 *   ID:          $Id: PeanoHilbertSetup.cs,v 1.1 2009-02-01 19:15:22 glemson Exp $
 *   Revision:    $Revision: 1.1 $
 *   Date:        $Date: 2009-02-01 19:15:22 $
 */
#endregion

namespace GadgetLoader
{
    internal sealed class PeanoHilbertSetup
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        internal static readonly PeanoHilbertSetup Instance = new PeanoHilbertSetup();

        internal readonly sbyte[, , ,] quadrants = new sbyte[,,,]{
            /* rotx=0, roty=0-3 */
            {{{0, 7}, {1, 6}}, {{3, 4}, {2, 5}}},
            {{{7, 4}, {6, 5}}, {{0, 3}, {1, 2}}},
            {{{4, 3}, {5, 2}}, {{7, 0}, {6, 1}}},
            {{{3, 0}, {2, 1}}, {{4, 7}, {5, 6}}},
            /* rotx=1, roty=0-3 */
            {{{1, 0}, {6, 7}}, {{2, 3}, {5, 4}}},
            {{{0, 3}, {7, 4}}, {{1, 2}, {6, 5}}},
            {{{3, 2}, {4, 5}}, {{0, 1}, {7, 6}}},
            {{{2, 1}, {5, 6}}, {{3, 0}, {4, 7}}},
            /* rotx=2, roty=0-3 */
            {{{6, 1}, {7, 0}}, {{5, 2}, {4, 3}}},
            {{{1, 2}, {0, 3}}, {{6, 5}, {7, 4}}},
            {{{2, 5}, {3, 4}}, {{1, 6}, {0, 7}}},
            {{{5, 6}, {4, 7}}, {{2, 1}, {3, 0}}},
            /* rotx=3, roty=0-3 */
            {{{7, 6}, {0, 1}}, {{4, 5}, {3, 2}}},
            {{{6, 5}, {1, 2}}, {{7, 4}, {0, 3}}},
            {{{5, 4}, {2, 3}}, {{6, 7}, {1, 0}}},
            {{{4, 7}, {3, 0}}, {{5, 6}, {2, 1}}},
            /* rotx=4, roty=0-3 */
            {{{6, 7}, {5, 4}}, {{1, 0}, {2, 3}}},
            {{{7, 0}, {4, 3}}, {{6, 1}, {5, 2}}},
            {{{0, 1}, {3, 2}}, {{7, 6}, {4, 5}}},
            {{{1, 6}, {2, 5}}, {{0, 7}, {3, 4}}},
            /* rotx=5, roty=0-3 */
            {{{2, 3}, {1, 0}}, {{5, 4}, {6, 7}}},
            {{{3, 4}, {0, 7}}, {{2, 5}, {1, 6}}},
            {{{4, 5}, {7, 6}}, {{3, 2}, {0, 1}}},
            {{{5, 2}, {6, 1}}, {{4, 3}, {7, 0}}},
        };

        internal readonly sbyte[] rotxmap_table = new sbyte[] { 4, 5, 6, 7, 8, 9, 10, 11,
            12, 13, 14, 15, 0, 1, 2, 3, 17, 18, 19, 16, 23, 20, 21, 22};
        internal readonly sbyte[] rotymap_table = new sbyte[] { 1, 2, 3, 0, 16, 17, 18, 19,
            11, 8, 9, 10, 22, 23, 20, 21, 14, 15, 12, 13, 4, 5, 6, 7};

        internal readonly sbyte[] rotx_table = new sbyte[] { 3, 0, 0, 2, 2, 0, 0, 1 };
        internal readonly sbyte[] roty_table = new sbyte[] { 0, 1, 1, 2, 2, 3, 3, 0 };

        internal readonly sbyte[] sense_table = new sbyte[] { -1, -1, -1, +1, +1, -1, -1, -1 };

        internal int[,] quadrants_inverse_x = new int[24, 8];
        internal int[,] quadrants_inverse_y = new int[24, 8];
        internal int[,] quadrants_inverse_z = new int[24, 8];


        /// <summary>
        /// The actual constructor
        /// </summary>
        private PeanoHilbertSetup()
        {
            for (int rotation = 0; rotation < 24; rotation++)
            {
                for (int bitx = 0; bitx < 2; bitx++)
                    for (int bity = 0; bity < 2; bity++)
                        for (int bitz = 0; bitz < 2; bitz++)
                        {
                            sbyte quad = quadrants[rotation, bitx, bity, bitz];
                            quadrants_inverse_x[rotation, quad] = bitx;
                            quadrants_inverse_y[rotation, quad] = bity;
                            quadrants_inverse_z[rotation, quad] = bitz;
                        }
            }
        }

        /// <summary>
        /// Revision from CVS
        /// </summary>
        public static readonly string Revision = "$Revision: 1.1 $";
    }
}

#region Revision History
/* Revision History

        $Log: PeanoHilbertSetup.cs,v $
        Revision 1.1  2009-02-01 19:15:22  glemson
        *** empty log message ***

        Revision 1.2  2007/07/18 08:50:22  budavari
        Made singleton internal

*/
#endregion
