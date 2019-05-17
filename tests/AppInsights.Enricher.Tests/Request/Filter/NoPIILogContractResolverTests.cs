namespace AppInsights.Enricher.Tests.Request.Filter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enricher.Request.Filters;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Shouldly;
    using Xunit;

    internal class NoPIILogContractResolverStub : NoPIILogContractResolver
    {
        public IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) =>
            base.CreateProperties(type, memberSerialization);
    }
    
    public class NoPIILogContractResolverTests
    {
        private NoPIILogContractResolverStub _noPiiLogContractResolver = new NoPIILogContractResolverStub();
        
        internal class EmptyType
        {
            
        }

        internal class SimpleType
        {
            public string Field1 { get; set; }
        }
        
        internal class SimpleSensitiveType
        {
            public string Field1 { get; set; }
            [Sensitive] public string Field2 { get; set; }
        }
        
        internal class ComplexSensitiveType
        {
            public string Field1 { get; set; }
            [Sensitive] public string Field2 { get; set; }
            public SimpleSensitiveType Field3 { get; set; }
        }
        
        internal class ComplexSensitiveTypeWithFields
        {
            public readonly string Field1;
            [Sensitive] public readonly string Field2;
            public readonly SimpleSensitiveType Field3;
        }
        
        [Fact]
        public void CreateProperties_WhenTypeHasNoAnyAttributes_ReturnEmptyList()
        {
            var result = _noPiiLogContractResolver.CreateProperties(typeof(EmptyType), new MemberSerialization());
            
            result.ShouldBeEmpty();
        }
        
        [Fact]
        public void CreateProperties_WhenTypeHasNoAnySensitiveAttributes_ReturnAllPropertiesNotChanged()
        {
            var result = _noPiiLogContractResolver.CreateProperties(typeof(SimpleType), new MemberSerialization());
            
            result.Single().PropertyName.ShouldBe("Field1");
            result.Single().PropertyType.ShouldBe(typeof(string));
        }
        
        
        [Fact]
        public void CreateProperties_WhenTypeHasSensitiveAttributeAboveSingleProperty_ReplacedPropertyOnlyForSensitiveOne()
        {
            var result = _noPiiLogContractResolver.CreateProperties(typeof(SimpleSensitiveType), new MemberSerialization());
            
            result.ElementAt(0).PropertyName.ShouldBe("Field1");
            result.ElementAt(0).PropertyType.ShouldBe(typeof(string));
            result.ElementAt(1).PropertyName.ShouldBe("Field2");
            result.ElementAt(1).PropertyType.ShouldBe(typeof(string));
            result.ElementAt(1).ValueProvider.GetValue("PII Data");
        }
        
        [Fact]
        public void CreateProperties_WhenTypeHasMultipleSensitiveAttributes_ReturnAllPropertiesWithReplacedSensitiveOnes()
        {
            var result = _noPiiLogContractResolver.CreateProperties(typeof(ComplexSensitiveType), new MemberSerialization());
            
            result.ElementAt(0).PropertyName.ShouldBe("Field1");
            result.ElementAt(0).PropertyType.ShouldBe(typeof(string));
            
            result.ElementAt(1).PropertyName.ShouldBe("Field2");
            result.ElementAt(1).PropertyType.ShouldBe(typeof(string));
            result.ElementAt(1).ValueProvider.GetValue("PII Data");
            
            result.ElementAt(2).PropertyName.ShouldBe("Field3");
            result.ElementAt(2).PropertyType.ShouldBe(typeof(SimpleSensitiveType));
        }
        
        [Fact]
        public void CreateProperties_WhenTypeHasMultipleSensitiveAttributesAssignedToFields_ReturnAllPropertiesAndFieldsWithReplacedSensitiveOnes()
        {
            var result = _noPiiLogContractResolver.CreateProperties(typeof(ComplexSensitiveTypeWithFields), new MemberSerialization());
            
            result.ElementAt(0).PropertyName.ShouldBe("Field1");
            result.ElementAt(0).PropertyType.ShouldBe(typeof(string));
            
            result.ElementAt(1).PropertyName.ShouldBe("Field2");
            result.ElementAt(1).PropertyType.ShouldBe(typeof(string));
            result.ElementAt(1).ValueProvider.GetValue("PII Data");
            
            result.ElementAt(2).PropertyName.ShouldBe("Field3");
            result.ElementAt(2).PropertyType.ShouldBe(typeof(SimpleSensitiveType));
        }
    }
}