using minimal_api;
namespace minimal_api;
public class program
{


}
    IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
    CreateHostBuilder(args).Build().Run();