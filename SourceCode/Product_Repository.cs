// database help to handle all SQLite communication
using System.Data; // provides class for working with tables
using System.Data.SQLite; // all db interation
using System.IO; // used for working with files

namespace simpleInventoryGUI
{
    internal class Product_Repository // class to handle all database operations for Product
    {
        private readonly string connectString; // connection string to connect to db

        public Product_Repository(string dbPath) // constructor to set up connection using the path to db
        {
            connectString = $"Data Source={dbPath};Version=3";
        }

        public void CreateDatabase() // creates db and table if one does not exist
        {
            // checks if db file exist or creates it
            if (!File.Exists("inventory.db"))
                SQLiteConnection.CreateFile("inventory.db");

            using (var conn = new SQLiteConnection(connectString)) // open connection to SQLite db
            {
                conn.Open();

                // statement to create the PRODUCTS table if one does not exist
                string sql = @"CREATE TABLE IF NOT EXISTS PRODUCTS (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Barcode TEXT UNIQUE NOT NULL, Quantity INTEGER NOT NULL, Supplier TEXT);";

                using (var cmd = new SQLiteCommand(sql, conn)) // execute SQL commant to create table
                {
                    cmd.ExecuteNonQuery(); // run command but doesn't expect results
                }
            }
        }
        
        // add product to db. True if successful, false if product exist
        public bool AddProduct(Product product)
        {
            using (var conn = new SQLiteConnection(connectString))
            {
                conn.Open();

                // insert neew product or ignore if barcode exist
                string sql = "INSERT OR IGNORE INTO Products (Name, Barcode, Quantity, Supplier) VALUES (@Name, @Barcode, @Quantity, @Supplier)";
                
                using (var cmd = new SQLiteCommand(sql,conn))
                {
                    // parameters to safely insert data to SQL query
                    cmd.Parameters.AddWithValue("@Name", product.Name);
                    cmd.Parameters.AddWithValue("@Barcode", product.Barcode);
                    cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                    cmd.Parameters.AddWithValue("@Supplier", product.Supplier);

                    return cmd.ExecuteNonQuery() > 0; // execute query. Return true if rows were effected
                }
            }
        }

        // removes product from db using barcode. returns true is row was deleted
        public bool RemoveProduct(string barcode)
        {
            using (var conn = new SQLiteConnection(connectString))
            {
                conn.Open();

                // statement to delete product by barcode
                string sql = "DELETE FROM Products WHERE Barcode = @Barcode";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Barcode", barcode);
                    return cmd.ExecuteNonQuery() > 0; // execute delete command. return true if 1 or more rows were deleted
                }
            }
        }

        // update quantity with given barcode
        public bool UpdateQuantity(string barcode, int quantity)
        {
            using (var conn = new SQLiteConnection(connectString))
            {
                conn.Open();

                // statement to update quantity of given barcode
                string sql = "UPDATE Products SET Quantity = @Quantity WHERE Barcode = @Barcode";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@Barcode", barcode);

                    return cmd.ExecuteNonQuery() > 0; // return true if any rows were updated
                }
            }
        }

        // load all products from db to table
        public DataTable LoadAll()
        {
            using (var conn = new SQLiteConnection(connectString))
            {
                conn.Open();

                // SQL query to get all products
                string sql = "SELECT * FROM Products";

                using (var adpt = new SQLiteDataAdapter(sql, conn)) // SQL adapter to fill table with query results
                {
                    var tbl = new DataTable(); // create in memory table
                    adpt.Fill(tbl); // fill table with query data

                    return tbl; // return populated table
                }
            }
        } 
    }
}
