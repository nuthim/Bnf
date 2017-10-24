using System.Collections.Generic;

namespace Bnf.Serialization
{
    public class BnfSettings
    {
        #region Constants
        public const string DefaultNullText = "yyyy-MM-dd";
        public const string DefaultDateFormat = "yyyy-MM-dd";
        public const string DefaultTimeFormat = "hh:mm:ss";
        #endregion

        #region Fields
        private string _nullText;
        private string _dateFormat;
        private string _timeFormat;
        private IDictionary<string, string> _escapeCodes;
        #endregion

        #region Constructor

        public BnfSettings()
        {
            IgnoreNull = true;
            _escapeCodes.Add("{", @"\[");
            _escapeCodes.Add("}", @"\]");
            _escapeCodes.Add(@"\", @"\\");
            _escapeCodes.Add("|", @"\/");
        }

        #endregion

        #region Properties

        public bool IgnoreNull { get; set; }

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

        #endregion
    }
}
