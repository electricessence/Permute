using Open.Disposable;
using System.Buffers;
using System.Diagnostics;

namespace Permute;

internal static class Utility
{

	public static IEnumerable<T[]> RowConfigurations<T>(this IReadOnlyList<IEnumerable<T>> source)
	{
		var listPool = ListPool<IEnumerator<T>>.Shared;
		List<IEnumerator<T>> enumerators = listPool.Take();
		try
		{
			foreach (var e in source.Select(e => e.GetEnumerator()))
			{
				if (!e.MoveNext())
				{
					yield break;
				}
				enumerators.Add(e);
			}

			var count = enumerators.Count;
			Debug.Assert(source.Count == count);

			bool GetNext() => ListPool<int>.Shared.Rent(reset =>
			{
				for (var i = 0; i < count; i++)
				{
					var e = enumerators[i];
					if (e.MoveNext())
					{
						foreach (var r in reset)
						{
							enumerators[r] = e = source[r].GetEnumerator();
							e.MoveNext();
						}
						return true;
					}
					e.Dispose();
					if (i == count - 1) break;
					reset.Add(i);
				}

				return false;
			});


			var arrayPool = ArrayPool<T>.Shared;
			var buffer = arrayPool.Rent(count);
			try
			{
				do
				{
					for (var i = 0; i < count; i++)
					{
						buffer[i] = enumerators[i].Current ?? throw new NullReferenceException();
					}
					yield return buffer;
				}
				while (GetNext());
			}
			finally
			{
				arrayPool.Return(buffer);
			}

		}
		finally
		{
			listPool.Give(enumerators);
		}
	}
}
