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
 * 03/12/2014      RC          2.21.4     Initial coding
 * 04/16/2014      RC          2.21.4     Fixed code to handle vertical beams.
 * 07/24/2014      RC          2.23.0     Fixed bug in DecodeRtiEnsemble() if numCodeRepeats is 0 or N is 0.
 * 
 * 
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Correlation Data Type.
    /// </summary>
    public class Pd0Correlation : Pd0DataType
    {
        #region Variable

        /// <summary>
        /// LSB for the ID for the PD0 Correlation data type.
        /// </summary>
        public const byte ID_LSB = 0x00;

        /// <summary>
        /// MSB for the ID for the PD0 Correlation data type.
        /// </summary>
        public const byte ID_MSB = 0x02;

        /// <summary>
        /// Number of bytes in a depth cell.
        /// 4 Beams per depth cell.
        /// 1 Bytes per beam.
        /// </summary>
        public const int BYTES_PER_DEPTHCELL = 4;

        /// <summary>
        /// Number of bytes for the header.
        /// The LSB and MSB.
        /// </summary>
        public const int BYTES_PER_HEADER = 2;

        #endregion

        #region Properties

        /// <summary>
        /// Correlation magnitude data give the magnitude of the normalized echo
        /// autocorrelation at the lag used for estimating the Doppler phase change.
        /// The Workhorse represents this magnitude by a linear scale between 0 and
        /// 255, where 255 is perfect correlation (i.e., a solid target). A value of zero
        /// indicates bad correlation values.
        /// </summary>
        public byte[,] Correlation { get; set; }

        #endregion

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public Pd0Correlation()
            : base(ID_LSB, ID_MSB, Pd0ID.Pd0Types.Correlation)
        {

        }

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="data">Data to decode for the object.</param>
        public Pd0Correlation(byte[] data)
            : base(ID_LSB, ID_MSB, Pd0ID.Pd0Types.Correlation)
        {
            Decode(data);
        }

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="data">Data to decode for the object.</param>
        /// <param name="offset">Offset in the binary data.</param>
        public Pd0Correlation(byte[] data, ushort offset)
            : base(ID_LSB, ID_MSB, Pd0ID.Pd0Types.Correlation)
        {
            this.Offset = offset;
            Decode(data);
        }

        /// <summary>
        /// Initialize the object.
        /// </summary>
        public Pd0Correlation(int numDepthCells)
            : base(ID_LSB, ID_MSB, Pd0ID.Pd0Types.Correlation)
        {
            Correlation = new byte[numDepthCells, 4];
        }

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="corr">Correlation data set.</param>
        /// <param name="numCodeRepeats">Number of code repeats.  Found in SystemSetupDataSet.</param>
        public Pd0Correlation(DataSet.CorrelationDataSet corr, float numCodeRepeats)
            : base(ID_LSB, ID_MSB, Pd0ID.Pd0Types.Correlation)
        {
            DecodeRtiEnsemble(corr, numCodeRepeats);
        }

        /// <summary>
        /// Encode the data type to binary PD0 format.
        /// </summary>
        /// <returns>Binary PD0 format.</returns>
        public override byte[] Encode()
        {
            // Start with the first 2 bytes for the header
            // Then determine how many depth cells exist
            int numBytes = BYTES_PER_HEADER;
            numBytes += (Correlation.GetLength(0) * BYTES_PER_DEPTHCELL);

            byte[] data = new byte[numBytes];

            // Header
            data[0] = ID_LSB;
            data[1] = ID_MSB;

            // Set the start location
            int loc = 2;

            for (int x = 0; x < Correlation.GetLength(0); x++)
            {
                // Add the data to the array
                data[loc++] = Correlation[x, 0];
                data[loc++] = Correlation[x, 1];
                data[loc++] = Correlation[x, 2];
                data[loc++] = Correlation[x, 3];
            }

            return data;
        }

        /// <summary>
        /// Decode the given binary PD0 data in the object.
        /// </summary>
        /// <param name="data">Binary PD0 data.</param>
        public override void Decode(byte[] data)
        {
            // Remove the first 2 bytes for the header
            // Divide by 8, because there are 8 bytes per depth cell
            // 2 bytes per beam in a depth cell.
            // 4 beams per depth cells
            int numDepthCells = (int)Math.Round(((double)(data.Length - BYTES_PER_HEADER) / BYTES_PER_DEPTHCELL));

            // Create the array to hold all the depth cells
            Correlation = new byte[numDepthCells, 4];

            // Start after the header
            for (int x = 2; x < data.Length; x += BYTES_PER_DEPTHCELL)
            {
                int depthCell = (int)Math.Round((double)(x / BYTES_PER_DEPTHCELL));

                Correlation[depthCell, 0] = data[x + 0];
                Correlation[depthCell, 1] = data[x + 1];
                Correlation[depthCell, 2] = data[x + 2];
                Correlation[depthCell, 3] = data[x + 3];
            }
        }

        /// <summary>
        /// Get the correlation from the array based off the depth cell and
        /// beam given.
        /// </summary>
        /// <param name="depthCell">Depth cell.</param>
        /// <param name="beam">Beam.</param>
        /// <returns>Correlation.</returns>
        public byte GetCorrelation(int depthCell, int beam)
        {
            return Correlation[depthCell, beam];
        }

        /// <summary>
        /// Get the number of depth cells.
        /// </summary>
        /// <returns>Number of depth cells.</returns>
        public int GetNumDepthCells()
        {
            return Correlation.GetLength(0);
        }

        /// <summary>
        /// Get the number of bytes in the data type.
        /// This is based off the number of depth cells and
        /// bytes per depth cells.
        /// </summary>
        /// <returns>Number of bytes for the data type.</returns>
        public override int GetDataTypeSize()
        {
            // Start with the first 2 bytes for the header
            // Then determine how many depth cells exist
            int numBytes = BYTES_PER_HEADER;
            numBytes += (Correlation.GetLength(0) * BYTES_PER_DEPTHCELL);

            return numBytes;
        }

        /// <summary>
        /// Get the size of a Correlation Data type based
        /// off the number of depth cells.
        /// </summary>
        /// <param name="numDepthCells">Number of depth cells.</param>
        /// <returns>Number of byte in a Correlation Data Type.</returns>
        public static int GetCorrelationSize(int numDepthCells)
        {
            return 2 + (4 * numDepthCells);
        }

        #region RTI Ensemble

        /// <summary>
        /// Convert the RTI Correlation data set to the PD0 Correlation data type.
        /// </summary>
        /// <param name="corr">RTI Correlation data set.</param>
        /// <param name="numRepeats">Number of code repeats.  Found in SystemSetupDataSet.</param>
        public void DecodeRtiEnsemble(DataSet.CorrelationDataSet corr, float numRepeats)
        {
            if (corr.CorrelationData != null)
            {
                //Correlation = new byte[corr.CorrelationData.GetLength(0), corr.CorrelationData.GetLength(1)];
                Correlation = new byte[corr.CorrelationData.GetLength(0), PD0.NUM_BEAMS];

                // The value has to be converted from percentage to 0-255
                // Scale 0%-100% to 0-255
                // 255 = 100%
                // 0   =   0%
                // 50% = 0.50 * 255 = 127.5 = 255/2

                for (int bin = 0; bin < corr.CorrelationData.GetLength(0); bin++)
                {
                    // 4 Beam system
                    if (corr.CorrelationData.GetLength(1) >= PD0.NUM_BEAMS)
                    {
                        for (int beam = 0; beam < corr.CorrelationData.GetLength(1); beam++)
                        {
                            // beam order 3,2,0,1
                            int newBeam = 0;
                            switch (beam)
                            {
                                case 0:
                                    newBeam = 3;
                                    break;
                                case 1:
                                    newBeam = 2;
                                    break;
                                case 2:
                                    newBeam = 0;
                                    break;
                                case 3:
                                    newBeam = 1;
                                    break;
                            }

                            // Check if numRepeats = 0    
                            if (numRepeats == 0) { numRepeats = 1.0f; }

                            float n = ((numRepeats - 1.0f) / numRepeats);
                                
                            // Check if n = 0    
                            if (n == 0) { n = 1.0f; }
                            
                            float val = corr.CorrelationData[bin, newBeam] * 128.0f;
                            Correlation[bin, beam] = (byte)(Math.Round(val / n));

                            //Correlation[bin, beam] = (byte)(Math.Round(corr.CorrelationData[bin, newBeam] * 255));
                        }
                    }
                    // Vertical beam
                    else if (corr.CorrelationData.GetLength(1) == 1)
                    {
                        Correlation[bin, 0] = (byte)(Math.Round(corr.CorrelationData[bin, 0] * 255));
                    }
                }
            }
        }

        #endregion

    }
}
