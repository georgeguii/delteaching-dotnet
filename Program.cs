using delteaching_dotnet;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var contasBancarias = new List<ContaBancaria>
{
    new() { Id = 1, Titular = "João Silva", Saldo = 5000 },
    new() { Id = 2, Titular = "Maria Oliveira", Saldo = 3000 }
};

// **GET** - Listar todas as contas bancárias
app.MapGet("/api/contas", () => Results.Ok(contasBancarias));

// **GET** - Buscar uma conta bancária por ID
app.MapGet("/api/contas/{id}", (int id) =>
{
    var conta = contasBancarias.FirstOrDefault(c => c.Id == id);
    return conta is not null ? Results.Ok(conta) : Results.NotFound(new { message = "Conta não encontrada" });
});

// **POST** - Criar uma nova conta bancária
app.MapPost("/api/contas", (ContaBancaria novaConta, HttpContext context) =>
{
    if (string.IsNullOrEmpty(novaConta.Titular) || novaConta.Saldo == 0)
    {
        return Results.BadRequest(new { message = "Titular e saldo são obrigatórios" });
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
});

// **PUT** - Atualizar os dados de uma conta bancária
app.MapPut("/api/contas/{id}", (int id, ContaBancaria contaAtualizada) =>
{
    var conta = contasBancarias.FirstOrDefault(c => c.Id == id);
    if (conta is null)
    {
        return Results.NotFound(new { message = "Conta não encontrada" });
    }

    conta.Titular = contaAtualizada.Titular ?? conta.Titular;
    conta.Saldo = contaAtualizada.Saldo != 0 ? contaAtualizada.Saldo : conta.Saldo;
    return Results.Ok(conta);
});

// **DELETE** - Excluir uma conta bancária
app.MapDelete("/api/contas/{id}", (int id) =>
{
    var conta = contasBancarias.FirstOrDefault(c => c.Id == id);
    if (conta is null)
    {
        return Results.NotFound(new { message = "Conta não encontrada" });
    }

    contasBancarias.Remove(conta);
    return Results.NoContent();
});

app.Run();
