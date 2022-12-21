using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;

namespace CommandsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

                var platforms = grpcClient.ReturnAllPlatforms();

                SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
            }
        }

        private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("--> Seeding new platforms...");

            Console.WriteLine($"--> {platforms.Count()}");

            foreach (var plat in platforms)
            {
                Console.WriteLine($"External Platform ID: ${plat.ExternalId}");

                if (!repo.ExternalPlatformExists(plat.ExternalId))
                {
                    Console.WriteLine($"Adding platform: ${plat.Name}");
                    repo.CreatePlatform(plat);
                }
                repo.SaveChanges();
            }
        }
    }
}