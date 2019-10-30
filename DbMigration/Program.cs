using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Cassandra;
using Cassandra.Data.Linq;

namespace DbMigration
{
    //measurement table context
   public class MeasurementContext : DbContext
    {
        public MeasurementContext() : base("SavoniaMeasurementsV2Entities") { }
        public DbSet<Measurement> Measurement { get; set; }
    }
    //Data table context
    public class DataContext : DbContext
    {
        public DataContext() : base("SavoniaMeasurementsV2Entities") { }
        public DbSet<Data> Data { get; set; }
    }
    public class Cassandracontext : DbContext
    {
        public Cassandracontext() : base("samiv4.measuremendata") { }
        
    }

    class Program
    {
        static void Main(string[] args)
        {

            //Cassandra connection
            var cluster = Cluster.Builder()
                                .AddContactPoints("Samidb.ky.local")
                                .WithPort(9042)

                                .Build();

            // Connect to the nodes using a keyspace
            var session = cluster.Connect("samiv4");

            using (var context = new DataContext())
            {
                
                try
                {           
                   //fetch data from Data table
                    var data = (from da in context.Data where da.MeasurementID >= 0 && da.MeasurementID <= 1 select da);

                    foreach (var row in data)
                    {
                        Console.WriteLine("ID: " + row.MeasurementID + " Tag: " + row.Tag + " Value: " + row.Value + " LongValue: " + row.LongValue + " TextValue: " + row.TextValue + 
                            " BinaryValue: " + row.BinaryValue + " XmlValue: " + row.XmlValue);

                        
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }               
            }
            
            Console.ReadKey();

            using (var context2 = new MeasurementContext())
            {
                string dateTime = DateTime.Now.ToString("yyyy-MM");
                
                try
                {
                    //fetch data from Measurement table
                    var measurement = (from me in context2.Measurement where me.ID >= 1 && me.ID < 2 select me);

                    foreach (var row in measurement)
                    {
                        Console.WriteLine(" ID: " + row.ID + " ProviderID: " + row.ProviderID + " Object: " + row.Object + " Tag: " + row.Tag + " Timestamp: " + row.Timestamp + " Note: " + row.Note +
                            " Location: " + row.Location + " RowCreateTimestamp: " + row.RowCreatedTimestamp + " KeyId: " + row.KeyId);

                        var cqlquery = session.Execute("INSERT INTO samiv4.measurementdata(providerid, sensortag, timebucket, timestamp) values(" + row.ProviderID + "," + row.Tag + "," + dateTime +   "," + row.Timestamp + " )allow filtering; ");
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.ReadKey();

         

            // Get name of a Cluster
            Console.WriteLine("The cluster's name is: " + cluster.Metadata.ClusterName);

            // Execute a query on a connection synchronously
            var rs = session.Execute("SELECT * FROM samiv4.measurementdata");

            //insert data
            //var cqlquery = session.Execute("INSERT INTO samiv4.measurementdata(providerid, sensortag, timebucket, timestamp) values(22, 'testi', '2019-10', '2019-10-28 10:29:32 +0300' ); ");

            // Iterate through the RowSet (read)
            foreach (var row in rs)
             {
                 var value = row.GetValue<int>("providerid");
                 Console.WriteLine(value);
                 // Do something with the value
             }
             Console.ReadKey();
             
            
            
    }
    }
}
