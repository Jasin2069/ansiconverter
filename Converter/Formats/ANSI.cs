﻿using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.IO;
using static Data;
using static Microsoft.VisualBasic.Information;
using static Microsoft.VisualBasic.Interaction;
using static Microsoft.VisualBasic.Strings;

namespace MediaFormats
{
    public class ANSI
    {
        public event InfoMsgEventHandler InfoMsg;

        public delegate void InfoMsgEventHandler(String Msg, Boolean nolinebreak, Boolean removelast);

        public ANSI()
        {
        }

        public void ProcessANSIFile(byte[] aFile)
        {
            ProcessANSIFile("", aFile);
        }

        public void ProcessANSIFile(string sFile, byte[] aFile = null)
        {
            if (bDebug)
            {
                Console.WriteLine("Process ANS File: " + sFile);
            }
            int StoreX = 1;
            int StoreY = 1;
            int AnsiMode = 0;
            byte[] aAnsi = null;
            string[] aPar;
            bool bDrawChar;
            bool bEnde;
            int CurChr;
            int iLoop2;
            string sStr = "";
            string sEsc = "";
            Bitmap Frame = null;
            double multiplier = 0.0;
            LineWrap = false;
            Yoffset = 0;
            int BPF = (int)Math.Floor((BPS / 8) / FPS);
            cBPF = 0;
            iFramesCount = 0;
            if ((!(aFile == null)))
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
                    Screen[x, Y] = new ConverterSupport.ScreenChar(x);
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
                    if (bMakeVideo)
                    {
                        multiplier = 100 / UBound(aAnsi);

                        if (InfoMsg != null)
                        {
                            InfoMsg(" - Frames Created: ", true, false);
                        }
                        if (InfoMsg != null)
                        {
                            InfoMsg("[b]    0[/b] (  0.00%)", false, false);
                        }
                    }

                    while (iLoop <= UBound(aAnsi))
                    {
                        bDrawChar = true;
                        CurChr = (int)aAnsi[iLoop];
                        switch (AnsiMode)
                        {
                            case 0:
                                //ESC
                                if (CurChr == 27)
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
                                            //  If bDebug = True Then Console.WriteLine("Sauce Meta found")
                                        }
                                    }
                                }
                                break;

                            case 1:
                                //[
                                if (CurChr == 91)
                                {
                                    AnsiMode = 2;
                                    bDrawChar = false;
                                }
                                else
                                {
                                    if (ConverterSupport.Convert.SetChar(Chr(27).ToString()) == false)
                                    {
                                        iLoop = UBound(aAnsi) + 1;
                                    }
                                    else
                                    {
                                        bDrawChar = true;
                                        AnsiMode = 0;
                                    }
                                }
                                break;

                            case 2:
                                object aRRSize;
                                aRRSize = UBound(aAnsi);
                                sEsc = Chr(CurChr).ToString();
                                //0-9 or ; or ?
                                if ((CurChr >= 48 & CurChr <= 57) | CurChr == 59 | CurChr == 63)
                                {
                                    bEnde = false;
                                    while (bEnde == false)
                                    {
                                        iLoop += 1;
                                        if (iLoop > (int)aRRSize)
                                        {
                                            bEnde = true;
                                        }
                                        else
                                        {
                                            CurChr = aAnsi[iLoop];
                                            sEsc += (string)Chr(CurChr).ToString();
                                            //A-Z, a-z
                                            if ((CurChr >= 65 & CurChr <= 90) | (CurChr >= 97 & CurChr <= 122) | CurChr == 27)
                                            {
                                                bEnde = true;
                                            }
                                        }
                                        if (CurChr == 27)
                                        {
                                            AnsiMode = 1;
                                            bDrawChar = false;
                                        }
                                    }
                                }

