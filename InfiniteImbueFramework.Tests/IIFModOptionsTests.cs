using InfiniteImbueFramework;
using InfiniteImbueFramework.Configuration;
using NUnit.Framework;

namespace InfiniteImbueFramework.Tests
{
    [TestFixture]
    public class IIFModOptionsTests
    {
        [SetUp]
        public void SetUp()
        {
            IIFModOptions.LogLevel = "Basic";
        }

        [Test]
        public void Version_IsSemverLike()
        {
            Assert.That(IIFModOptions.VERSION, Is.Not.Null.And.Not.Empty);
            Assert.That(IIFModOptions.VERSION, Does.Match("^\\d+\\.\\d+\\.\\d+"));
        }

        [Test]
        public void CurrentLogLevel_MapsFromOptions()
        {
            IIFModOptions.LogLevel = "Verbose";
            Assert.That(IIFLog.CurrentLevel, Is.EqualTo(IIFLogLevel.Verbose));

            IIFModOptions.LogLevel = "Off";
            Assert.That(IIFLog.CurrentLevel, Is.EqualTo(IIFLogLevel.Off));
        }
    }
}
