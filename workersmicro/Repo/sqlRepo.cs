using Npgsql;
using System.Data;
using workersmicro.Model;

namespace workersmicro.Repo
{
    internal static class sqlRepo
    {
        
        private static string connectionString;
        public static async Task<bool> isWorking(long? id)
        {
            try
            {
                using var con = new NpgsqlConnection(connectionString);


                bool workerstatus = false;
                con.Open();
                var listSalary = new List<WorkerSalary>();
                var dataForFastReport = DateTime.Today.AddDays(-4).ToString("dd.MM.yy");
                var sql = $"SELECT workerstatus FROM workerinfo WHERE uniq={id}";
                using var cmd = new NpgsqlCommand(sql, con);

                await using NpgsqlDataReader? rdr = cmd.ExecuteReader();
                    
                    while (await rdr.ReadAsync())

                    {
                        workerstatus = rdr.GetBoolean(0);

                    }

                
                con.Close();
                con.Dispose();
                return workerstatus;
            

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка\n" + ex);
                return false;
            }


        }
        public static async Task<List<long?>> CreateWorkersListAsync()
        {
            try
            {
                using var con = new NpgsqlConnection(connectionString);
                con.Open();
                var listID = new List<long?>();
                var dataForFastReport = DateTime.Today.AddDays(-4).ToString("dd.MM.yy");
                var sql = $"SELECT uniq FROM workerinfo;";
                using var cmd = new NpgsqlCommand(sql, con);
                using NpgsqlDataReader? rdr = cmd.ExecuteReader();


                while (await rdr.ReadAsync())

                {
                    listID.Add(rdr.IsDBNull("uniq") ? 0 : rdr.GetInt64("uniq"));
                }
                
                con.Close();
                return listID;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка\n" + ex);
                return null;
            }


        }
        public static async Task<List<WorkerSalary>> FastSelectAsync()
        {
            try
            {
               using var con = new NpgsqlConnection(connectionString);
               con.Open();
               var listSalary = new List<WorkerSalary>();
               var dataForFastReport = DateTime.Today.AddDays(-4).ToString("dd.MM.yy");
               var sql = $"SELECT uniq,name, SUM(salary) FROM rezofwork WHERE tbegin>'{dataForFastReport}' GROUP BY uniq,name;";
               using var cmd = new NpgsqlCommand(sql, con);
               using NpgsqlDataReader? rdr = cmd.ExecuteReader();


                while (await rdr.ReadAsync())

                {
                    listSalary.Add(new WorkerSalary { Id = rdr.IsDBNull(0) ? "Нет идентификатора" : rdr.GetValue(0).ToString(), name = rdr.IsDBNull(1) ? "Нет имени" : rdr.GetValue(1).ToString(), salary = rdr.IsDBNull(2) ? "Нет зарплаты" : rdr.GetValue(2).ToString() });
                }
                foreach (var w in listSalary)
                    await Console.Out.WriteLineAsync(w.Id + "\t" + w.name + "\t" + w.salary);

                con.Close();
                return listSalary;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка\n" + ex);
                return null;
            }


        }
        public static async Task<List<WorkerSalary>> SelectBetweenTwoDatesAsync(string date1, string date2)
        {
            try
            {
                using var con = new NpgsqlConnection(connectionString);
                con.Open();
                var listSalary = new List<WorkerSalary>();
                bool isTwoDate = false;
                string[] twoDatesString = null;
                string sql = null;


                if (date2 != "0")
                {

                    sql = $"SET datestyle = dmy;SELECT uniq,name, SUM(salary) FROM rezofwork WHERE tbegin>='{date1}' AND tbegin<'{date2}' GROUP BY uniq,name;";
                }
                else
                    sql = $"SET datestyle = dmy;SELECT uniq,name, SUM(salary) FROM rezofwork WHERE tbegin>='{date1}' GROUP BY uniq,name;";


                using var cmd = new NpgsqlCommand(sql, con);
                using NpgsqlDataReader? rdr = cmd.ExecuteReader();

                while (await rdr.ReadAsync())
                {
                    listSalary.Add(new WorkerSalary { Id = rdr.IsDBNull(0) ? "Нет идентификатора" : rdr.GetValue(0).ToString(), name = rdr.IsDBNull(1) ? "Нет имени" : rdr.GetValue(1).ToString(), salary = rdr.IsDBNull(2) ? "Нет зарплаты" : rdr.GetValue(2).ToString() });
                }

                con.Close();
                return listSalary;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка\n" + ex);
                return null;
            }


        }
        public static async Task<WorkRezult> SelectWorkerLastRowByID(long id)
        {
            
            try
            {

                using var con = new NpgsqlConnection(connectionString);

                con.Open();

                WorkRezult worker = null;

                var sql = $"SELECT * FROM rezofwork WHERE id=(SELECT MAX(id) FROM rezofwork WHERE uniq={id});";

                using var cmd = new NpgsqlCommand(sql, con);
                using NpgsqlDataReader? rdr = cmd.ExecuteReader();

                while (await rdr.ReadAsync())

                {
                    worker = new WorkRezult
                    {
                        ID = id,
                        name = rdr.IsDBNull("name") ? "Нет имени" : rdr.GetString("name"),
                        project = rdr.IsDBNull("project") ? "Нет проекта" : rdr.GetString("project"),
                        tBegin = rdr.IsDBNull("tbegin") ? DateTime.MinValue : rdr.GetDateTime("tbegin"),
                        pricePerHour = rdr.IsDBNull("price") ? 222 : rdr.GetInt16("price")
                    };
                    await Console.Out.WriteLineAsync("in Select by id"+worker.ID + "\t" + worker.name + "\t" + worker.project + worker.tBegin);
                }



                con.Close();
                return worker;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка\n" + ex);
                return null;
            }


        }

