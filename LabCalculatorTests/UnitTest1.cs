using NUnit.Framework;
using Antlr4.Runtime;
using LabCalculator;

namespace LabCalculator.Tests
{
    public abstract class TestBase
    {
        protected LabCalculatorVisitor Calculator;

        [SetUp]
        public void SetUp()
        {
            Calculator = new LabCalculatorVisitor();
        }

        protected double EvaluateExpression(string expression)
        {
            var inputStream = new AntlrInputStream(expression);
            var lexer = new LabCalculatorLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new LabCalculatorParser(tokenStream);
            var context = parser.compileUnit();

            return Calculator.VisitCompileUnit(context);
        }
    }

    [TestFixture]
    public class BinaryOperationTests : TestBase
    {
        [Test]
        public void TestAdditionOperation()
        {
            var result = EvaluateExpression("5 + 3");
            Assert.AreEqual(8, result);
        }

        [Test]
        public void TestSubtractionOperation()
        {
            var result = EvaluateExpression("5 - 3");
            Assert.AreEqual(2, result);
        }

        [Test]
        public void TestMultiplicationOperation()
        {
            var result = EvaluateExpression("5 * 3");
            Assert.AreEqual(15, result);
        }

        [Test]
        public void TestDivisionOperation()
        {
            var result = EvaluateExpression("10 div 3");
            Assert.AreEqual(3, result);
        }

        [Test]
        public void TestDivisionWithDecimalOperation()
        {
            var result = EvaluateExpression("10 / 4");
            Assert.AreEqual(2.5, result);
        }

        [Test]
        public void TestModuloOperation()
        {
            var result = EvaluateExpression("10 mod 3");
            Assert.AreEqual(1, result);
        }
    }

    [TestFixture]
    public class UnaryOperationTests : TestBase
    {
        [Test]
        public void TestIncrementOperation()
        {
            var result = EvaluateExpression("inc 5");
            Assert.AreEqual(6, result);
        }

        [Test]
        public void TestDecrementOperation()
        {
            var result = EvaluateExpression("dec 5");
            Assert.AreEqual(4, result);
        }

        [Test]
        public void TestExponentiationOperation()
        {
            var result = EvaluateExpression("2 ^ 3");
            Assert.AreEqual(8, result);
        }
    }

    [TestFixture]
    public class ExpressionEvaluationTests : TestBase
    {
        [Test]
        public void TestParenthesizedExpression()
        {
            var result = EvaluateExpression("(5 + 3) * 2");
            Assert.AreEqual(16, result);
        }
    }
}
