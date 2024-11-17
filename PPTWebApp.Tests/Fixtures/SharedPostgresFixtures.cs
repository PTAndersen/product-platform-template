namespace PPTWebApp.Tests.Fixtures;

using System;
using Xunit;

/// <summary>
/// A fixture that ensures the shared PostgreSQL container is started before any tests run and stopped afterward.
/// </summary>
public class SharedPostgresFixture : IDisposable
{
    public string ConnectionString { get; }

    public SharedPostgresFixture()
    {
        ConnectionString = SharedPostgresContainer.GetConnectionString();
    }

    public void Dispose()
    {
        SharedPostgresContainer.StopContainer();
    }
}

/// <summary>
/// Defines a shared collection for test classes using the SharedPostgresFixture.
/// Ensures that all test classes in this collection share the same fixture.
/// </summary>
[CollectionDefinition("SharedPostgresCollection")]
public class SharedPostgresCollection : ICollectionFixture<SharedPostgresFixture>
{
    
}
