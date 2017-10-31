using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bnf.Serialization
{
    public class BnfSettings
    {
        #region Constants
        public const string DefaultNullText = null;
        private readonly IDictionary<Type, string> _formatStrings;
        private readonly IDictionary<char, string> _escapeCodes;
        #endregion

        #region Constructor

        public BnfSettings()
        {
            _escapeCodes = new Dictionary<char, string>();
            _escapeCodes.Add('{', @"{{");
            _escapeCodes.Add('}', @"}}");
            _escapeCodes.Add('|', @"||");

            _formatStrings = new Dictionary<Type, string>();
        }

        #endregion

        #region Properties

        public string NullText { get; set; }

        public IReadOnlyDictionary<char, string> EscapeCodes
        {
            get { return new ReadOnlyDictionary<char, string>(_escapeCodes); }
        }

        public IReadOnlyDictionary<Type, string> FormatStrings
        {
            get { return new ReadOnlyDictionary<Type, string>(_formatStrings); }
        }

        #endregion

        #region Public Methods

        public string GetFormatString(Type type)
        {
            if (type == null)
                return "{0}";

            string format;
            _formatStrings.TryGetValue(type, out format);
            return format;
        }

        public void SetFormatString(Type type, string formatString)
        {
            if (formatString == null)
            {
                _formatStrings.Remove(type);
                return;
            }

            _formatStrings[type] = formatString.Trim();
        }

        public string GetEscapeCode(char escapeChar)
        {
            string escapeCode;
            _escapeCodes.TryGetValue(escapeChar, out escapeCode);
            return escapeCode;
        }

        public void SetEscapeCode(char escapeChar, string escapeCode)
        {
            if (escapeCode == null)
            {
                _escapeCodes.Remove(escapeChar);
                return;
            }


            //TODO: Validate the escape code
            var code = escapeCode.Trim();
            if (code.Length == 1 && code == escapeChar.ToString())
                throw new ArgumentException("Escape code can't be same as the character to be escaped", nameof(escapeCode));

            _escapeCodes[escapeChar] = escapeCode.Trim();
        }

        #endregion
    }
}