        public static async Task InsertInDBAsync(WorkRezult workRezult)
        {
            try
            {
                using var con = new NpgsqlConnection(connectionString);
                con.Open();
 
                var sqlInsert = $"INSERT INTO rezofwork (id,uniq,name,project,tbegin,price) VALUES (DEFAULT,@ID,@name,@project,@tBegin,@price);UPDATE workerinfo SET workerstatus=true WHERE uniq={workRezult.ID};";

                await using (var cmdInsert = new NpgsqlCommand(sqlInsert, con)) {

                    cmdInsert.Parameters.AddWithValue("ID", workRezult.ID);
                    cmdInsert.Parameters.AddWithValue("name", workRezult.name);
                    cmdInsert.Parameters.AddWithValue("project", workRezult.project);
                    cmdInsert.Parameters.AddWithValue("tBegin", workRezult.tBegin);
                    cmdInsert.Parameters.AddWithValue("price", workRezult.pricePerHour);



                    cmdInsert.ExecuteNonQuery();
                }
                
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка3\n" + ex);
            }


        }
        public static async Task UpdateInDBAsync(WorkRezult workRezult)
        {
            try
            {
                using var con = new NpgsqlConnection(connectionString);
                con.Open();
                var sqlUpdate = $"UPDATE rezofwork SET tend=@tEnd,timeofwork=@timeofwork,salary=@salary where id=(SELECT MAX(id) FROM rezofwork WHERE uniq=@ID);UPDATE workerinfo SET workerstatus=false WHERE uniq={workRezult.ID};";

                await using (var cmdInsert = new NpgsqlCommand(sqlUpdate, con))
                {

                    cmdInsert.Parameters.AddWithValue("ID", workRezult.ID);
                    cmdInsert.Parameters.AddWithValue("tEnd", workRezult.tEnd);
                    cmdInsert.Parameters.AddWithValue("timeofwork", workRezult.timeOfWork);
                    cmdInsert.Parameters.AddWithValue("salary", workRezult.salary);
                    await Console.Out.WriteLineAsync(sqlUpdate);


                    cmdInsert.ExecuteNonQuery();
                }

                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка3\n" + ex);
            }


        }
        public static async Task UpdateWorkerStatusAsync(long? ID,bool status)
        {
            try
            {
                using var con = new NpgsqlConnection(connectionString);
                con.Open();


                var sqlUpdate = $"UPDATE workerinfo SET workerstatus={status} where uniq={ID};";
                await Console.Out.WriteLineAsync(sqlUpdate);

                await using (var cmdInsert = new NpgsqlCommand(sqlUpdate, con))
                {
                    cmdInsert.ExecuteNonQuery();
                }

                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка3\n" + ex);
            }


        }
        public static async Task RegistrInDBAsync(long id,string name="Нет имени")
        {
            try
            {
                using var con = new NpgsqlConnection(connectionString);
                con.Open();

                var sqlInsert = $"INSERT INTO workerinfo (uniq,name) VALUES ({id},'{name}') ON CONFLICT (uniq) DO NOTHING;";

                await using (var cmdInsert = new NpgsqlCommand(sqlInsert, con))
                {


                    cmdInsert.ExecuteNonQuery();
                }

                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка3\n" + ex);
            }


        }
        
        public static async Task Createtable()
        {
            try
            {
                using var con = new NpgsqlConnection(connectionString);
                con.Open();


                var sqlUpdate = $"create table IF NOT EXISTS workerinfo (id SERIAL PRIMARY KEY,uniq numeric UNIQUE,name name,price smallint default 222,workerstatus boolean default false);";

                await using (var cmdInsert = new NpgsqlCommand(sqlUpdate, con))
                {
                    cmdInsert.ExecuteNonQuery();
                }

                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошипка3\n" + ex);
            }


        }
        public static async Task Init(string _connectionString)
        {
            connectionString = _connectionString;
        }
                       
    }
}
