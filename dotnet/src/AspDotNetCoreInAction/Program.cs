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

var personApi = app.MapGroup("/person").AddEndpointFilterFactory(ValidationHelper.ValidateIdFactory);
personApi.MapGet("/", () => Person.All);
personApi.MapPost("/", PersonHandlers.AddPerson);
personApi.MapGet("/{id}", PersonHandlers.GetPerson);
personApi.MapPost("/{id}", PersonHandlers.InsertPerson)
    .AddEndpointFilter(async (context, next) =>
    {
        app.Logger.LogInformation("Logging...");
        var result = await next(context);
        app.Logger.LogInformation($"Handler result: {result}");
        return result;
    });
personApi.MapPut("{id}", PersonHandlers.ReplacePerson);
personApi.MapDelete("{id}", PersonHandlers.DeletePerson);

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

app.MapPost("/product/{category}/{title}/{id}/{price}", ProductHandlers.AddProduct);
app.MapGet("/product/{category}/{title=all}/{id?}", ProductHandlers.GetProduct);
app.MapGet("/product", () => Product.All);

app.Run();

public record Product(string Category, string Title, int Id, int Price)
{
    public static readonly List<Product> All = new();
}

class ProductHandlers
{
    internal static IResult AddProduct(string category, string title, int id, int price)
    {
        var product = new Product(category, title, id, price);
        Product.All.Add(product);
        return TypedResults.Created($"/product/{category}/{title}/{id}", product);
    }

    internal static IResult GetProduct(string category, string title, int? id)
    {
        var product = Product.All.Find(
            p => p.Category == category &&
            (title == "all" || p.Title == title) &&
            (!id.HasValue || p.Id == id.Value) 
        );
        return TypedResults.Ok(product);
    }
}

public record Person(string FirstName, string LastName)
{
    public static readonly Dictionary<int, Person> All = new();
}

class PersonHandlers
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
        int idIndex = Person.All.Keys.DefaultIfEmpty(0).Max() + 1;
        Person.All.Add(idIndex, person);
        return TypedResults.Created($"/person/{idIndex}", person);
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
