using delteaching_dotnet;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Adicionando servi�os do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Gerenciamento de Contas Banc�rias",
        Version = "v1",
        Description = "API para realizar opera��es de CRUD em contas banc�rias."
    });
});

var app = builder.Build();

// Habilitar Swagger no pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Contas Banc�rias v1");
    });
}

var contasBancarias = new List<ContaBancaria>
{
    new() { Id = 1, Titular = "Jo�o Silva", Saldo = 5000 },
    new() { Id = 2, Titular = "Maria Oliveira", Saldo = 3000 }
};

// **GET** - Listar todas as contas banc�rias
app.MapGet("/api/contas", () => Results.Ok(contasBancarias))
    .WithTags("Contas")
    .WithName("GetAllContas")
    .WithOpenApi();

// **GET** - Buscar uma conta banc�ria por ID
app.MapGet("/api/contas/{id}", (int id) =>
{
    var conta = contasBancarias.FirstOrDefault(c => c.Id == id);
    return conta is not null ? Results.Ok(conta) : Results.NotFound(new { message = "Conta n�o encontrada" });
})
    .WithTags("Contas")
    .WithName("GetContaById")
    .WithOpenApi();

// **POST** - Criar uma nova conta banc�ria
app.MapPost("/api/contas", (ContaBancaria novaConta, HttpContext context) =>
{
    if (string.IsNullOrEmpty(novaConta.Titular) || novaConta.Saldo == 0)
    {
        return Results.BadRequest(new { message = "Titular e saldo s�o obrigat�rios" });
    }

    novaConta.Id = contasBancarias.LastOrDefault()?.Id + 1 ?? 1;
    contasBancarias.Add(novaConta);

    var host = app.Configuration["ASPNETCORE_URLS"]?.Split(";").FirstOrDefault();

    var links = new List<string>
    {
        $"<{host}/api/contas/{novaConta.Id}>; rel=\"self\"",
        $"<{host}/api/contas/{novaConta.Id}>; rel=\"update\"",
        $"<{host}/api/contas/{novaConta.Id}>; rel=\"delete\""
    };

    context.Response.Headers.Add("Link", string.Join(", ", links));

    return Results.Created($"/api/contas/{novaConta.Id}", novaConta);
})
    .WithTags("Contas")
    .WithName("CreateConta")
    .WithOpenApi();

// **PUT** - Atualizar os dados de uma conta banc�ria
app.MapPut("/api/contas/{id}", (int id, ContaBancaria contaAtualizada) =>
{
    var conta = contasBancarias.FirstOrDefault(c => c.Id == id);
    if (conta is null)
    {
        return Results.NotFound(new { message = "Conta n�o encontrada" });
    }

    conta.Titular = contaAtualizada.Titular ?? conta.Titular;
    conta.Saldo = contaAtualizada.Saldo != 0 ? contaAtualizada.Saldo : conta.Saldo;
    return Results.Ok(conta);
})
    .WithTags("Contas")
    .WithName("UpdateConta")
    .WithOpenApi();

// **DELETE** - Excluir uma conta banc�ria
app.MapDelete("/api/contas/{id}", (int id) =>
{
    var conta = contasBancarias.FirstOrDefault(c => c.Id == id);
    if (conta is null)
    {
        return Results.NotFound(new { message = "Conta n�o encontrada" });
    }

    contasBancarias.Remove(conta);
    return Results.NoContent();
})
    .WithTags("Contas")
    .WithName("DeleteConta")
    .WithOpenApi();

app.Run();
