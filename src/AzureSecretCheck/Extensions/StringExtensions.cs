using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzureSecretCheck.Extensions
{
    public static class StringExtensions
    {
        const string blockComments = @"/\*(.*?)\*/";
        const string lineComments = @"//(.*?)\r?\n";
        const string strings = @"""((\\[^\n]|[^""\n])*)""";
        const string verbatimStrings = @"@(""[^""]*"")+";
        public static string RemoveComments(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            string retval = Regex.Replace(input,
            blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
            me =>
            {
                if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                    return me.Value.StartsWith("//") ? Environment.NewLine : "";
                // Keep the literal strings
                return me.Value;
            },
            RegexOptions.Singleline);
            return retval;
        }
    }
}
