using System.Collections.Generic;

namespace Bnf.Serialization
{
    public class BnfSettings
    {
        #region Constants
        public const string DefaultNullText = null;
        public const string DefaultDateFormat = "{0:yyyy-MM-dd}";
        public const string DefaultTimeFormat = "{0:hh\\:mm\\:ss}";
        #endregion

        #region Fields
        private string _nullText;
        private string _dateFormat;
        private string _timeFormat;
        private IDictionary<char, string> _escapeCodes;
        #endregion

        #region Constructor

        public BnfSettings()
        {
            _escapeCodes = new Dictionary<char, string>();
            _escapeCodes.Add('{', @"{{");
            _escapeCodes.Add('}', @"}}");
            _escapeCodes.Add('|', @"||");
        }

        #endregion

        #region Properties

        public string NullText
        {
            get { return _nullText ?? _nullText; }
            set { _nullText = value; }
        }

        public string DateFormat
        {
            get { return _dateFormat ?? DefaultDateFormat; }
            set { _dateFormat = value; }
        }

        public string TimeFormat
        {
            get { return _timeFormat ?? DefaultTimeFormat; }
            set { _timeFormat = value; }
        }

        public IDictionary<char, string> EscapeCodes
        {
            get { return _escapeCodes; }
            set
            {
                ValidateEscapeCodes(value);
                _escapeCodes = value;
            }
        }

        #endregion

        private void ValidateEscapeCodes(IDictionary<char, string> _escapeCodes)
        {
            return;
        }
    }
}
