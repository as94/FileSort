using Moq;

namespace FileSort.Tests;

public class StringGeneratorTests
{
    [Test]
    public void WhenGenerateString_ShouldGetOne()
    {
        var idGenerator = new Mock<IIdGenerator>();
        idGenerator.Setup(x => x.Get()).Returns(5);
        var nameGenerator = new Mock<INameGenerator>();
        nameGenerator.Setup(x => x.Get()).Returns("Apple");
        var generator = new StringGenerator(idGenerator.Object, nameGenerator.Object);

        var str = generator.Get();

        Assert.That(str, Is.EqualTo("5. Apple"));
    }
    
    // TODO: test random behaviour 
}