# Butterfly.Db ![Butterfly Logo](https://raw.githubusercontent.com/firesharkstudios/Butterfly/master/img/logo-40x40.png) 

> Access a database without an ORM and subscribe to database change events in C#

## Getting Started

## Install from Nuget

| Name | Package | Install |
| --- | --- | --- |
| Butterfly.Db | [![nuget](https://img.shields.io/nuget/v/Butterfly.Db.svg)](https://www.nuget.org/packages/Butterfly.Db/) | `nuget install Butterfly.Db` |
| Butterfly.Db.MySql | [![nuget](https://img.shields.io/nuget/v/Butterfly.Db.MySql.svg)](https://www.nuget.org/packages/Butterfly.Db.MySql/) | `nuget install Butterfly.Db.MySql` |
| Butterfly.Db.Postgres | [![nuget](https://img.shields.io/nuget/v/Butterfly.Db.Postgres.svg)](https://www.nuget.org/packages/Butterfly.Db.Postgres/) | `nuget install Butterfly.Db.Postgres` |
| Butterfly.Db.SQLite | [![nuget](https://img.shields.io/nuget/v/Butterfly.Db.SQLite.svg)](https://www.nuget.org/packages/Butterfly.Db.SQLite/) | `nuget install Butterfly.Db.SQLite` |
| Butterfly.Db.SqlServer | [![nuget](https://img.shields.io/nuget/v/Butterfly.Db.SqlServer.svg)](https://www.nuget.org/packages/Butterfly.Db.SqlServer/) | `nuget install Butterfly.Db.SqlServer` |

### Install from Source Code

git clone https://github.com/firesharkstudios/butterfly-db

## Overview

*Butterfly.Db* defines interfaces to access a database and receive events when a database changes.

*Butterfly.Db.Mysql*, *Butterfly.Db.Postgres*, *Butterfly.Db.SQLite*, and *Butterfly.Db.Postgress* implement the Butterfly.Db interfaces in the respective databases.

## Accessing a Database

### Overview

An [IDatabase](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.IDatabase.html) instance allows modifying data, selecting data, and creating *DynamicViews*.

```cs
var id = await database.InsertAndCommitAsync<string>("todo", new {
    name = "My Todo"
});
await database.UpdateAndCommitAsync("todo", new {
    id,
    name = "My New Todo"
});
await database.DeleteAndCommitAsync("todo", id);

var name = await database.SelectValueAsync<string>("SELECT name FROM todo", id);
```

### Selecting Data

There are four flavors of selecting data with different return values...

| Method | Description |
| --- | --- |
| [SelectRowAsync()](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.IDatabase.html#Butterfly_Core_Database_IDatabase_SelectRowAsync_System_String_System_Object_) | Returns a single *Dict* instance |
| [SelectRowsAsync()](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.IDatabase.html#Butterfly_Core_Database_IDatabase_SelectRowsAsync_System_String_System_Object_System_Int32_) | Returns an array of *Dict* instances |
| [SelectValueAsync<T>()](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.IDatabase.html#Butterfly_Core_Database_IDatabase_SelectValueAsync__1_System_String_System_Object___0_) | Returns a single value |
| [SelectValuesAsync<T>()](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.IDatabase.html#Butterfly_Core_Database_IDatabase_SelectValueAsync__1_System_String_System_Object___0_) | Returns an array of values |

Each flavor above takes a *sql* parameter and optional *values* parameter.

The *sql* parameter can be specified in multiple ways...

| Name | Example Value |
| --- | --- | --- |
| Table name only | `"todo"` |
| SELECT without WHERE | `"SELECT * FROM todo"` |
| Full SELECT| `"SELECT * FROM todo WHERE id=@id"` |

The *values* parameter can also be specified in multiple ways...

| Name | Example Value |
| --- | --- | --- |
| Anonymous type | `new { id = "123" }` |
| Dictionary | `new Dict { ["id"] = "123" }` |
| Primary Key Value | `"123"` |

Specific value types will also cause a WHERE clause to be rewritten as follows...

| Original WHERE | Values | New WHERE |
| --- | --- | --- |
| WHERE test=@test | `new { test = (string)null }` | WHERE test IS NULL |
| WHERE test!=@test | `new { test = (string)null }` | WHERE test IS NOT NULL |
| WHERE test=@test | `new { test = new string[] {"123","456") }` | WHERE test IN ('123', '456') |
| WHERE test!=@test | `new { test = new string[] {"123","456") }` | WHERE test NOT IN ('123', '456') |

So, these are all valid examples...

```cs
// Both of these effectively run SELECT * FROM employee
Dict[] allEmployees1 = await database.SelectRowsAsync("employee");
Dict[] allEmployees2 = await database.SelectRowsAsync("SELECT * FROM employee");

// Both of these effectively run SELECT * FROM employee WHERE department_id="123"_
Dict[] departmentEmployees1 = await database.SelectRowsAsync("employee", new {
    department_id = "123"
});
Dict[] departmentEmployees1 = await database.SelectRowsAsync("employee", new Dict {
    { "department_id", "123" }
});

// All three of these effectively run SELECT name FROM employee WHERE id='123'
string name1 = await database.SelectValueAsync<string>("SELECT name FROM employee", "123");
string name2 = await database.SelectValueAsync<string>("SELECT name FROM employee", new {
    id = "123"
});
string name3 = await database.SelectValueAsync<string>("SELECT name FROM employee", new Dict {
    { "id", "123" },
});

// Effectively runs SELECT * FROM employee WHERE department_id IS NULL
Dict[] rows = await database.SelectRowsAsync("employee", new {
    department_id = (string)null
});

// Effectively runs SELECT * FROM employee WHERE department_id IS NOT NULL
Dict[] rows = await database.SelectRowsAsync("SELECT * employee WHERE department_id!=@department_id", new {
    department_id = (string)null
});

// Effectively runs SELECT * FROM employee WHERE department_id IN ('123', '456')
Dict[] rows = await database.SelectRowsAsync("employee", new {
    department_id = new string[] { "123", "456"}
});

// Effectively runs SELECT * FROM employee WHERE department_id NOT IN ('123', '456')
Dict[] rows = await database.SelectRowsAsync("SELECT * employee WHERE department_id!=@department_id", new {
    department_id = new string[] { "123", "456"}
});
```
### Modifying Data

A [IDatabase](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.IDatabase.html) instance has convenience methods that create a transaction, perform a specific action, and commit the transaction as follows...

```cs
// Execute a single INSERT and return the value of the primary key
string id = database.InsertAndCommitAsync<string>("employee", new {
	first_name = "Jim",
	last_name = "Smith",
	balance = 0.0f,
});

// Assuming the employee table has a unique index on the id field, 
// this updates the balance field on the matching record
database.UpdateAndCommitAsync<string>("employee", new {
	id = "123",
	balance = 0.0f,
});

// Assuming the employee table has a unique index on the id field, 
// this deletes the matching record
database.DeleteAndCommitAsync<string>("employee", "123");
```

In addition, you can explicitly create and commit a transaction that performs multiple actions...

```cs
// If either INSERT fails, neither INSERT will be saved
using (ITransaction transaction = await database.BeginTransactionAsync()) {
	string departmentId = transaction.InsertAsync<string>("department", new {
		name = "Sales"
	});
	string employeeId = transaction.InsertAsync<string>("employee", new {
		name = "Jim Smith",
		department_id = departmentId,
	});

    // Don't forget to Commit the transaction
	await transaction.CommitAsync();
}
```

Sometimes, it's useful to run code after a transaction is committed, this can be done using [OnCommit](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.ITransaction.html#Butterfly_Core_Database_ITransaction_OnCommit_Func_Task__) to register an action that will execute after the transaction is committed.

### Synchronizing Data

It's common to synchronize a set of records in the database with a new set of inputs.  

The [SynchronizeAsync](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.ITransaction.html#Butterfly_Core_Database_ITransaction_SynchronizeAsync_System_String_System_Collections_Generic_Dictionary_System_String_System_Object____System_Collections_Generic_Dictionary_System_String_System_Object____Func_System_Collections_Generic_Dictionary_System_String_System_Object__System_Object__System_String___) can be used to determine the right INSERT, UPDATE, and DELETE statements to synchronize two collections...

```cs
// Assumes an article_tag table with article_id and tag_name fields
public async Task SynchronizeTags(string articleId, string[] tagNames) {
    // First, retrieve the existing records from the database
    Dict[] existingRecords = database.SelectRowsAsync(
        @"SELECT article_id, tag_name 
        FROM article_tag 
        WHERE article_id=@articleId",
        new {
            articleId
        }
    );

    // Next, create the new records collection from the tagNames parameter
    Dict[] newRecords = tagNames.Select(x => new Dict {
        { "article_id", articleId },
        { "tag_name", x },
    }).ToArray();

    // Now, execute SynchronizeAsync() to determine the right 
    // INSERT, UPDATE, and DELETE statements to make the collections match
    using (ITransaction transaction = database.BeginTransactionAsync()) {
        await transaction.SynchronizeAsync(
            "article_tag", 
            existingRecords, 
            newRecords
        );
    }
}
```

### Defaults, Overrides, and Preprocessors

A [IDatabase](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.IDatabase.html) instance allows defining...

- Default Values (applies to INSERTs)
- Override Values (applies to INSERTs and UPDATEs)
- Input Proprocessors

Each can be defined globally or per table.

Examples...

```cs
// Add an id field to any INSERT with values like at_58b5fff4-322b-4fe8-b45d-386dac7a79f9
// if INSERTing on an auth_token table
database.SetDefaultValue(
    "id", 
    tableName => $"{tableName.Abbreviate()}_{Guid.NewGuid().ToString()}"
);

// Add a created_at field to any INSERT with the current time
database.SetDefaultValue("created_at", tableName => DateTime.Now.ToUnixTimestamp());

// Add an updated_at field to any INSERT or UPDATE with the current time
this.database.SetOverrideValue("updated_at", tableName => DateTime.Now.ToUnixTimestamp());

// Remap any DateTime values to UNIX timestamp values
database.AddInputPreprocessor(BaseDatabase.RemapTypeInputPreprocessor<DateTime>(
    dateTime => dateTime.ToUnixTimestamp()
));

// Remap any $NOW$ values to the current UNIX timestamp
database.AddInputPreprocessor(BaseDatabase.RemapTypeInputPreprocessor<string>(
    text => text=="$NOW$" ? DateTime.Now.ToUnixTimestamp().ToString() : text
));

// Remap any $UPDATE_AT$ values to be the same value as the updated_at field
database.AddInputPreprocessor(BaseDatabase.CopyFieldValue("$UPDATED_AT$", "updated_at"));
```

## Using Dynamic Views

### Overview

A [DynamicViewSet](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Dynamic.DynamicViewSet.html) allows...

- Defining multiple [DynamicView](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Dynamic.DynamicView.html) instances using a familiar SELECT syntax
- Publishing the initial rows as a single [DataEventTransaction](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Event.DataEventTransaction.html) instance
- Publishing any changes as new [DataEventTransaction](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Event.DataEventTransaction.html) instances

Each [DynamicView](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Dynamic.DynamicView.html) instance must...

- Have a unique name (defaults to the first table name in the SELECT) within a [DynamicViewSet](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Dynamic.DynamicViewSet.html)
- Have key field(s) that uniquely identify each row (defaults to the primary key of the first table in the SELECT) 

You can use the [Butterfly Client](#butterfly-client) libraries to consume these [DataEventTransaction](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Event.DataEventTransaction.html) instances to keep local javascript arrays synchronized with your server.

Key limitations...

- Only INSERTs, UPDATEs, and DELETEs executed via an [IDatabase](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.IDatabase.html) instance will trigger data change events
- SELECT statements with UNIONs are not supported
- SELECT statements with subqueries may not be supported depending on the type of subquery
- SELECT statements with multiple references to the same table can only trigger updates on one of the references

A [DynamicView](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Dynamic.DynamicView.html) will execute additional modified SELECT statements on each underlying data change event.  These modified SELECT statements are designed to execute quickly (always includes a primary key of an underlying table); however, this is additional overhead that should be considered on higher traffic implementations.

### Example

Here is an example of creating a [DynamicViewSet](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Dynamic.DynamicViewSet.html) and triggering [DataEventTransaction](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Event.DataEventTransaction.html) instances by starting the [DynamicViewSet](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Dynamic.DynamicViewSet.html) and by executing an INSERT...
```cs
var dynamicViewSet = database.CreateAndStartDynamicViewAsync(
    @"SELECT t.id, t.name todo_name, u.name user_name
    FROM todo t 
        INNER JOIN user u ON t.user_id=u.id
    WHERE is_done=@isDoneFilter",
    dataEventTransaction => {
        var json = JsonUtil.Serialize(dataEventTransaction, format: true);
        Console.WriteLine($"dataEventTransaction={json}");
    },
    new {
        isDoneFilter = "Y"
    }
);
dynamicViewSet.Start();
```

The above code would cause a [DataEventTransaction](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Event.DataEventTransaction.html) like this to be echoed to the console...

```js
dataEventTransaction={
  "dateTime": "2018-08-24 14:25:59",
  "dataEvents": [
    {
      "name": "todo",
      "keyFieldNames": [
        "id"
      ],
      "dataEventType": "InitialBegin",
      "id": "f916082a-7e56-4974-8bce-9c0af0792362"
    },
    {
      "record": {
        "id": "t_7dcdaf99-50ab-4bd5-ab26-271974e9cc49",
        "todo_name": "Todo #4",
        "user_name": "Patrick"
      },
      "name": "todo",
      "keyValue": "t_7dcdaf99-50ab-4bd5-ab26-271974e9cc49",
      "dataEventType": "Initial",
      "id": "134afc7e-a24e-448a-b800-baed7774d6d2"
    },
    {
      "record": {
        "id": "t_0f2c7147-317b-4f70-851c-dc906db6f2c3",
        "todo_name": "Todo #1",
        "user_name": "Spongebob"
      },
      "name": "todo",
      "keyValue": "t_0f2c7147-317b-4f70-851c-dc906db6f2c3",
      "dataEventType": "Initial",
      "id": "aaa6e491-5ad4-4a2b-9891-b1d402172c46"
    },
    {
      "record": {
        "id": "t_e71e3d82-2153-4b1b-8fcd-29815805307b",
        "todo_name": "Todo #2",
        "user_name": "Spongebob"
      },
      "name": "todo",
      "keyValue": "t_e71e3d82-2153-4b1b-8fcd-29815805307b",
      "dataEventType": "Initial",
      "id": "efea5a4b-9a9c-4bea-bc19-d6a460f27abb"
    },
    {
      "dataEventType": "InitialEnd",
      "id": "f25b8841-b9a3-4ec6-af0a-3d34687fa767"
    }
  ]
}
```

Now, let's add a record that impacts our [DynamicViewSet](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Dynamic.DynamicViewSet.html)...

```cs
await database.InsertAndCommitAsync<string>("todo", new {
    name = "Task #5",
    user_id = spongebobId,
    is_done = "N",
});
```

The above code would trigger the following [DataEventTransaction](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Event.DataEventTransaction.html) to be echoed to the console...

```js
dataEventTransaction={
  "dateTime": "2018-08-24 14:25:59",
  "dataEvents": [
    {
      "record": {
        "id": "t_89378473-97ed-4e0f-9c1d-4303ef6f4d04",
        "todo_name": "Task #5",
        "user_name": "Spongebob"
      },
      "name": "todo",
      "keyValue": "t_89378473-97ed-4e0f-9c1d-4303ef6f4d04",
      "dataEventType": "Insert",
      "id": "e140185e-9636-45e9-9687-a3368ad6caeb"
    }
  ]
}
```

You can run a more robust example [here](https://github.com/firesharkstudios/butterfly-server-dotnet/blob/master/Butterfly.Example.Database/Program.cs).

## Implementations

### Using a Memory Database

[Butterfly.Core.Database.MemoryDatabase](https://butterflyserver.io/docfx/api/Butterfly.Core.Database.Memory.MemoryDatabase.html) database is included in [Butterfly.Core](api/Butterfly.Core.md) and doesn't require installing additional packages; however, *MemoryDatabase* has these key limitattions...

- Data is NOT persisted
- SELECT statements with JOINs are NOT supported

Under the hood, the *MemoryDatabase* is using a System.Data.DataTable instance to manage the data.

In your application...

```csharp
var database = new Butterfly.Core.Database.Memory.MemoryDatabase();
```

### Using MySQL

In the *Package Manager Console*...

```
Install-Package Butterfly.MySql
```

In your application...

```csharp
var database = new Butterfly.MySql.MySqlDatabase("Server=127.0.0.1;Uid=test;Pwd=test!123;Database=butterfly_db_demo");
```

### Using Postgres

In the *Package Manager Console*...

```
Install-Package Butterfly.Postgres
```

In your application...

```csharp
var database = new Butterfly.Postgres.PostgresDatabase("User ID=test;Password=test!123;Host=localhost;Port=5432;Database=test;");
```

### Using SQLite

In the *Package Manager Console*...

```
Install-Package Butterfly.SQLite
```

In your application...

```csharp
var database = new Butterfly.SQLite.SQLiteDatabase("Filename=./my_database.db");
```

### Using MS SQL Server

In the *Package Manager Console*...

```
Install-Package Butterfly.SqlServer
```

In your application...

```csharp
var database = new Butterfly.SqlServer.SqlServerDatabase("Server=localhost; Initial Catalog=Butterfly; User ID=test; Password=test!123");
```

## Contributing

If you'd like to contribute, please fork the repository and use a feature
branch. Pull requests are warmly welcome.

## Licensing

The code is licensed under the [Mozilla Public License 2.0](http://mozilla.org/MPL/2.0/).  
