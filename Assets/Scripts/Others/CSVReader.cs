using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };


    public static List<Dictionary<string, object>> Read(string filepath)
	{
		var list = new List<Dictionary<string, object>>();

		string source;

		if (Application.platform == RuntimePlatform.Android)
        {
			#pragma warning disable CS0618
			WWW reader = new WWW(filepath);
			#pragma warning restore CS0618

			while (!reader.isDone) { }

            source = reader.text;
		}
        else
        {
			StreamReader sr = new StreamReader(filepath);
            source = sr.ReadToEnd();
			sr.Close();
		}

        var lines = Regex.Split(source, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);

        for (var i = 1; i < lines.Length; i++)
        {

            var values = Regex.Split(lines[i], SPLIT_RE);

            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();

            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];

                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                object finalvalue = value;
                int n;
                float f;

                if (int.TryParse(value, out n))
                    finalvalue = n;
                else if (float.TryParse(value, out f))
                    finalvalue = f;

                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }
}

//Source: https://github.com/tikonen/blog/tree/master/csvreader