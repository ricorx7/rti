﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace RTI
{
    class AdcpPD0CodecReadFile
    {
            #region Variables

            /// <summary>
            ///  Setup logger
            /// </summary>
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            #endregion

            /// <summary>
            /// Initialize the object.
            /// </summary>
            public AdcpPD0CodecReadFile()
            {

            }

            /// <summary>
            /// Get a list of all the ensembles in the file.
            /// This will look for the start locations of all the ensembles.
            /// Then it will get the entire ensemble from the file.
            /// </summary>
            /// <param name="file">File to get the ensemble.</param>
            /// <returns>The list of ensembles.</returns>
            public List<DataSet.EnsemblePackage> GetEnsembles(string file)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        // Get the list of all the ensemble start locations
                        var ensStart = FindEnsembleStarts(file);

                        // Decode the ensembles found
                        return FindCompleteEnsembles(ensStart, file);
                    }
                    catch (Exception e)
                    {
                        log.Error("Error Finding ensembles in file.", e);
                    }
                }

                return new List<DataSet.EnsemblePackage>();
            }

            /// <summary>
            /// Look for the start of an ensemble.  This will look for the ID of a header
            /// in the ensemble.  If found, set the start location.
            /// </summary>
            /// <param name="file">File to find the start locations</param>
            /// <returns>List of all the start locations.</returns>
            protected List<int> FindEnsembleStarts(string file)
            {
                var ensStartList = new List<int>();

                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var queue = new List<byte>();

                    // Read in the data from file
                    byte[] buffer = new byte[1];
                    int count = 0;
                    int index = 0;
                    while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)     // Get 1 bytes
                    {
                        // Add the data to the queue
                        queue.Add(buffer[0]);

                        if (queue.Count >= 2)
                        {
                            if (queue[0] == 0x7F &&
                                queue[1] == 0x7F)
                            {
                                // Add the start location to the list
                                // Subtract 1 to get back to the beginning of the queue
                                ensStartList.Add(index - 1);
                            }

                            // Remove the first item from the queue
                            queue.RemoveAt(0);
                        }

                        index++;
                    }

                    fileStream.Close();
                    fileStream.Dispose();
                }
                return ensStartList;
            }

            /// <summary>
            /// Find the entire ensemble in the file.  This will look for the start location.
            /// Decode the payload size and checksum.  If they are good, then generate an 
            /// ensemble from the data.  Add the data to the list and return it.
            /// </summary>
            /// <param name="ensStart">List of all the ensembles.</param>
            /// <param name="file">File to look for the ensembles.</param>
            /// <returns>List of all the ensembles in the file.</returns>
            protected List<DataSet.EnsemblePackage> FindCompleteEnsembles(List<int> ensStart, string file)
            {
                var list = new List<DataSet.EnsemblePackage>();

                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    foreach (var start in ensStart)
                    {
                        try
                        {
                            var buffer = new byte[DataSet.Ensemble.ENSEMBLE_HEADER_LEN]; //Buffer is byte array of size 32. In Binary codec, buffer is byte array of size 32, containing 32 bytes from file

                            // Move the start location and read in the header
                            fileStream.Seek(start, SeekOrigin.Begin);
                            if (fileStream.Read(buffer, 0, buffer.Length) >= DataSet.Ensemble.ENSEMBLE_HEADER_LEN)// Always true, buffer always size of variable, this loads in bytes to Buffer, however
                            {
                            // Get the payload size
                            int payloadSize = MathHelper.LsbMsbInt(buffer[2], buffer[3]) + PD0.CHECKSUM_NUM_BYTE; //When referencing positions in buffer, uses "start" Which implies it is looking for the position in the actual file. (Error?)

                            // Get the ensemble size
                            int ensSize = MathHelper.LsbMsbInt(buffer[2], buffer[3]) + PD0.CHECKSUM_NUM_BYTE;// Same equation as payload size, but the LsbMsbInt Might change buffer itself?

                            // Sanity check
                            if (ensSize > DataSet.Ensemble.ENSEMBLE_HEADER_LEN)
                                {
                                    // Get the entire ensemble
                                    var rawEns = new byte[ensSize];
                                    fileStream.Seek(start, SeekOrigin.Begin);
                                    fileStream.Read(rawEns, 0, rawEns.Length);

                                // Check the checksum

                                ushort calculatedChecksum = PD0.CalculateChecksum(rawEns, ensSize- PD0.CHECKSUM_NUM_BYTE);
                                ushort ensembleChecksum = MathHelper.LsbMsbUShort(rawEns[rawEns.Length - 2], rawEns[rawEns.Length - 1]);

                                //long calculatedChecksum = DataSet.Ensemble.CalculateEnsembleChecksum(rawEns);
                                //long ensembleChecksum = DataSet.Ensemble.RetrieveEnsembleChecksum(rawEns);

                                if (calculatedChecksum == ensembleChecksum)
                                    {
                                        Pd0Codec _pd0Codec = new Pd0Codec();
                                        _pd0Codec.DecodePd0Data(rawEns);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error("Error looking for an ensemble. Loc: " + start, e);
                        }
                    }
                }

                return list;
            }

        }
    }