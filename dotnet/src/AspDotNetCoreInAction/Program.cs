using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello.");
app.MapGet("/person", () => Person.All);
app.MapGet("/person/{id}", Handlers.GetPerson);
app.MapPost("/person", Handlers.AddPerson);
app.MapPut("/person/{id}", Handlers.ReplacePerson);
app.MapDelete("/person/{id}", Handlers.DeletePerson);

app.Run();

public record Person(string FirstName, string LastName)
{
    public static readonly Dictionary<int, Person> All = new();
}

class Handlers
{
    public static IResult GetPerson(int id)
    {
        return Person.All.TryGetValue(id, out var result) ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    public static IResult AddPerson(Person person)
    {
        Person.All.Add(Person.All.Count + 1, person);
        return TypedResults.Ok();
    }

    public static IResult ReplacePerson(int id, Person person)
    {
        if (Person.All.Remove(id))
        {
            Person.All[id] = person;
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }

    public static IResult DeletePerson(int id)
    {
        return Person.All.Remove(id) ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
