using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HansKindberg.Tests.Linq.Expressions
{
	[TestClass]
	public class ExpressionPrerequisiteTest
	{
		#region Methods

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void MakeBinary_IfBinaryTypeParameterIsInvokde_ShouldThrowAnArgumentException()
		{
			try
			{
				// ReSharper disable ReturnValueOfPureMethodIsNotUsed
				Expression.MakeBinary(ExpressionType.Invoke, Expression.Constant(10), Expression.Constant(10));
				// ReSharper restore ReturnValueOfPureMethodIsNotUsed
			}
			catch(ArgumentException argumentException)
			{
				const string parameterName = "binaryType";

				if(argumentException.ParamName == parameterName && argumentException.Message == "Unhandled binary: Invoke" + Environment.NewLine + "Parameter name: " + parameterName)
					throw;
			}
		}

		[TestMethod]
		public void MakeBinary_IfBinaryTypeParameterIsSubtract_ShouldNotThrowAnException()
		{
			var binaryExpression = Expression.MakeBinary(ExpressionType.Subtract, Expression.Constant(10), Expression.Constant(10));

			Assert.IsNotNull(binaryExpression);

			Assert.AreEqual("(10 - 10)", binaryExpression.ToString());
		}

		#endregion
	}
}