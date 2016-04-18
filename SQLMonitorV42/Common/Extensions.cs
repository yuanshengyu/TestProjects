using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Xnlab.SQLMon
{
    public static class Extensions
    {
        public static void Invoke(this Control Control, Action Action)
        {
            try
            {
                if (Control != null && Control.IsHandleCreated && !Control.IsDisposed)
                    Control.Invoke(Action);
            }
            catch (Exception)
            {
            }
        }

        public static string RemoveSpace(this string Content)
        {
            Content = Content.Trim('\t');
            while (Content.IndexOf("  ") != -1)
            {
                Content = Content.Replace("  ", " ");
            }
            return Content.Trim();
        }

        public static string ParseObjectName(this string Line)
        {
            Line = SubstringTill(Line, Utils.MultiCommentStart);
            Line = SubstringTill(Line, Utils.SingleCommentStart);
            Line = Line.Trim('[', ']', ';', ' ');
            var schema = QueryEngine.DefaultSchema + QueryEngine.Dot;
            if (Line.StartsWith(schema))
                Line = Line.Substring(schema.Length);
            Line = SubstringTill(Line, " ");
            Line = SubstringTill(Line, "(");
            return Line.Trim();
        }

        public static string SubstringTill(this string Line, string Separator)
        {
            var index = Line.IndexOf(Separator);
            if (index != -1)
                return Line.Substring(0, index);
            else
                return Line;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
