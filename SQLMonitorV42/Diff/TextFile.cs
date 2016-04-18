using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DifferenceEngine
{
	public class TextLine : IComparable
	{
		public string Line;
		public int _hash;

		public TextLine(string str)
		{
			Line = str.Replace("\t","    ");
			_hash = str.GetHashCode();
		}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			return _hash.CompareTo(((TextLine)obj)._hash);
		}

		#endregion
	}


	public class DiffListText : IDiffList
	{
		private const int MaxLineLength = 1024;
		private List<TextLine> _lines;

		public DiffListText(string Source, bool IsFile)
		{
            _lines = new List<TextLine>();
            if (IsFile)
            {
                using (StreamReader sr = new StreamReader(Source))
                {
                    String line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Length > MaxLineLength)
                        {
                            throw new InvalidOperationException(
                                string.Format("File contains a line greater than {0} characters.",
                                MaxLineLength.ToString()));
                        }
                        _lines.Add(new TextLine(line));
                    }
                }
            }
            else
            {
                Source.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList().ForEach(l => _lines.Add(new TextLine(l)));
            }
		}
		#region IDiffList Members

		public int Count()
		{
			return _lines.Count;
		}

		public IComparable GetByIndex(int index)
		{
			return (TextLine)_lines[index];
		}

		#endregion
	
	}
}