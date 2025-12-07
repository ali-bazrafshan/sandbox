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
app.MapGet("/person/{id}", Handlers.GetPerson);
app.MapPost("/person", Handlers.AddPerson);
app.MapPost("person/{id}", Handlers.InsertPerson);
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
    public static IResult GetPerson(int id)
    {
        // Since we have added StatusCodePages middleware, all error responses will automatically be Problem Details
        // and we don't need to use (Typed)Results.Problem or (Typed)Results.ValidationProblem
        return Person.All.TryGetValue(id, out var result) ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    public static IResult AddPerson(Person person)
    {
        Person.All.Add(Person.All.Count + 1, person);
        return TypedResults.Created($"/person/{Person.All.Count + 1}", person);
    }

    public static IResult InsertPerson(int id, Person person)
    {
        return Person.All.TryAdd(id, person) ? TypedResults.Created($"/person/{id}", person) : TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            { "message", new[] {"A person with this ID already exists.", "Try with another ID."} }
        });
    }

    public static IResult ReplacePerson(int id, Person person)
    {
        if (Person.All.Remove(id))
        {
            Person.All[id] = person;
            return TypedResults.NoContent();
        }

        return TypedResults.Problem(statusCode: 404);
    }

    public static IResult DeletePerson(int id)
    {
        return Person.All.Remove(id) ? TypedResults.NoContent() : TypedResults.Problem(statusCode: 404);
    }
}
