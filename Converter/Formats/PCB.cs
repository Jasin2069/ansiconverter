﻿using System;
using System.IO;
using static Data;
using static Microsoft.VisualBasic.Information;
using static Microsoft.VisualBasic.Interaction;
using static Microsoft.VisualBasic.Strings;

namespace MediaFormats
{
    public class PCB
    {
        public void ProcessPCBFile(byte[] aFile)
        {
            ProcessPCBFile("", aFile);
        }

        public static void ProcessPCBFile(string sFile, byte[] aFile = null)
        {
            if (bDebug)
            {
                Console.WriteLine("Process PCB File: " + sFile);
            }
            byte[] aAnsi = null;
            int AnsiMode = 0;
            bool bDrawChar;
            int CurChr;
            int iLoop2;
            string sStr = "";

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
                        switch (AnsiMode)
                        {
                            case 0:
                                //@
                                if (CurChr == 64)
                                {
                                    AnsiMode = 1;
                                    bDrawChar = false;
                                }
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
                                break;

                            case 1:
                                //X
                                if (CurChr == 88)
                                {
                                    AnsiMode = 2;
                                    bDrawChar = false;
                                }
                                else
                                {
                                    bool bRevert = true;
                                    if (UBound(aAnsi) >= iLoop + 5)
                                    {
                                        sStr = "";
                                        for (iLoop2 = 1; iLoop2 <= 5; iLoop2++)
                                        {
                                            sStr += Chr(aAnsi[iLoop + iLoop2]);
                                        }
                                        if (sStr == Left("CLS@", 4) | sStr == Left("PON@", 4))
                                        {
                                            iLoop += 4;
                                            AnsiMode = 0;
                                            bRevert = false;
                                        }
                                        else
                                        {
                                            if (sStr == "POFF@")
                                            {
                                                iLoop += 5;
                                                AnsiMode = 0;
                                                bRevert = false;
                                            }
                                        }
                                    }
                                    if (bRevert == true)
                                    {
                                        if (ConverterSupport.Convert.SetChar(Chr(64).ToString()) == false)
                                        {
                                            iLoop = UBound(aAnsi) + 1;
                                        }
                                        else
                                        {
                                            bDrawChar = true;
                                            AnsiMode = 0;
                                        }
                                    }
                                }
                                break;

                            case 2:
                                if (UBound(aAnsi) >= iLoop + 1)
                                {
                                    if (ConverterSupport.Convert.isHex(Chr(CurChr).ToString()) & ConverterSupport.Convert.isHex(Chr(aAnsi[iLoop + 1]).ToString()))
                                    {
                                        BackColor = ConverterSupport.Convert.smaller((int)ConverterSupport.Convert.HexToDec((string)Chr(CurChr).ToString()), 7);
                                        ForeColor = (int)ConverterSupport.Convert.HexToDec((string)Chr(aAnsi[iLoop + 1]).ToString());
                                        if (ForeColor > 7)
                                        {
                                            Bold = 8;
                                            ForeColor -= 8;
                                        }
                                        else
                                        {
                                            Bold = 0;
                                        }
                                        AnsiMode = 0;
                                        iLoop += 1;
                                        bDrawChar = false;
                                    }
                                    else
                                    {
                                        bDrawChar = true;
                                    }
                                }
                                else
                                {
                                    bDrawChar = true;
                                }
                                if (bDrawChar == true)
                                {
                                    if (ConverterSupport.Convert.SetChar(Chr(64).ToString()) == false)
                                    {
                                        iLoop = UBound(aAnsi) + 1;
                                    }
                                    else
                                    {
                                        if (ConverterSupport.Convert.SetChar(Chr(88).ToString()))
                                        {
                                            AnsiMode = 0;
                                        }
                                        else
                                        {
                                            iLoop = UBound(aAnsi) + 1;
                                        }
                                    }
                                }
                                break;
                        }
                        if (bDrawChar == true & AnsiMode == 0)
                        {
                            switch (CurChr)
                            {
                                case 10:
                                    YPos += 1;
                                    if (YPos > maxY - 1)
                                    {
                                        YPos = maxY - 1;
                                        iLoop = UBound(aAnsi) + 1;
                                    }
                                    else
                                    {
                                        XPos = minX;
                                    }
                                    if (YPos > LinesUsed)
                                    {
                                        LinesUsed = YPos;
                                    }
                                    // restore X in linefeed so's to support *nix type files
                                    break;

                                case 13:
                                //ignore
                                case 26:
                                default:
                                    if (ConverterSupport.Convert.SetChar(Chr(CurChr).ToString()) == false)
                                    {
                                        iLoop = UBound(aAnsi) + 1;
                                    }
                                    break;
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