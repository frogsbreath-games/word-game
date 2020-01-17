using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WordGame.API.Application.Services
{
	public interface IRandomAccessor
	{
		Random Random { get; }
	}

	public class RandomAccessor : IRandomAccessor
	{
		protected readonly Random _rand;

		public RandomAccessor()
		{
			_rand = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
		}

		public Random Random => _rand;
	}
}
