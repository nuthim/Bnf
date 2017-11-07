using System;
using Bnf.Serialization.Infrastructure;

namespace Bnf.Serialization
{
    public class BnfSerializer
    {
        #region Fields
        private BnfSettings _settings;
        public const char FieldSeparator = '|';
        public const char KeyValueSeparator = '=';
        #endregion

        public BnfSettings Settings
        {
            get { return _settings ?? (_settings = new BnfSettings()); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _settings = value;
            }
        }

        #region Constructor

        public BnfSerializer() : this(null)
        {

        }

        public BnfSerializer(BnfSettings settings)
        {
            _settings = settings;
        }

        #endregion

        public string Serialize(object data)
        {
            return new SerializerImpl(Settings, FieldSeparator, KeyValueSeparator).Serialize(data);
        }

        public T Deserialize<T>(string bnf)
        {
            return new DeserializerImpl(Settings, FieldSeparator, KeyValueSeparator).Deserialize<T>(bnf);
        }

        public object Deserialize(string bnf, Type type)
        {
            return new DeserializerImpl(Settings, FieldSeparator, KeyValueSeparator).Deserialize(bnf, type);
        }
    }
}
