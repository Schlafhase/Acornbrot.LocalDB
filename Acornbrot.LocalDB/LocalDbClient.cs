using Acornbrot.LocalDB.Interfaces;
using Newtonsoft.Json;

namespace Acornbrot.LocalDB;

public class LocalDbClient
{
	private readonly DirectoryInfo _root;

	public LocalDbClient(string root, bool createIfNotExists = false)
	{
		if (Path.Exists(root))
		{
			_root = new DirectoryInfo(root);
		}
		else
		{
			_root = createIfNotExists ? Directory.CreateDirectory(root) : throw new DirectoryNotFoundException();
		}
	}

	public async Task CreateAsync(IDbObject item)
	{
		string path = Path.Combine(_root.FullName, item.Id.ToString());

		if (File.Exists(path))
		{
			throw new InvalidOperationException("Item already exists.");
		}

		File.Create(path).Close();
		TextWriter tw = new StreamWriter(path);
		await tw.WriteAsync(JsonConvert.SerializeObject(item));
		tw.Close();
	}

	public async ValueTask<T> GetAsync<T>(Guid id)
	{
		string path = Path.Combine(_root.FullName, id.ToString());

		if (!File.Exists(path))
		{
			throw new FileNotFoundException();
		}

		TextReader tr = new StreamReader(path);
		string json = await tr.ReadToEndAsync();
		tr.Close();
		return JsonConvert.DeserializeObject<T>(json) ?? throw new InvalidOperationException("Deserialization failed.");
	}

	public async Task ReplaceAsync(IDbObject item)
	{
		string path = Path.Combine(_root.FullName, item.Id.ToString());

		if (!File.Exists(path))
		{
			throw new FileNotFoundException();
		}

		File.Create(path).Close();

		TextWriter tw = new StreamWriter(path);
		await tw.WriteAsync(JsonConvert.SerializeObject(item));
		tw.Close();
	}

	public Task DeleteAsync(Guid id)
	{
		string path = Path.Combine(_root.FullName, id.ToString());

		if (!File.Exists(path))
		{
			throw new FileNotFoundException();
		}

		File.Delete(path);
		return Task.CompletedTask;
	}

	public IOrderedQueryable<T> GetItemLinqQueryable<T>() where T : IDbObject
	{
		return _root.EnumerateFiles()
					.Select(file =>
					{
						try
						{
							using TextReader tr = new StreamReader(file.FullName);
							string json = tr.ReadToEnd();
							T result = JsonConvert.DeserializeObject<T>(json)!;
							return result;
						}
						catch(Exception e) 
						{
							return default;
						}
					})
					.Where(item => item is not null)
					.ToArray()
					// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
					.AsQueryable()
					.OrderBy(item => item.Id);
	}
	
	public List<T> ToList<T>(IQueryable<T> query)
	{
		try
		{
			return query.AsEnumerable().ToList();
		}
		catch (Exception e)
		{
			throw;
		}
	}
}