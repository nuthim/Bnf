using System;
using System.Collections.Generic;


namespace Bnf.Serialization
{
    public class BnfSettings
    {
        #region Constants
        public const string DefaultNullText = null;
        private readonly IDictionary<Type, string> _formatStrings;
        #endregion

        #region Fields
        private string _nullText;
        #endregion

        #region Constructor

        public BnfSettings()
        {
            EscapeCodes = new Dictionary<char, string>();
            EscapeCodes.Add('{', @"{{");
            EscapeCodes.Add('}', @"}}");
            EscapeCodes.Add('|', @"||");

            _formatStrings = new Dictionary<Type, string>();
        }

        #endregion

        #region Properties

        public string NullText
        {
            get { return _nullText ?? _nullText; }
            set { _nullText = value; }
        }

        public IDictionary<char, string> EscapeCodes
        {
            get; set;
        }


        #endregion

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

    }
}
