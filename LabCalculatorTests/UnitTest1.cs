using NUnit.Framework;
using Antlr4.Runtime;

using LabCalculator;

namespace LabCalculator.Tests
{
    [TestFixture]
    public class LabCalculatorTests
    {
        private LabCalculatorVisitor _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = new LabCalculatorVisitor();
        }

        private double EvaluateExpression(string expression)
        {
            var inputStream = new AntlrInputStream(expression);
            var lexer = new LabCalculatorLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new LabCalculatorParser(tokenStream);
            var context = parser.compileUnit();

            return _calculator.VisitCompileUnit(context);
        }

        [Test]
        public void TestModuloOperation()
        {
            // Test: 10 mod 3
            var result = EvaluateExpression("10 mod 3");
            Assert.AreEqual(1, result);
        }

        [Test]
        public void TestDivisionOperation()
        {
            // Test: 10 div 3
            var result = EvaluateExpression("10 div 3");
            Assert.AreEqual(3, result);
        }

        [Test]
        public void TestAdditionOperation()
        {
            // Test: 5 + 3
            var result = EvaluateExpression("5 + 3");
            Assert.AreEqual(8, result);
        }

        [Test]
        public void TestSubtractionOperation()
        {
            // Test: 5 - 3
            var result = EvaluateExpression("5 - 3");
            Assert.AreEqual(2, result);
        }

        [Test]
        public void TestMultiplicationOperation()
        {
            // Test: 5 * 3
            var result = EvaluateExpression("5 * 3");
            Assert.AreEqual(15, result);
        }

        [Test]
        public void TestDivisionWithDecimalOperation()
        {
            // Test: 10 / 4
            var result = EvaluateExpression("10 / 4");
            Assert.AreEqual(2.5, result);
        }

        [Test]
        public void TestExponentiationOperation()
        {
            // Test: 2 ^ 3
            var result = EvaluateExpression("2 ^ 3");
            Assert.AreEqual(8, result);
        }

        [Test]
        public void TestIncrementOperation()
        {
            // Test: inc 5
            var result = EvaluateExpression("inc 5");
            Assert.AreEqual(6, result);
        }

        [Test]
        public void TestDecrementOperation()
        {
            // Test: dec 5
            var result = EvaluateExpression("dec 5");
            Assert.AreEqual(4, result);
        }

        [Test]
        public void TestParenthesizedExpression()
        {
            // Test: (5 + 3) * 2
            var result = EvaluateExpression("(5 + 3) * 2");
            Assert.AreEqual(16, result);
        }
    }
}
