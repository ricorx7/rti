﻿/*
 * Copyright 2011, Rowe Technology Inc. 
 * All rights reserved.
 * http://www.rowetechinc.com
 * https://github.com/rowetechinc
 * 
 * Redistribution and use in source and binary forms, with or without modification, are
 * permitted provided that the following conditions are met:
 * 
 *  1. Redistributions of source code must retain the above copyright notice, this list of
 *      conditions and the following disclaimer.
 *      
 *  2. Redistributions in binary form must reproduce the above copyright notice, this list
 *      of conditions and the following disclaimer in the documentation and/or other materials
 *      provided with the distribution.
 *      
 *  THIS SOFTWARE IS PROVIDED BY Rowe Technology Inc. ''AS IS'' AND ANY EXPRESS OR IMPLIED 
 *  WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 *  FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> OR
 *  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 *  CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 *  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 *  ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 *  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 *  ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *  
 * The views and conclusions contained in the software and documentation are those of the
 * authors and should not be interpreted as representing official policies, either expressed
 * or implied, of Rowe Technology Inc.
 * 
 * HISTORY
 * -----------------------------------------------------------------
 * Date            Initials    Version    Comments
 * -----------------------------------------------------------------
 * 02/11/2014      RC          2.21.3     Initial coding
 * 
 */

namespace RTI
{
    namespace VesselMount
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;

        /// <summary>
        /// Set the heading values based off the options.
        /// </summary>
        public class VmHeadingOffset
        {

            /// <summary>
            /// Add the heading offset to the heading.  This will take the magnetic offset and add it to the
            /// heading value in the ensemble.  It will then take the alignment offset and add it to the heading value
            /// in the ensemble.
            /// </summary>
            /// <param name="ensemble">Ensemble to change the value.</param>
            /// <param name="options">Options to know how the change the value.</param>
            public static void HeadingOffset(ref DataSet.Ensemble ensemble, VesselMountOptions options)
            {
                // Add the magnetic to Ancillary and Bottom Track heading
                AddAncillaryHeadingOffset(ref ensemble, options.HeadingOffsetMag);
                AddBottomTrackHeadingOffset(ref ensemble, options.HeadingOffsetMag);

                // Add the alignment offset to the Ancillary and Bottom Track heading
                AddAncillaryHeadingOffset(ref ensemble, options.HeadingOffsetAlignment);
                AddBottomTrackHeadingOffset(ref ensemble, options.HeadingOffsetAlignment);
            }

            /// <summary>
            /// Add the given offset to the Ancillary heading.
            /// </summary>
            /// <param name="ensemble">Ensemble to modify the heading.</param>
            /// <param name="offset">Offset value to add.</param>
            private static void AddAncillaryHeadingOffset(ref DataSet.Ensemble ensemble, float offset)
            {
                if (ensemble.IsAncillaryAvail)
                {
                    ensemble.AncillaryData.Heading += offset;
                }
            }

            /// <summary>
            /// Add the given offset to the Bottom Track heading.
            /// </summary>
            /// <param name="ensemble">Ensemble to modify the heading.</param>
            /// <param name="offset">Offset value to add.</param>
            private static void AddBottomTrackHeadingOffset(ref DataSet.Ensemble ensemble, float offset)
            {
                if (ensemble.IsBottomTrackAvail)
                {
                    ensemble.BottomTrackData.Heading += offset;
                }
            }
        }

    }
}
