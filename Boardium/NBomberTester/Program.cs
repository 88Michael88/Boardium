using NBomber.Contracts.Stats;
using NBomber.CSharp;

namespace NBomberTester {
    public class EndPointTester {
        public static void BasicTest() {
            Random random = new Random();
            string baseURL = @"https://localhost:7093";
            using var httpClient = new HttpClient();

            var scenario = Scenario.Create("SequentialRequestTest", async context => {
                var step1 = await Step.Run("Home Page B", context, async () => {
                    var response = await httpClient.GetAsync($"{baseURL}/");
                    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                });

                var step2 = await Step.Run("Games Page", context, async () => {
                    var response = await httpClient.GetAsync($"{baseURL}/Games/");
                    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                });

                var step3 = await Step.Run("Games Details Page Filtered", context, async () => {
                    int randomNumber = random.Next(0, 3);
                    string[] categories = { "Karcianka", "Strategia", "Rodzinna" };
                    var response = await httpClient.GetAsync($"{baseURL}/Games?category={categories[randomNumber]}");
                    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                });

                var step4 = await Step.Run("Games Details Page", context, async () => {
                    int randomNumber = random.Next(1, 4);
                    var response = await httpClient.GetAsync($"{baseURL}/Games/BoardGame?gameIndex={randomNumber}");
                    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                });

                return Response.Ok();
            })
                .WithWarmUpDuration(TimeSpan.FromSeconds(10))
                .WithLoadSimulations(Simulation.KeepConstant(2500, TimeSpan.FromSeconds(500)));

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFormats(ReportFormat.Html)
                .Run();
        }
    }
    internal class Program {
        static void Main(string[] args) {
            EndPointTester.BasicTest();
        }
    }
}
