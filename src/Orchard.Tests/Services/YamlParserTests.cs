using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Orchard.Services;

namespace Orchard.Tests.Services {

    [TestFixture]
    public class YamlParserTests {
        
        [Test]
        public void ShouldConvertYamlToWellknowType() {
            var parser = new YamlParser();
            var yaml = SampleYamlDocument;

            var order = parser.Deserialize<Order>(yaml);

            Assert.AreEqual("Nikola", order.Customer.FirstName);
            Assert.AreEqual(2, order.Items.Count);
        }

        [Test]
        public void ShouldConvertYamlToDynamic()
        {
            var parser = new YamlParser();
            var yaml = SampleYamlDocument;

            var order = parser.Deserialize(yaml);

            Assert.AreEqual("Nikola", (string)order.Customer.FirstName);
            Assert.AreEqual(2, (int)order.Items.Count);
        }

        public class Order {
            public DateTime Date { get; set; }
            public Customer Customer { get; set; }
            public IList<OrderItem> Items { get; set; }
        }

        public class OrderItem {
            public string Product { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }

        public class Customer {
            public string FirstName { get; set; }
            public string LastName { get; set; }

        }

        private const string SampleYamlDocument =
@"
Date: 1916-04-01
Customer:
    FirstName: Nikola
    LastName: Tesla
Items:
    - Product: Bulb
      Quantity: 1
      Price: 1.46
      
    - Product: Wire
      Quantity: 1
      Price: 0.32
";
    }
}