                                if (AnsiMode == 2)
                                {
                                    aPar = BuildParams(sEsc);
                                    int NumParams = UBound(aPar);
                                    switch (Chr(CurChr).ToString())
                                    {
                                        case "A":
                                            //Move Cursor Up
                                            if (NumParams > 0)
                                            {
                                                iLoop2 = ConverterSupport.Convert.ChkNum(aPar[1]);
                                                if (iLoop2 == 0)
                                                {
                                                    iLoop2 = 1;
                                                }
                                            }
                                            else
                                            {
                                                iLoop2 = 1;
                                            }
                                            YPos = YPos - iLoop2;
                                            int iSavY = YPos;
                                            // If bDebug = True Then Console.WriteLine("YPos - " & iLoop2 & ", New YPos: " & YPos)
                                            if (YPos < minY)
                                            {
                                                YPos = minY;
                                            }
                                            //If YPos < Yoffset Then
                                            //Yoffset = YPos
                                            //End If
                                            // If iSavY <> YPos Then
                                            //If bDebug = True Then Console.WriteLine("Adjusted Pos Y: " & YPos)
                                            // End If
                                            cBPF -= 1;
                                            break;

                                        case "B":
                                            //Move Cursor Down
                                            if (NumParams > 0)
                                            {
                                                iLoop2 = ConverterSupport.Convert.ChkNum(aPar[1]);
                                                if (iLoop2 == 0)
                                                {
                                                    iLoop2 = 1;
                                                }
                                            }
                                            else
                                            {
                                                iLoop2 = 1;
                                            }
                                            YPos = YPos + iLoop2;
                                            iSavY = YPos;
                                            // If bDebug = True Then Console.WriteLine("YPos + " & iLoop2 & ", New YPos: " & YPos)
                                            if (YPos > maxY - 1)
                                            {
                                                YPos = maxY - 1;
                                            }
                                            if (YPos > LinesUsed)
                                            {
                                                // If LinesUsed > 25 Then
                                                //Yoffset += (YPos - LinesUsed)
                                                //End If
                                                LinesUsed = YPos;
                                            }
                                            // If iSavY <> YPos Then
                                            //If bDebug = True Then Console.WriteLine("Adjusted Pos Y: " & YPos)
                                            // End If
                                            cBPF -= 1;
                                            break;

                                        case "C":
                                            //Move Cursor Right
                                            if (NumParams > 0)
                                            {
                                                iLoop2 = ConverterSupport.Convert.ChkNum(aPar[1]);
                                                if (iLoop2 == 0)
                                                {
                                                    iLoop2 = 1;
                                                }
                                            }
                                            else
                                            {
                                                iLoop2 = 1;
                                            }
                                            for (int x = 1; x <= iLoop2; x++)
                                            {
                                                if (ConverterSupport.Convert.SetChar("-2") == false)
                                                    iLoop = UBound(aAnsi) + 1;
                                            }

                                            cBPF -= 1;
                                            break;

                                        case "D":
                                            //Move Cursor left
                                            if (NumParams > 0)
                                            {
                                                iLoop2 = ConverterSupport.Convert.ChkNum(aPar[1]);
                                                if (iLoop2 == 0)
                                                {
                                                    iLoop2 = 1;
                                                }
                                            }
                                            else
                                            {
                                                iLoop2 = 1;
                                            }
                                            if (iLoop2 == 255)
                                            {
                                                XPos = minX;
                                            }
                                            else
                                            {
                                                for (int x = 1; x <= iLoop2; x++)
                                                {
                                                    if (ConverterSupport.Convert.SetChar("-3") == false)
                                                        iLoop = UBound(aAnsi) + 1;
                                                }
                                            }
                                            cBPF -= 1;
                                            break;

                                        case "H":
                                            //Move Cursor to pos

                                            if (NumParams > 0)
                                            {
                                                iLoop2 = ConverterSupport.Convert.ChkNum(aPar[1]);
                                                YPos = (int)IIf(iLoop2 > 0, minY + (iLoop2 - 1), minY);
                                            }
                                            else
                                            {
                                                YPos = minY;
                                            }
                                            if (NumParams > 1)
                                            {
                                                iLoop2 = ConverterSupport.Convert.ChkNum(aPar[2]);
                                                XPos = (int)IIf(iLoop2 > 0, minX + (iLoop2 - 1), minX);
                                            }
                                            else
                                            {
                                                XPos = minX;
                                            }
                                            int iSavX = XPos;
                                            iSavY = YPos;
                                            // If bDebug = True Then Console.WriteLine("New Cursor Pos X:" & XPos & ", Y: " & YPos)
                                            if (YPos > maxY - 1)
                                            {
                                                YPos = maxY - 1;
                                            }
                                            if (YPos < minY)
                                            {
                                                YPos = minY;
                                            }
                                            if (XPos > maxX)
                                            {
                                                XPos = maxX;
                                            }
                                            if (XPos < minX)
                                            {
                                                XPos = minX;
                                            }
                                            if (YPos > LinesUsed)
                                            {
                                                if (LinesUsed > 25)
                                                {
                                                    Yoffset += (YPos - LinesUsed);
                                                }
                                                LinesUsed = YPos;
                                            }
                                            if (YPos < Yoffset)
                                            {
                                                Yoffset = YPos - 1;
                                            }
                                            // If iSavX <> XPos Or iSavY <> YPos Then
                                            // If bDebug = True Then Console.WriteLine("Adjusted Pos X: " & XPos & ", Y: " & YPos)
                                            // End If
                                            cBPF -= 1;
                                            break;

                                        case "f":
                                            //Move Cursor to pos
                                            if (NumParams > 0)
                                            {
                                                iLoop2 = ConverterSupport.Convert.ChkNum(aPar[1]);
                                                YPos = (int)IIf(iLoop2 > 0, minY + (iLoop2 - 1), minY);
                                            }
                                            else
                                            {
                                                YPos = minY;
                                            }
                                            if (NumParams > 1)
                                            {
                                                iLoop2 = ConverterSupport.Convert.ChkNum(aPar[2]);
                                                XPos = (int)IIf(iLoop2 > 0, minX + (iLoop2 - 1), minX);
                                            }
                                            else
                                            {
                                                XPos = minX;
                                            }
                                            iSavX = XPos;
                                            iSavY = YPos;
                                            if (YPos > maxY - 1)
                                            {
                                                YPos = maxY - 1;
                                            }
                                            if (YPos < minY)
                                            {
                                                YPos = minY;
                                            }
                                            if (XPos > maxX)
                                            {
                                                XPos = maxX;
                                            }
                                            if (XPos < minX)
                                            {
                                                XPos = minX;
                                            }
                                            if (YPos > LinesUsed)
                                            {
                                                //Yoffset()
                                                if (LinesUsed > 25)
                                                {
                                                    Yoffset += (YPos - LinesUsed);
                                                }
                                                LinesUsed = YPos;
                                            }
                                            if (YPos < Yoffset)
                                            {
                                                Yoffset = YPos - 1;
                                            }
                                            //If iSavX <> XPos Or iSavY <> YPos Then
                                            // If bDebug = True Then Console.WriteLine("Adjusted Pos X: " & XPos & ", Y: " & YPos)
                                            // End If
                                            cBPF -= 1;
                                            break;

                                        case "J":
                                            //ForeColor = 7 : BackColor = 0 : Blink = False : Bold = 0 : Reversed = False
                                            if (NumParams > 0)
                                            {
                                                switch (ConverterSupport.Convert.ChkNum(aPar[1]))
                                                {
                                                    case 0:
                                                        //erase from cursor to end of screen
                                                        ClearLineFromCursor(YPos, XPos);
                                                        if (YPos < maxY)
                                                        {
                                                            for (int Y = YPos + 1; Y <= maxY; Y++)
                                                            {
                                                                ClearLine(Y);
                                                            }
                                                        }
                                                        break;
                                                    //   If bDebug = True Then Console.WriteLine("Clear Cursor to End of Screen X:" & XPos & ", Y: " & YPos)
                                                    case 1:
                                                        //Erase from beginning of screen to cursor
                                                        ClearLineToCursor(YPos, XPos);
                                                        if (YPos > minY)
                                                        {
                                                            for (int Y = YPos - 1; Y <= minY; Y++)
                                                            {
                                                                ClearLine(Y);
                                                            }
                                                        }
                                                        break;
                                                    //If bDebug = True Then Console.WriteLine("Clear Beginning of Screen to Cursor X:" & XPos & ", Y: " & YPos)
                                                    case 2:
                                                        //Clear screen and home cursor
                                                        for (int Y = minY; Y <= LinesUsed; Y++)
                                                        {
                                                            ClearLine(Y);
                                                        }

                                                        XPos = minX;
                                                        YPos = minY;
                                                        break;
                                                        //If bDebug = True Then Console.WriteLine("Clear Screen X:" & XPos & ", Y: " & YPos)
                                                }
                                                //Erase from cursor to end of screen
                                            }
                                            else
                                            {
                                                ClearLineFromCursor(YPos, XPos);
                                                if (YPos < maxY)
                                                {
                                                    for (int Y = YPos + 1; Y <= maxY; Y++)
                                                    {
                                                        ClearLine(Y);
                                                    }
                                                }
                                                //  If bDebug = True Then Console.WriteLine("Clear Cursor to End of Screen X:" & XPos & ", Y: " & YPos)
                                            }

                                            cBPF -= 1;
                                            break;

                                        case "m":
                                            //Set Attribute
                                            if (NumParams == 0)
                                            {
                                                ForeColor = 7;
                                                BackColor = 0;
                                                Blink = false;
                                                Bold = 0;
                                                Reversed = false;
                                            }
                                            for (int iLoop3 = 1; iLoop3 <= NumParams; iLoop3++)
                                            {
                                                object i2;
                                                iLoop2 = ConverterSupport.Convert.ChkNum(aPar[iLoop3]);
                                                switch (iLoop2)
                                                {
                                                    case 0:
                                                        if ((string)Left(aPar[iLoop3], 1) == "0")
                                                        {
                                                            ForeColor = 7;
                                                            BackColor = 0;
                                                            Blink = false;
                                                            Bold = 0;
                                                            Reversed = false;
                                                        }
                                                        break;

                                                    case 1:
                                                        Bold = 8;
                                                        break;

                                                    case 2:
                                                        Bold = 0;
                                                        break;

                                                    case 5:
                                                        Blink = true;
                                                        break;

                                                    case 7:
                                                        i2 = ForeColor;
                                                        ForeColor = BackColor;
                                                        BackColor = (int)i2;
                                                        Reversed = true;
                                                        break;

                                                    case 22:
                                                        Bold = 0;
                                                        break;

                                                    case 25:
                                                        Blink = false;
                                                        break;

                                                    case 27:
                                                        i2 = ForeColor;
                                                        ForeColor = BackColor;
                                                        BackColor = (int)i2;
                                                        Reversed = false;
                                                        break;

                                                    case 30:
                                                        ForeColor = 0;
                                                        break;

                                                    case 31:
                                                        ForeColor = 4;
                                                        break;

                                                    case 32:
                                                        ForeColor = 2;
                                                        break;

                                                    case 33:
                                                        ForeColor = 6;
                                                        break;

                                                    case 34:
                                                        ForeColor = 1;
                                                        break;

                                                    case 35:
                                                        ForeColor = 5;
                                                        break;

                                                    case 36:
                                                        ForeColor = 3;
                                                        break;

                                                    case 37:
                                                        ForeColor = 7;
                                                        break;

                                                    case 40:
                                                        BackColor = 0;
                                                        break;

                                                    case 41:
                                                        BackColor = 4;
                                                        break;

                                                    case 42:
                                                        BackColor = 2;
                                                        break;

                                                    case 43:
                                                        BackColor = 6;
                                                        break;

                                                    case 44:
                                                        BackColor = 1;
                                                        break;

                                                    case 45:
                                                        BackColor = 5;
                                                        break;

                                                    case 46:
                                                        BackColor = 3;
                                                        break;

                                                    case 47:
                                                        BackColor = 7;
                                                        break;
                                                }
                                                cBPF -= 1;
                                            }
                                            break;

                                        case "K":
                                            if (NumParams > 0)
                                            {
                                                switch (ConverterSupport.Convert.ChkNum(aPar[1]))
                                                {
                                                    case 0:
                                                        //clear to the end of the line
                                                        ClearLineFromCursor(YPos, XPos);
                                                        break;

                                                    case 1:
                                                        //Erase from beginning of line to cursor
                                                        ClearLineToCursor(YPos, XPos);
                                                        break;

                                                    case 2:
                                                        //Erase line containing the cursor
                                                        ClearLine(YPos);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                ClearLineFromCursor(YPos, XPos);
                                                //clear to the end of the line
                                            }
                                            cBPF -= 1;
                                            break;
                                        //iLoop2 = XPos
                                        //Do While iLoop2 <= maxX
                                        // iLoop2 = IIf(SetChar("-1"), iLoop2 + 1, maxX + 1)
                                        // Loop
                                        case "s":
                                            StoreX = XPos;
                                            StoreY = YPos;
                                            //save cursor position
                                            cBPF -= 2;
                                            break;
                                        //If bDebug = True Then Console.WriteLine("Save pos X: " & XPos & ", Y:" & YPos)
                                        case "u":
                                            // If bDebug = True Then Console.WriteLine("Restore pos X: " & StoreX & ", Y:" & StoreY & ", Old Pos X: " & XPos & ", Y:" & YPos)
                                            XPos = StoreX;
                                            YPos = StoreY;
                                            //return to saved position
                                            cBPF -= 1;
                                            if (YPos > LinesUsed)
                                            {
                                                //Yoffset()
                                                if (LinesUsed > 25)
                                                {
                                                    Yoffset += (YPos - LinesUsed);
                                                }
                                                LinesUsed = YPos;
                                            }
                                            break;
                                            // If YPos < Yoffset Then
                                            //Yoffset = YPos - 1
                                            break;
                                        //End If
                                        case "n":
                                            //Report Cursor Position
                                            cBPF -= 2;
                                            break;

                                        case "h":
                                            //Set Display Mode
                                            //<[=Xh   X = Mode, 7 = Turn ON linewrap
                                            if (NumParams > 0)
                                            {
                                                if (aPar[1] == "?7" | aPar[1] == "7")
                                                {
                                                    //If bDebug = True Then Console.WriteLine(aPar[1] & ", Turn on Linewrap")
                                                    LineWrap = true;
                                                }
                                            }
                                            break;

                                        case "l":
                                            //Set Display Mode
                                            //<[=7l  = Truncate Lines longer than 80 chars
                                            if (NumParams > 0)
                                            {
                                                if (aPar[1] == "?7" | aPar[1] == "7")
                                                {
                                                    //If bDebug = True Then Console.WriteLine(aPar[1] & ", Truncate Lines longer than 80 chars")
                                                    LineWrap = false;
                                                }
                                            }
                                            break;

                                        default:
                                            if (bDebug)
                                            {
                                                Console.WriteLine("Unknown ANSI Command: " + Chr(CurChr).ToString() + " (" + CurChr.ToString() + ") Params: " + Join(aPar, "|").ToString());
                                            }
                                            break;
                                    }
                                    bDrawChar = false;
                                    AnsiMode = 0;
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
                                        if (LinesUsed > 25)
                                        {
                                            Yoffset += (YPos - LinesUsed);
                                        }
                                        LinesUsed = YPos;
                                    }
                                    break;
                                //If YPos < Yoffset Then
                                //Yoffset = YPos
                                //End If
                                // restore X in linefeed so's to support *nix type files
                                case 13:
                                    // XPos = 1
                                    //ignore
                                    break;

                                case 26:
                                default:
                                    if (ConverterSupport.Convert.SetChar(Chr(CurChr).ToString()) == false)
                                    {
                                        iLoop = UBound(aAnsi) + 1;
                                    }
                                    break;
                            }
                        }
                        if (YPos > LinesUsed)
                        {
                            //   If LinesUsed > 25 Then
                            //Yoffset += (YPos - LinesUsed)
                            //End If
                            LinesUsed = YPos;
                        }
                        //If YPos < Yoffset Then
                        //Yoffset = YPos - 1
                        //End If
                        iLoop += 1;
                        cBPF += 1;
                        if (cBPF == BPF)
                        {
                            if (bMakeVideo)
                            {
                                Frame = ConverterSupport.Convert.CreateVideoFrame(pSmallFont, pNoColors, Yoffset);
                                //TempVideoFolder
                                iFramesCount += 1;
                                //oAVIFile.AddFrame(Frame)
                                //If bDebug Then
                                //
                                string newOutFile = Path.Combine(TempVideoFolder, Path.GetFileNameWithoutExtension(OutFileWrite) + "_" + Strings.Right("00000" + iFramesCount.ToString(), 5) + ".PNG");
                                Frame.Save(newOutFile, System.Drawing.Imaging.ImageFormat.Png);
                                if (iFramesCount / 10 == (int)iFramesCount / 10)
                                {
                                    if (InfoMsg != null)
                                    {
                                        InfoMsg("[b]" + Strings.Right("     " + iFramesCount.ToString(), 5) + "[/b] (" + Strings.Right("   " + Math.Round(iLoop * multiplier, 2).ToString(), 6) + "%)", true, true);
                                    }
                                }

                                //End If
                                //
                            }
                            cBPF = 0;
                        }
                        if (iLoop / 1000 == (int)iLoop / 1000)
                            System.Windows.Forms.Application.DoEvents();
                    }
                    if (bMakeVideo)
                    {
                        if (InfoMsg != null)
                        {
                            InfoMsg("[b]" + Strings.Right("     " + iFramesCount.ToString(), 5) + "[/b] (100.00%)", true, true);
                        }
                        if (LastFrame > 0 & !(Frame == null))
                        {
                            for (int tmpi = 1; tmpi <= Math.Floor(FPS * LastFrame); tmpi++)
                            {
                                iFramesCount += 1;
                                string newOutFile = Path.Combine(TempVideoFolder, Path.GetFileNameWithoutExtension(OutFileWrite) + "_" + Strings.Right("00000" + iFramesCount.ToString(), 5) + ".PNG");
                                Frame.Save(newOutFile, System.Drawing.Imaging.ImageFormat.Png);
                                if (InfoMsg != null)
                                {
                                    InfoMsg("[b]" + Strings.Right("     " + iFramesCount.ToString(), 5) + "[/b] (100.00%)", true, true);
                                }
                            }
                        }
                        if (InfoMsg != null)
                        {
                            InfoMsg("[b]" + Strings.Right("     " + iFramesCount.ToString(), 5) + "[/b] (100.00%)", true, true);
                        }
                        if (InfoMsg != null)
                        {
                            InfoMsg(Environment.NewLine, true, false);
                        }
                    }
                }
            }
        }

        private void ClearLine(int iLine)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Screen[x, iLine] = new ConverterSupport.ScreenChar(x);
                Screen[x, iLine].BackColor = BackColor;
            }
        }

        private void ClearLineToCursor(int iLine, int ipos)
        {
            for (int x = minX; x <= ipos; x++)
            {
                Screen[x, iLine] = new ConverterSupport.ScreenChar(x);
                Screen[x, iLine].BackColor = BackColor;
            }
        }

        private void ClearLineFromCursor(int iLine, int iPos)
        {
            for (int x = iPos; x <= maxX; x++)
            {
                Screen[x, iLine] = new ConverterSupport.ScreenChar(x);
                Screen[x, iLine].BackColor = BackColor;
            }
        }

        private bool MoveCursorFW(int iNum)
        {
            bool bRes = true;
            for (int x = 1; x <= iNum; x++)
            {
                XPos += 1;
                if (XPos > maxX)
                {
                    //If LineWrap = True Then
                    YPos += 1;
                    XPos = 1;
                    if (YPos > maxY)
                    {
                        YPos = maxY;
                        bRes = false;
                        break; // TODO: might not be correct. Was : Exit For
                    }
                    //Else
                    //    XPos = maxX
                    //End If
                }
            }
            return bRes;
        }

        private bool MoveCursorBW(int iNum)
        {
            bool bRes = true;
            for (int x = 1; x <= iNum; x++)
            {
                XPos -= 1;
                if (XPos < minX)
                {
                    //If LineWrap = True Then
                    YPos -= 1;
                    XPos = maxX;
                    if (YPos < minY)
                    {
                        YPos = minY;
                        bRes = false;
                        break; // TODO: might not be correct. Was : Exit For
                    }
                    //  Else
                    //    XPos = minX
                    // End If
                }
            }
            return bRes;
        }

        //----------------------------------------------------
        public string[] BuildParams(string str)
        {
            int pcount = 0;
            int iLp2 = 0;
            string sWork = str;
            string sCurr = "";
            bool bEnde2 = false;
            string[] aParams = null;
            // ERROR: Not supported in C#: ReDimStatement

            aParams[0] = "0";
            if (sWork.Length > 1)
            {
                while (sWork.Length > 0)
                {
                    iLp2 = 0;
                    sCurr = "";
                    bEnde2 = false;
                    while (bEnde2 == false & iLp2 < sWork.Length)
                    {
                        //0-9 or ?
                        if ((Asc(Mid(sWork, iLp2 + 1, 1)) >= 48 & Asc(Mid(sWork, iLp2 + 1, 1)) <= 57) | Asc(Mid(sWork, iLp2 + 1, 1)) == 63)
                        {
                            sCurr += (string)Mid(sWork, iLp2 + 1, 1);
                            iLp2 += 1;
                        }
                        else
                        {
                            iLp2 += 1;
                            bEnde2 = true;
                        }
                        if (iLp2 >= Len(sWork))
                        {
                            bEnde2 = true;
                        }
                    }
                    if (iLp2 > 0)
                    {
                        sWork = Right(sWork, sWork.Length - iLp2);
                    }
                    pcount += 1;
                    Array.Resize(ref aParams, pcount + 1);
                    aParams[pcount] = sCurr;
                }
            }
            //System.Windows.Forms.Application.DoEvents()
            return aParams;
        }
    }
}