namespace MTCG.Exceptions;
public class InvalidQueryResultException : Exception
{
	public InvalidQueryResultException()
	{
		// nothing else to do
	}

	public InvalidQueryResultException(string message)
		: base(message)
	{
		// nothing else to do
	}

	public InvalidQueryResultException(string message, Exception inner)
		: base(message, inner)
	{
		// nothing else to do
	}
}
