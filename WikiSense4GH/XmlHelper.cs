using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WikiSense4GH {
    static class XmlHelper {
        /// <summary>
        /// Gets a member type from a XmlNode assuming we pass a member-tag.
        /// </summary>
        /// <param name="_node">Node containing member</param>
        /// <param name="_index">Index of member name</param>
        /// <returns>Member name, will remove member type if index = 0</returns>
        public static string GetMemberName(XmlNode _node, int _index) {
            string[] values = SplitValues(_node.Attributes["name"].Value);

            if (_index >= values.Length) {
                throw new ArgumentOutOfRangeException();
            }

            string result = values [ 0 ].Remove(0, 2);
            if (_index == 0 ) {
                return result;
            }

            for(int i = 1 ; i <= _index ; i++ ) {
                result += "." + values [ i ];
            }

            return result;
        }

        /// <summary>
        /// Counts how many member-names that exists in one node.
        /// </summary>
        /// <param name="_node">Node to count</param>
        /// <returns>Amount of member names in this node</returns>
        public static int CountMemberNames(XmlNode _node) {
            return SplitValues(_node.Attributes["name"].Value).Length;
        }

        /// <summary>
        /// Gets the member type, i.e. the first character of the member
        /// </summary>
        /// <param name="_node">Node containing type</param>
        /// <returns>Type identifyer</returns>
        public static char GetMemberType(XmlNode _node) {
            return _node.Attributes [ "name" ].Value [ 0 ];
        }

        /// <summary>
        /// Removes unneccesary whitespaces that may occur in longer strings.
        /// </summary>
        /// <param name="_string">String to clean from whitespaces.</param>
        /// <returns>Whitespace free string with exactly one space between every word!</returns>
        public static string RemoveWhitespace(string _string) {
            _string = _string.Replace("\t", "");
            string[] split = _string.Split(' ');
            string res = "";
            for(int i = 0 ; i < split.Length ; i++ ) {
                if ( split [ i ] != "" ) {
                    res += split [ i ] + " ";
                }
            }

            return res;
        }

        /// <summary>
        /// Splits values properly and saves function arguments from being split
        /// </summary>
        /// <param name="_value">String to be split</param>
        /// <returns>The split string in an array</returns>
        private static string [ ] SplitValues(string _value) {
            string functionBreakout = "";
            if ( _value.Contains("(") ) {
                int breakoutIndex = _value.LastIndexOf('(');
                functionBreakout = _value.Remove(0, breakoutIndex);
                _value = _value.Remove(breakoutIndex);
            }

            string[] values = _value.Split('.');
            values [ values.Length - 1 ] += functionBreakout;

            return values;
        }
    }
}
