﻿using System;
using System.IO;
using static Data;
using static Microsoft.VisualBasic.Conversion;
using static Microsoft.VisualBasic.Information;
using static Microsoft.VisualBasic.Interaction;
using static Microsoft.VisualBasic.Strings;

namespace MediaFormats
{
    public class BIN
    {
        public void ProcessBINFile(byte[] aFile)
        {
            ProcessBINFile("", aFile);
        }

        public static void ProcessBINFile(string sFile, byte[] aFile = null)
        {
            byte[] aAnsi = null;
            bool bDrawChar;
            int CurChr;
            int iLoop2;
            string sStr = "";

            string sCol = "";
            if (!(aFile == null))
            {
                aAnsi = ConverterSupport.Convert.MergeByteArrays(ConverterSupport.Convert.NullByteArray(), aFile);
            }
            else
            {
                if (File.Exists(sFile))
                {
                    aAnsi = ConverterSupport.InputOutput.ReadBinaryFile(sFile);
                    aAnsi = ConverterSupport.Convert.MergeByteArrays(ConverterSupport.Convert.NullByteArray(), aAnsi);
                }
            }

            ConverterSupport.Mappings.BuildMappings(sCodePg);

            ForeColor = 7;
            BackColor = 0;
            LineWrap = true;
            Blink = false;
            Bold = 0;
            Reversed = false;
            LinesUsed = 0;
            // ERROR: Not supported in C#: ReDimStatement

            for (int x = minX; x <= maxX; x++)
            {
                for (int Y = minY; Y <= maxY; Y++)
                {
                    Data.Screen[x, Y] = new ConverterSupport.ScreenChar(x);
                }
            }
            System.Windows.Forms.Application.DoEvents();
            XPos = minX;
            YPos = minY;
            if (!(aAnsi == null))
            {
                if (UBound(aAnsi) > 0)
                {
                    iLoop = 1;
                    while (iLoop <= UBound(aAnsi))
                    {
                        bDrawChar = true;
                        CurChr = (int)aAnsi[iLoop];

                        //SUB or "S"
                        if (CurChr == 26 | CurChr == 83)
                        {
                            int iSauceOffset = (int)IIf(CurChr == 83, 1, 0);
                            if (UBound(aAnsi) >= iLoop + 128 - iSauceOffset)
                            {
                                sStr = "";
                                for (iLoop2 = 1 - iSauceOffset; iLoop2 <= 5 - iSauceOffset; iLoop2++)
                                {
                                    sStr += Chr(aAnsi[iLoop + iLoop2]);
                                }
                                if (sStr == "SAUCE")
                                {
                                    bDrawChar = false;
                                    iLoop += 1 - iSauceOffset;
                                    iLoop = oSauce.GetFromByteArray(aAnsi, iLoop);
                                    bHasSauce = true;
                                    //Read Sauce
                                    if (bDebug == true)
                                        Console.WriteLine("Sauce Meta found");
                                }
                            }
                        }
                        if (bDrawChar == true)
                        {
                            if (UBound(aAnsi) >= iLoop + 1)
                            {
                                sStr = CurChr.ToString();
                                iLoop += 1;
                                CurChr = (int)aAnsi[iLoop];
                                sCol = Right("0" + Hex(CurChr), 2);
                                ForeColor = (int)ConverterSupport.Convert.HexToDec(Right(sCol, 1));
                                BackColor = (int)ConverterSupport.Convert.HexToDec(Left(sCol, 1));
                                if (ForeColor > 7)
                                {
                                    Bold = 8;
                                    ForeColor -= 8;
                                }
                                else
                                {
                                    Bold = 0;
                                }
                                CurChr = System.Convert.ToInt32(sStr);
                                if (CurChr == 0)
                                {
                                    if (!ConverterSupport.Convert.SetChar(" "))
                                    {
                                        iLoop = UBound(aAnsi) + 1;
                                    }
                                    bDrawChar = false;
                                }
                            }
                        }

                        if (bDrawChar == true)
                        {
                            if (ConverterSupport.Convert.SetChar(Chr(CurChr).ToString()) == false)
                            {
                                iLoop = UBound(aAnsi) + 1;
                            }
                        }
                        iLoop += 1;
                        if (iLoop / 1000 == (int)iLoop / 1000)
                            System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }
    }
}