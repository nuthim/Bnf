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
            _escapeCodes = new Dictionary<char, string>
            {
                {'{', @"\["},
                { '}', @"\]"},
                { '\\', @"\\"},
                { '|', @"\/"}
            };

            _formatStrings = new Dictionary<Type, string>();
        }

        #endregion

        #region Properties

        public string NullText { get; set; }

        public IReadOnlyDictionary<char, string> EscapeCodes => new ReadOnlyDictionary<char, string>(_escapeCodes);

        public IReadOnlyDictionary<Type, string> FormatStrings => new ReadOnlyDictionary<Type, string>(_formatStrings);

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

        #endregion
    }
}
