//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Name    : Victor Tkach
/// Semester: Spring 2025
/// Class   : 10209
/// Email   : vtkach@students.solano.edu
/// Desc    : Semester_Project_Inventory_GUI
/// Pledge  : As a Falcon @ Solano College, I will conduct myself with honor and integrity at all times. I
///           will not lie, cheat, or steal, nor will I accept the actions of those who do. This program is
///           solely my work, or proper attribution has been given to code that I did not write. If I am
///           found to violate this policy, I realize I will receive an F for this course with no exception.
///           
/// References:
/// - Microsoft Docs: System.Data.SQLite usage in C#: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/
/// - OpenFoodFacts API Documentation: https://world.openfoodfacts.org/data
/// - Newtonsoft JSON (Json.NET) for parsing JSON: https://www.newtonsoft.com/json
/// - Exporting SQLite database to CSV: https://stuartsplace.com/information-technology/programming/c-sharp/c-sharp-and-sqlite-exporting-data-csv
/// - CSV Escaping in C#: https://ssojet.com/escaping/csv-escaping-in-c/
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// UI class for user interactions
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace simpleInventoryGUI
{

    public partial class Inventory_GUI : Form // Main form class
    {
        private static string dbPath = "inventory.db"; // Path to Database
        private static string connectionString = $"Data Source={dbPath};Version=3"; // Connection to SQLite Database
        private static readonly HttpClient http = new HttpClient(); // instance for making HTTP request
        private readonly Product_Repository repo = new Product_Repository("inventory.db"); // Instance for repository to interact with db

        public Inventory_GUI() // GUI constructor
        {
            InitializeComponent(); // Initialize components
            repo.CreateDatabase(); // verifies db and table are made
            LoadProducts(); // load existing products to datagrid
        }

        private async void btnAdd_Click(object sender, EventArgs e) // event handler for add button
        {
            string barcode = txtBarcode.Text.Trim(); //  trim whitespace from barcode input
            string name = await LookupProductName(barcode) ?? txtName.Text.Trim(); // attempts to look up product name on OpenFoodFacts API using barcode
            string supplier = txtSupplier.Text.Trim(); // trim whitespace from supplier input

            if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(barcode)) // parse quantity input to int
            {
                MessageBox.Show("Fill in all required fields."); // error handling
                return;
            }

            var product = new Product // creates new product using inputs
            {
                Barcode = barcode,
                Name = name,
                Supplier = supplier,
                Quantity = quantity
            };
            
            bool added = repo.AddProduct(product); // attempts to add product to inventory.db
            MessageBox.Show(added ? "Product added." : "Product already exists, please update quantity."); // inform user of result

            LoadProducts(); // reload products to reflect changes
        }
        private async Task<string> LookupProductName(string barcode) // Method to lookup product name using API
        {
            string apiURL = $"https://world.openfoodfacts.org/api/v2/product/{barcode}.json"; // construct name using provided barcode

            try
            {
                Console.WriteLine($"Looking up product for barcode: {barcode}");
                var response = await http.GetStringAsync(apiURL); // GET request to API to get response as string
                JObject json = JObject.Parse(response); // parse JSON response

                if (json["product"] != null) // check if product field exits in JSON
                {
                    // extract product name from JSON
                    string productName = json["product"]?["product_name"]?.ToString();
                    Console.WriteLine($"Product Found: {productName}");
                    return productName;
                }
                else
                {
                    Console.WriteLine("Product not found in OpenFoodFacts database."); // error handling
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API lookup failed: {ex.Message}"); // error handling
                return null;
            }
        }
        private void btnRemove_Click(object sender, EventArgs e) // event handler for remove button
        {
            string barcode = txtBarcode.Text.Trim(); // trim whitespace from product input
            bool removed = repo.RemoveProduct(barcode); // attempt to remove product from db
            MessageBox.Show(removed ? "Product removed." : "Product not found."); // inform result

            LoadProducts(); // reload products to reflect changes
        }

        private void btnUpdate_Click(object sender, EventArgs e) // event handler for update button
        {
            string barcode = txtBarcode.Text.Trim(); // trim whitespace from barcode
            if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity)) // parse quantity input to int
            {
                MessageBox.Show("Please enter a valid quantity."); // error handling
                return;
            }

            bool updated = repo.UpdateQuantity(barcode, quantity); // attempt to update quantity in inventory.db
            MessageBox.Show(updated ? "Product updated." : "Product not found."); // inform results

            LoadProducts(); // reload to reflect changes
        }
        private void btnExport_Click(object sender, EventArgs e) // event handler for export button
        {
            // Reference: Stuart's Place - Exporting SQLite to CSV
            // Source: https://stuartsplace.com/information-technology/programming/c-sharp/c-sharp-and-sqlite-exporting-data-csv

            using (var conn = new SQLiteConnection(connectionString)) // connect to SQLite db
            {
                conn.Open(); // open connection

                string sql = "SELECT * FROM Products"; // query to select all Product records from db
                using (var cmd = new SQLiteCommand(sql, conn)) // command to execute SQL query
                using (var reader = cmd.ExecuteReader()) // execute query and obtain reader for results

                using (var sw = new StreamWriter("inventory_export.csv")) // streamwriter to write to CSV file
                {
                    sw.WriteLine("Id,Name,Barcode,Quantity,Supplier"); // writer header lines

                    while (reader.Read()) // go through each record in results
                    {
                        // retrieve value from field and convert to string
                        string id = reader["Id"].ToString();
                        string name = EscapeForCsv(reader["Name"].ToString());
                        string barcode = EscapeForCsv(reader["Barcode"].ToString());
                        string quantity = EscapeForCsv(reader["Quantity"].ToString());
                        string supplier = EscapeForCsv(reader["Supplier"].ToString());

                        sw.WriteLine($"{id},{name},{barcode},{quantity}, {supplier}"); // write record to csv file
                    }
                }
            }
            MessageBox.Show("Data exported to inventory_export.csv"); // inform that export was successful
        }
        // Reference: SSOJet - CSV Escaping in C#
        // Source: https://ssojet.com/escaping/csv-escaping-in-c/
        private string EscapeForCsv(string value) // helper method to escape special charaters to format to CSV
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n")) // checks for commas, quotes, or newlines
            {
                value = value.Replace("\"", "\"\""); // replaces internal " with "" for CSV escaping rules
                return $"\"{value}\""; // wrapped in "" to tell CSV reader this is one value
            }
            return value; // return normally if there are no special characters
        }
        private void LoadProducts() // method to load all products in db to datagrid
        {
            dataGridViewProducts.DataSource = repo.LoadAll(); // set data source
            dataGridViewProducts.RowHeadersVisible = false; // hide row header
        }

        private void Inventory_GUI_Load(object sender, EventArgs e)
        {

        }
    }
}
