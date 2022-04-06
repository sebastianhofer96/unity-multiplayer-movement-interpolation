using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class Utils
{
    // paths for benchmark files
    public static readonly string DATA_PATH = Application.persistentDataPath;
    public static readonly string INTENTIONS_FILE = "intentions.csv";
    public static readonly string SAMPLES_FILE = "samples.csv";
    public static readonly string REPORT_FILE = "report.csv";

    // import list from CSV file
    public static List<T> ImportFromCSV<T>(string inputFileName)
    {
        string path = Path.Combine(DATA_PATH, inputFileName);

        if (!File.Exists(path))
        {
            return new List<T>();
        }

        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, CultureInfo.CurrentCulture))
        {
            return csv.GetRecords<T>().ToList();
        }
    }

    // export list to CSV file
    public static void ExportAsCSV<T>(List<T> rows, string outputFileName)
    {
        using (var writer = new StreamWriter(Path.Combine(DATA_PATH, outputFileName)))
        using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
        {
            csv.WriteRecords(rows);
        }
    }

    // read a command line argument
    public static string GetArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    // converts a bool array (with max size 8) to a single byte
    public static byte BooleansToByte(bool[] inputBools)
    {
        byte outputByte = 0;

        for (int i = 0; i < inputBools.Length; i++)
        {
            outputByte |= (byte)(Convert.ToByte(inputBools[i]) << i);
        }

        return outputByte;
    }

    // converts a single byte to a bool array of size 8
    public static bool[] ByteToBooleans(byte inputByte)
    {
        bool[] outputBools = new bool[8];

        for (int i = 0; i < outputBools.Length; i++)
        {
            outputBools[i] = Convert.ToBoolean(inputByte & 1);
            inputByte >>= 1;
        }

        return outputBools;
    }
}