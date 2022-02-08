using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.Logger.LogInformation("Application starting up");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/ping", () => "pong");

// Get all items
app.MapGet("/todoitems", async (TodoDb db) =>
 {
     return await db.Todos.ToListAsync();
 });

//Get completed items
app.MapGet("todoitems/complete", async (TodoDb db) =>
{
    return await db.Todos.Where(t => t.IsComplete).ToListAsync();
});

// Get a specific item
app.MapGet("/todoitems/{id}", async (TodoDb db, int id) =>
{
    return await db.Todos.FirstOrDefaultAsync(t => t.Id == id);
});

// Create a new item
app.MapPost("/todoitems", async (TodoDb db, Todo item) =>
{
    await db.Todos.AddAsync(item);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{item.Id}", item);
});

// Edit an item
app.MapPut("/todoitems/{id}", async (TodoDb db, int id, Todo item) =>
{
    var todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == id);
    if (todo == null)
    {
        return Results.NotFound();
    }

    todo.Name = item.Name;
    todo.IsComplete = item.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Delete an item
app.MapDelete("/todoitems/{id}", async (TodoDb db, int id) =>
{
    var todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == id);
    if (todo == null)
    {
        return Results.NotFound();
    }

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// Add respond ports
app.Urls.Add("http://localhost:5000");
app.Urls.Add("https://localhost:5001");
app.Run();

// Todo model class
class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

// Database context class
class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}