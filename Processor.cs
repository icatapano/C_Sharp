using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.AddIn;
using System.Text;

using Iatric.EasyConnect.AddIns.Views;
using System.IO;

[AddIn("ProVationSIU", Description = "Structure the RGS, AIS, and AIP segments to meet ProVation Specs", Publisher = "Ian Catapano")]
public class ProcessorSIU : AddInBase, ICustomProcessorAddInView
{

    #region Member Variables

    private string _errorMessage;
    private string _filterMessage;

    #endregion

    #region Constructors

    public ProcessorSIU()
    {
    }

    #endregion


    #region Public Methods

    public bool Initialize(string debugFilePath, string parameters)
    {
        string result = this.InitializeBase(debugFilePath, parameters);

        if (String.IsNullOrEmpty(result))
        {
            return true;
        }
        else
        {
            _errorMessage = result;
            return false;
        }

    }

    public Iatric.EasyConnect.AddIns.Views.DataStatus ProcessData(byte[] data, string tempFilePath)
    {
        try
        {
            // List variables to hold the specific segments
            List<string> segRGS = new List<string>();
            List<string> segAIS = new List<string>();
            List<string> segAIP = new List<string>();

            // Variables to hold the starting point for inserting elemets
            int start = 0, aisStart = 0, aipStart = 0;

            // Variable array of procedure mnemonics for the AIS segment
            string[] proList = new string[37] { "ANOSCOPY", "BRON", "BRONNAVIEBUS", "COLONOSCOPY", "COLONPOLYP", "COLONSTENT", "EBUSBRONCH", "EGDABLATE",
                "EGDAPC", "EGDBOTOX", "EGDBRAVO", "EGDBX", "EGDDILATE", "EGDDILATEFLUORO", "EGDENDOFLIP", "EGDESOFLIP", "EGDPILLCAM", "EGDSTENT", "EGJ",
                "ENTEROSCOPY", "ERCPDILATE", "ERCPPANCREAS", "ERCPSPY", "ERCPSTENTPLACE", "ERCPSTENTREMOVE", "ERCPSTONE", "EUSCELIAC", "EUSDIAGNOSTIC",
                "EUSFNAEGD", "EUSLIVEREGD", "EUSPSEUDOCYST", "EUSRECTAL", "MANNOBRAVO", "MANNOESOPH", "MANNOSAND", "SIGMOID", "SIGPOLYP" };

            // Load message to a string
            string message = System.Text.Encoding.ASCII.GetString(data);

            // Split each line of the message into an array element
            string[] temp = message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Seperate out the RGS, AIS, and AIP segments and put them into dedicated lists
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i].Substring(0, 3) == "RGS")
                {
                    if (temp[i].Substring(4, 1) == "1")
                        start = i;
                    segRGS.Add(temp[i]);
                    temp[i] = string.Empty;
                }
                else if (temp[i].Substring(0, 3) == "AIS")
                {
                    segAIS.Add(temp[i]);
                    temp[i] = string.Empty;
                }
                else if (temp[i].Substring(0, 3) == "AIP")
                {
                    segAIP.Add(temp[i]);
                    temp[i] = string.Empty;
                }
            }

            // Convert Lists to arrays to easily place in order
            string[] RGSarray = segRGS.ToArray();
            string[] AISarray = segAIS.ToArray();
            string[] AIParray = segAIP.ToArray();

            // Remove the unwanted information in the RGS segment
            for (int i = 0; i < RGSarray.Length; i++)
            {
                string[] fVal = RGSarray[i].Split('|');
                RGSarray[i] = "";
                RGSarray[i] += fVal[0] + "|" + fVal[1] + "|";
            }

            // Filter out all AIS segments that don't match the procedure list
            for (int i = 0; i < AISarray.Length; i++)
            {
                int match = 0;
                string[] fVal = AISarray[i].Split('|');
                string[] tempVal = fVal[3].Split('^');

                for (int j = 0; j < 37; j++)
                {
                    if (tempVal[0] == proList[j])
                        match = 1;
                }

                if (match == 0)
                {
                    AISarray[i] = "";
                }

                match = 0;
            }

            // Convert array to a list
            List<string> tList = new List<string>();

            for (int i = 0; i < AISarray.Length; i++)
                tList.Add(AISarray[i]);

            // Remove empty lines
            tList.RemoveAll(String.IsNullOrEmpty);

            // Convert list back to an array
            AISarray = tList.ToArray();

            // Convert temp array to a list
            List<String> tempList = new List<string>(temp);
            tempList.RemoveAll(String.IsNullOrEmpty);

            // Enter the segments back into the temp array in the desired order
            for (int i = 0; i < AISarray.Length; i++)
            {
                tempList.Add(RGSarray[i]);
                start++;

                tempList.Add(AISarray[i]);
                aisStart = i + 1;

                if (i >= AIParray.Length)
                {
                    tempList.Add(AIParray[0]);
                }
                else
                {
                    tempList.Add(AIParray[i]);
                    aipStart = i + 1;
                }
                start++;
            }

            // Enter the remaining segments back into the message
            for (int i = aisStart; i < AISarray.Length; i++)
            {
                int j = i + 1;
                tempList.Add("RGS|" + j + "|");
                tempList.Add(AISarray[i]);
                tempList.Add(AIParray[0]);
                start++;
            }

            // Convert the tempList back to an array
            string[] tempArray = tempList.ToArray();

            // Empty out the message variable to re-populate
            message = string.Empty;

            // Enter the array back into the string message
            message = String.Join("\r", tempArray);

            // Write message back to proper format
            data = System.Text.Encoding.ASCII.GetBytes(message);
            File.WriteAllBytes(tempFilePath, data);

            return DataStatus.Success;
        }

        catch (Exception ex)
        {
            this.ErrorMessage = ex.ToString();
            return DataStatus.Error;
        }
    }

    public bool ShutDown()
    {
        return true;
    }

    #endregion


    #region Properties

    public string ErrorMessage
    {
        get
        {
            return _errorMessage;
        }
        set
        {
            _errorMessage = value;
        }
    }

    public string FilterMessage
    {
        get
        {
            return _filterMessage;
        }
        set
        {
            _filterMessage = value;
        }
    }

    #endregion

}