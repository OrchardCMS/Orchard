using System.Security.Cryptography;
using System.Text;
using Autofac;
using NUnit.Framework;
using Orchard.Environment.Configuration;
using Orchard.Security;
using Orchard.Security.Providers;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Security {
    [TestFixture]
    public class DefaultEncryptionServiceTests {
        private IContainer container;

        [SetUp]
        public void Init() {

            var key = new byte[32];
            var iv = new byte[16];
            using ( var random = new RNGCryptoServiceProvider() ) {
                random.GetBytes(key);
                random.GetBytes(iv);
            }

            var shellSettings = new ShellSettings {
                Name = "Foo",
                DataProvider = "Bar",
                DataConnectionString = "Quux",
                EncryptionAlgorithm = "AES",
                EncryptionKey = key.ToHexString(),
                EncryptionIV = iv.ToHexString()
            };

            var builder = new ContainerBuilder();
            builder.RegisterInstance(shellSettings);
            builder.RegisterType<DefaultEncryptionService>().As<IEncryptionService>();
            container = builder.Build();
        }

        [Test]
        public void CanEncodeAndDecodeData() {
            var encryptionService = container.Resolve<IEncryptionService>();

            var secretData = Encoding.Unicode.GetBytes("this is secret data");
            var encrypted = encryptionService.Encode(secretData);
            var decrypted = encryptionService.Decode(encrypted);

            Assert.That(encrypted, Is.Not.EqualTo(decrypted));
            Assert.That(decrypted, Is.EqualTo(secretData));
        }

    }
}