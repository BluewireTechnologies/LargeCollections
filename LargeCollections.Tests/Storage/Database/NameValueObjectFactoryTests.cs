using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Gallio.Framework;
using LargeCollections.Storage.Database;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

namespace LargeCollections.Tests.Storage.Database
{
    [TestFixture]
    public class NameValueObjectFactoryTests
    {
        public class SimpleEntity
        {
            public string StringProperty {get;set;}
            public int IntField;
            public int ReadOnlyIntProperty {get; private set; }
            public readonly string ReadOnlyStringField;
        }

        public class EntityWithConstructor
        {
            public EntityWithConstructor(int intField)
            {
                ConstructorIntField = intField;
            }

            public EntityWithConstructor(float stringProperty)
            {
            }

            public string StringProperty {get;set;}
            public int IntField;
            public int ConstructorIntField;
        }

        public class EntityWithUnsatisfiedConstructor
        {
            public EntityWithUnsatisfiedConstructor(float unmappedProperty)
            {
            }
            public int IntField;
        }

        [Test]
        public void CanDeserialiseWritableProperty()
        {   
            var mappings = new INamePropertyMapping<SimpleEntity>[] {  new ColumnPropertyMapping<SimpleEntity, string>("source", e => e.StringProperty,SqlDbType.Variant) };
            var factory = new NameValueObjectFactory<string, SimpleEntity>(mappings);
            var entity = factory.ReadRecord(s => "Test Value");

            Assert.AreEqual("Test Value", entity.StringProperty);
        }

        [Test]
        public void CanDeserialiseWritableField()
        {   
            var mappings = new INamePropertyMapping<SimpleEntity>[] {  new ColumnPropertyMapping<SimpleEntity, int>("source", e => e.IntField, SqlDbType.Variant) };
            var factory = new NameValueObjectFactory<string, SimpleEntity>(mappings);
            var entity = factory.ReadRecord(s => 1);

            Assert.AreEqual(1, entity.IntField);
        }

        [Test]
        public void CannotDeserialiseUnwritableField()
        {   
            var mappings = new INamePropertyMapping<SimpleEntity>[] {  new ColumnPropertyMapping<SimpleEntity, string>("source", e => e.ReadOnlyStringField, SqlDbType.Variant) };
            var factory = new NameValueObjectFactory<string, SimpleEntity>(mappings);
            var entity = factory.ReadRecord(s => "Test Value");

            Assert.AreNotEqual("Test Value", entity.ReadOnlyStringField);
        }

        
        [Test]
        public void CannotDeserialiseUnwritableProperty()
        {   
            var mappings = new INamePropertyMapping<SimpleEntity>[] {  new ColumnPropertyMapping<SimpleEntity, int>("source", e => e.ReadOnlyIntProperty, SqlDbType.Variant) };
            var factory = new NameValueObjectFactory<string, SimpleEntity>(mappings);
            var entity = factory.ReadRecord(s => 1);

            Assert.AreNotEqual(1, entity.ReadOnlyIntProperty);
        }

        [Test]
        public void CanDeserialiseWithConstructorParameterMatchingNameAndType()
        {   
            var mappings = new INamePropertyMapping<EntityWithConstructor>[] {  new ColumnPropertyMapping<EntityWithConstructor, int>("source", e => e.IntField, SqlDbType.Variant) };
            var factory = new NameValueObjectFactory<string, EntityWithConstructor>(mappings);
            var entity = factory.ReadRecord(s => 1);

            Assert.AreEqual(1, entity.ConstructorIntField);
            Assert.AreNotEqual(1, entity.IntField); // shouldn't set property as well as use it for constructor.
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotDeserialiseWithConstructorParameterMatchingNameButNotType()
        {   
            var mappings = new INamePropertyMapping<EntityWithConstructor>[] {  new ColumnPropertyMapping<EntityWithConstructor, string>("source", e => e.StringProperty, SqlDbType.Variant) };
            var factory = new NameValueObjectFactory<string, EntityWithConstructor>(mappings);
            var entity = factory.ReadRecord(s => "test");
        }

        
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotDeserialiseWithoutAllConstructorParametersSatisfied()
        {   
            var mappings = new INamePropertyMapping<EntityWithUnsatisfiedConstructor>[] {  new ColumnPropertyMapping<EntityWithUnsatisfiedConstructor, int>("source", e => e.IntField, SqlDbType.Variant) };
            var factory = new NameValueObjectFactory<string, EntityWithUnsatisfiedConstructor>(mappings);
            var entity = factory.ReadRecord(s => 1);
        }
    }
}
