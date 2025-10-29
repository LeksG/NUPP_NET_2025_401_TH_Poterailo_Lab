using CounterStrike.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class CounterStrikeContextFactory : IDesignTimeDbContextFactory<CounterStrikeContext>
{
    public CounterStrikeContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CounterStrikeContext>();
        // Файл SQLite у консольному проекті
        optionsBuilder.UseSqlite("Data Source=../CounterStrike.Console/counterstrike.db");
        return new CounterStrikeContext(optionsBuilder.Options);
    }
}
