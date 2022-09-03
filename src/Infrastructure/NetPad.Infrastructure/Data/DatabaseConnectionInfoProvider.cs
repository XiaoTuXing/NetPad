using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetPad.Assemblies;

namespace NetPad.Data;

public class DatabaseConnectionInfoProvider : IDatabaseConnectionInfoProvider
{
    private readonly IDataConnectionResourcesCache _dataConnectionResourcesCache;
    private readonly IAssemblyLoader _assemblyLoader;

    public DatabaseConnectionInfoProvider(IDataConnectionResourcesCache dataConnectionResourcesCache, IAssemblyLoader assemblyLoader)
    {
        _dataConnectionResourcesCache = dataConnectionResourcesCache;
        _assemblyLoader = assemblyLoader;
    }

    public async Task<DatabaseStructure> GetDatabaseStructureAsync(DatabaseConnection databaseConnection)
    {
        if (databaseConnection is not EntityFrameworkDatabaseConnection dbConnection)
        {
            throw new InvalidOperationException("Cannot get structure except on Entity Framework database connections.");
        }

        var assemblyBytes = await _dataConnectionResourcesCache.GetAssemblyAsync(dbConnection);
        if (assemblyBytes == null)
        {
            return new DatabaseStructure(dbConnection.DatabaseName ?? string.Empty);
        }

        var assembly = _assemblyLoader.LoadFrom(assemblyBytes);

        var dbContextType = assembly.GetExportedTypes().FirstOrDefault(x => typeof(DbContext).IsAssignableFrom(x));
        if (dbContextType == null)
        {
            throw new Exception("Could not find a type in data connection assembly of type DbContext.");
        }

        var dbContextOptionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(dbContextType);
        var dbContextOptionsBuilder = Activator.CreateInstance(dbContextOptionsBuilderType) as DbContextOptionsBuilder;
        if (dbContextOptionsBuilder == null)
        {
            throw new Exception($"Could not create DbContextOptionsBuilder<> for DbContext of type {dbContextType.FullName}.");
        }

        await dbConnection.ConfigureDbContextOptionsAsync(dbContextOptionsBuilder);

        var ctor = dbContextType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(c => c.GetParameters().Length == 1);
        if (ctor == null)
        {
            throw new Exception($"Could not create find the right constructor on DbContext of type {dbContextType.FullName}.");
        }

        await using var dbContext = ctor.Invoke(new object?[] { dbContextOptionsBuilder.Options }) as DbContext;

        if (dbContext == null)
        {
            throw new Exception($"Could not create a DbContext of type {dbContextType.FullName}.");
        }

        return dbContext.GetDatabaseStructure();
    }
}
