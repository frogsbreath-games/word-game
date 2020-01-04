using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordGame.API
{
	public class ApiResponse<TData>
	{
		public TData Data { get; protected set; }

		public string Message { get; protected set; }

		public ApiError[] Errors { get; protected set; }

		public ApiResponse(TData data)
			=> Data = data;

		public static implicit operator ApiResponse<TData>(TData data)
			=> new ApiResponse<TData>(data);
	}

	public class ApiError
	{
		public string Code { get; protected set; }
		public string Message { get; protected set; }
	}
}
