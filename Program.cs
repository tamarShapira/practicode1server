using Microsoft.EntityFrameworkCore;
using ToDoApi;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddCors(opt=>opt.AddPolicy("CorsPolicy",x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());
app.MapPost("/addTodo", async (Item todo,ToDoDbContext db) =>
{
    db.Items.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/addTodo/{id}", async (int id,bool isComplete, ToDoDbContext db) =>
{
        var todo=await db.Items.FindAsync(id);
        if(todo is null) return Results.NotFound();
        todo.IsComplete = isComplete;
        await db.SaveChangesAsync();
        return Results.NoContent();
});

app.MapDelete("/deleteTodo/{id}", async (int id,ToDoDbContext db) =>
{
    if (await db.Items.FindAsync(id) is Item todo)
    {
        db.Items.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});
app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
app.Run();
