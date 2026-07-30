namespace RocketPizza.Data;

public static class SeedData
{
    public static void Initialize(IHostEnvironment environment, ILogger logger)
    {
        logger.LogInformation("Banco RocketPizzaDB: use BancoDeDados/01_CriarBanco.sql para criar e popular os dados.");
    }
}
