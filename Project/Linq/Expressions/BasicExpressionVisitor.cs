using System;
using System.Globalization;
using Remotion.Linq.Parsing;

namespace HansKindberg.Linq.Expressions
{
	public abstract class BasicExpressionVisitor : ThrowingExpressionVisitor
	{
		#region Methods

		protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
		{
			return new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The expression \"{0}\", of type \"{1}\", handled by visit-method \"{2}\" is not supported by this LINQ-provider.", unhandledItem, typeof(T), visitMethod));
		}

		#endregion
	}
}