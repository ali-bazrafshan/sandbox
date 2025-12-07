using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(); // Makes exception handlers return Problem Details

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages(); // Makes all error responses return Problem Details

app.MapGet("/", () => "Hello.");

app.MapGet("/person", () => Person.All);
app.MapGet("/person/{id}", Handlers.GetPerson).AddEndpointFilterFactory(ValidationHelper.ValidateIdFactory);
app.MapPost("/person", Handlers.AddPerson);
app.MapPost("person/{id}", Handlers.InsertPerson)
    .AddEndpointFilter(ValidationHelper.ValidateId)
    .AddEndpointFilter(async (context, next) =>
    {
        app.Logger.LogInformation("Logging...");
        var result = await next(context);
        app.Logger.LogInformation($"Handler result: {result}");
        return result;
    });
app.MapPut("/person/{id}", Handlers.ReplacePerson);
app.MapDelete("/person/{id}", Handlers.DeletePerson);

app.MapGet("/custom", (HttpResponse response) =>
{
    response.StatusCode = 426;
    response.ContentType = MediaTypeNames.Text.Plain;
    response.Headers["MyHeader"] = "MyHeaderValue";
    return response.WriteAsync("Update application!");
});

app.MapGet("/throw", () =>
{
    throw new Exception("Random exception.");
});

app.MapGet("/file", () => Results.File("sample.png", fileDownloadName: "blue.png", contentType: "image/png"));

app.Run();

public record Person(string FirstName, string LastName)
{
    public static readonly Dictionary<int, Person> All = new();
}

class Handlers
{
    internal static IResult GetPerson(int id)
    {
        // Since we have added StatusCodePages middleware, all error responses will automatically be Problem Details
        // and we don't need to use (Typed)Results.Problem
        return Person.All.TryGetValue(id, out var result) ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    internal static IResult AddPerson(Person person)
    {
        if (person == null || string.IsNullOrEmpty(person.FirstName) || string.IsNullOrWhiteSpace(person.LastName))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                { "person", new[] {"Invalid data."} }
            });
        }
        Person.All.Add(Person.All.Count + 1, person);
        return TypedResults.Created($"/person/{Person.All.Count + 1}", person);
    }

    internal static IResult InsertPerson(int id, Person person)
    {
        return Person.All.TryAdd(id, person) ? TypedResults.Created($"/person/{id}", person) : TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            { "message", new[] {"A person with this ID already exists.", "Try with another ID."} }
        });
    }

    internal static IResult ReplacePerson(int id, Person person)
    {
        if (Person.All.Remove(id))
        {
            Person.All[id] = person;
            return TypedResults.NoContent();
        }

        return TypedResults.Problem(statusCode: 404);
    }

    internal static IResult DeletePerson(int id)
    {
        return Person.All.Remove(id) ? TypedResults.NoContent() : TypedResults.Problem(statusCode: 404);
    }
}

class ValidationHelper
{
    internal static async ValueTask<object?> ValidateId(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var id = context.GetArgument<int>(0);
        if (id <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "id", new[] {"Invalid ID. ID must be a positive integer."} }
            });
        }
        return await next(context);
    }

    internal static EndpointFilterDelegate ValidateIdFactory(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
    {
        var parameters = context.MethodInfo.GetParameters();
        int? idIndex = null;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].Name == "id")
            {
                idIndex = i;
                break;
            }
        }

        if (!idIndex.HasValue)
        {
            return next;
        }

        return async (invocationContext) =>
        {
            var id = invocationContext.GetArgument<int>(idIndex.Value);
            if (id <= 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "id", new[] {"Invalid ID. ID must be a positive integer."} }
                });
            }
            return await next(invocationContext);
        };
    }
}
