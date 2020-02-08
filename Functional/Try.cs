namespace WordGame.API.Functional
{
	public abstract class Try
	{
		public static Try Success => new TrySuccess();

		public static Try Failure(string message) => new TryFailure(message);

		public bool IsSuccess() => this is TrySuccess;

		public bool IsFailure(out string message)
		{
			message = string.Empty;
			if (this is TryFailure failure)
			{
				message = failure.Message;
				return true;
			}
			return false;
		}

		public static implicit operator bool(Try t) => t.IsSuccess();
	}

	public class TrySuccess : Try { }

	public class TryFailure : Try
	{
		public string Message { get; }

		public TryFailure(string message) => Message = message;
	}
}
