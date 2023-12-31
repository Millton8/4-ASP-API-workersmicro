using workersmicro.MiddleWare;
using workersmicro.Model;
using workersmicro.Repo;


namespace workersmicro
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors();
            builder.Services.AddSingleton(builder.Configuration);
            var app = builder.Build();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            var pirecePerHourList = new Dictionary<string, int>();
            var listOfWorkersID = new List<long?>();
            
            
            sqlRepo.Init(app.Configuration["App:connectToDB"]);
            sheetsRepo.Init(app.Configuration["App:sheetsID"]);
            StartEvents();
            app.UseCors(builder => builder.AllowAnyHeader()
                                          .AllowAnyMethod()
                                          .AllowAnyOrigin());
            
            app.MapGet("/check/{id}/{name?}", CheckWorkerStatus);
            app.MapGet("/fast",FastSelect);
            app.MapGet("/price", PricePerHour);
            app.MapGet("/projects", SendProjects);
            app.MapGet("/twodates/{date1}/{date2?}", TwoDates);

            app.MapPost("/newwork", InsertInDBNewWork);
            app.MapPost("/update", UpdateWorkInDB);


            app.Run();


            async Task StartEvents()
            {
                await Console.Out.WriteLineAsync("Start envents");
                listOfWorkersID = await sqlRepo.CreateWorkersListAsync();
                System.Timers.Timer timer = new(interval: 1000*60*60*12);
                timer.Elapsed += async (sender, e) => await UpdateList();
                timer.Start();
                await Console.Out.WriteLineAsync("Events starting");

            }
            async Task UpdateList()
            {
                listOfWorkersID = await sqlRepo.CreateWorkersListAsync();

            }

            async Task SendProjects(HttpContext httpContext)
            {
                var projects = await sheetsRepo.ReadProjectsAsync();
                await httpContext.Response.WriteAsJsonAsync(projects);
            }
            async Task CheckWorkerStatus(HttpContext httpContext,long id,string? name)
            {
                if (!listOfWorkersID.Contains(id))
                {
                    await sheetsRepo.WriteUserAsync(id, name);
                    await sqlRepo.RegistrInDBAsync(id,name);
                    listOfWorkersID = await sqlRepo.CreateWorkersListAsync();
                }
                if (string.IsNullOrWhiteSpace(id.ToString()))
                {
                    httpContext.Response.StatusCode = 500;
                    return;
                }

                var status= await sqlRepo.isWorking(id);
                if (status == false)
                    httpContext.Response.StatusCode = 200;
                else
                    httpContext.Response.StatusCode = 201;
            }
            async Task FastSelect(HttpContext httpContext)
            {                
                var listOfWorkersSalary =await sqlRepo.FastSelectAsync();
                await httpContext.Response.WriteAsJsonAsync(listOfWorkersSalary);
            }
            async Task TwoDates(HttpContext httpContext, string date1, string date2)
            {
                var listOfWorkersSalary = await sqlRepo.SelectBetweenTwoDatesAsync(date1, date2);
                await httpContext.Response.WriteAsJsonAsync(listOfWorkersSalary);
            }
            async Task PricePerHour(HttpContext httpContext)
            {
                var listOfWorkersSalary = await sheetsRepo.ReadPricePerHourAsync();
                await httpContext.Response.WriteAsJsonAsync(listOfWorkersSalary);
            }
            async Task InsertInDBNewWork(HttpContext httpContext)
            {
                var worker = await httpContext.Request.ReadFromJsonAsync<WorkRezult>();
                worker.tBegin = DateTime.Now;
                pirecePerHourList = await sheetsRepo.ReadPricePerHourAsync();


                if (pirecePerHourList.ContainsKey(worker.ID.ToString()))
                    worker.pricePerHour = pirecePerHourList[worker.ID.ToString()];
                else
                    worker.pricePerHour = 222;
                await sqlRepo.InsertInDBAsync(worker);
            }
            async Task UpdateWorkInDB(HttpContext httpContext)
            {
                    var workerID = await httpContext.Request.ReadFromJsonAsync<long>();
                    var workerRezult = await sqlRepo.SelectWorkerLastRowByID(workerID);
                    workerRezult.AddInfo();

                    await httpContext.Response.WriteAsJsonAsync(workerRezult);
                    await sqlRepo.UpdateInDBAsync(workerRezult);
                    await sheetsRepo.WriteAsync(workerRezult);
              
            }


        }
    }
}