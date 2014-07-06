using System;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Classy.Models.Serializers
{
    [BsonSerializer(typeof(MongoDbMoneyFieldSerializer))]
    public class MongoDbMoneyFieldSerializer : IBsonSerializer
    {
        public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            var dbData = bsonReader.ReadInt32();
            return (decimal)dbData / (decimal)100;
        }

        public object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            var dbData = bsonReader.ReadInt32();
            return (decimal)dbData / (decimal)100;
        }

        public IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            return new DocumentSerializationOptions();
        }

        public void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            var realValue = (decimal) value;
            bsonWriter.WriteInt32(Convert.ToInt32(realValue * 100));
        }
    }
}