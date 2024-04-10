using Newtonsoft.Json.Linq;
using static LexicalAnalyzer;

namespace LexicalAnalyzerTests
{
    [TestFixture]
    public class AnalyzerTests
    {
        private static LexicalAnalyzer analyzer;

        [OneTimeSetUp]
        public static void Init()
        {
            analyzer = new LexicalAnalyzer();
        }

        [SetUp]
        public void Setup()
        {
            // Здійснюємо необхідну підготовку перед кожним тестом, якщо потрібно
        }

        [Test]
        public void Analyze_ShouldReturnCorrectTokenCount()
        {
            var code = "int x = 5;";
            List<LexicalAnalyzer.Token> tokens = LexicalAnalyzer.Analyze(code);
            Assert.That(tokens, Has.Count.EqualTo(5));
        }


        [Test]
        public void Analyze_ShouldIdentifyNumbersCorrectly()
        {
            var code = "x = 123;";
            List<Token> tokens = LexicalAnalyzer.Analyze(code);
            Assert.That(tokens[2].Type, Is.EqualTo(TokenType.Number));
        }

        [Test]
        public void Analyze_ShouldThrowExceptionForInvalidInput()
        {
            var code = "\0";
            Assert.Throws<ArgumentException>(() => LexicalAnalyzer.Analyze(code));
        }

        [Test]
        public void Analyze_CommentIsRecognized()
        {
            var code = "// This is a comment";
            List<Token> tokens = LexicalAnalyzer.Analyze(code);
            Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Comment));
        }

        [Test, TestCaseSource(nameof(GetCodeSamples))]
        public void Analyze_UsingParameterizedTests(string code, TokenType expectedType)
        {
            List<Token> tokens = LexicalAnalyzer.Analyze(code);
            Assert.That(tokens[0].Type, Is.EqualTo(expectedType));
        }

        private static IEnumerable<object[]> GetCodeSamples()
        {
            yield return new object[] { "// Comment", TokenType.Comment };
            yield return new object[] { "\"String\"", TokenType.String };
            yield return new object[] { "123", TokenType.Number };
            yield return new object[] { "int _identifier;", TokenType.Reserved };
        }

        [Test]
        public void Analyze_AssumeNotInString()
        {
            var code = "\"This is not terminated";
            Assume.That(() => !code.EndsWith("\""), "Code ends with a quote.");
            List<Token> tokens = LexicalAnalyzer.Analyze(code);
            Assert.That(tokens, Is.Empty);
        }
    }
}