using System;
using System.Dynamic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Bnf.Serialization
{
    public class ExpandoObjectSerializableWrapper : IXmlSerializable
    {
        #region Fields

        private readonly string _name;

        #endregion

        #region Properties
        public ExpandoObject WrappedObject { get; }

        #endregion

        #region Constructor

        public ExpandoObjectSerializableWrapper()
        {

        }

        public ExpandoObjectSerializableWrapper(ExpandoObject expandoObject) : this(null, expandoObject)
        {

        }

        private ExpandoObjectSerializableWrapper(string name, ExpandoObject expandoObject)
        {
            _name = name;
            WrappedObject = expandoObject;
        }

        #endregion

        #region IXmlSerializable Implementation

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            if (_name != null)
                writer.WriteStartElement(_name);

            if (WrappedObject == null)
                return;

            foreach (var item in WrappedObject)
            {
                var expando = item.Value as ExpandoObject;
                if (expando != null)
                    new ExpandoObjectSerializableWrapper(item.Key, expando).WriteXml(writer);
                else
                    writer.WriteElementString(item.Key, item.Value.ToString());
            }

            if (_name != null)
                writer.WriteEndElement();
        }

        #endregion
    }
}