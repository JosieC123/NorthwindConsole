
using NLog;
using System.Linq;
using NorthwindConsole.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger

var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

do
{
    Console.WriteLine("=====Main Menu=====");
    Console.WriteLine("1) Display categories");
    Console.WriteLine("2) Add category");
    Console.WriteLine("3) Display Category and related products");
    Console.WriteLine("4) Display all Categories and their related products");
    Console.WriteLine("5) Options for Product Table");
    Console.WriteLine("===================");
    Console.WriteLine("Enter to quit");
    string? choice = Console.ReadLine();
    Console.Clear();
    logger.Info("Option {choice} selected", choice);

    if (choice == "1")
    {
        // display categories
        var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json");

        var config = configuration.Build();

        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryName);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{query.Count()} records returned");
        Console.ForegroundColor = ConsoleColor.Magenta;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryName} - {item.Description}");
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (choice == "2")
    {
        // Add category
        Category category = new();
        Console.WriteLine("Enter Category Name:");
        category.CategoryName = Console.ReadLine()!;
        Console.WriteLine("Enter the Category Description:");
        category.Description = Console.ReadLine();

        ValidationContext context = new ValidationContext(category, null, null);
        List<ValidationResult> results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(category, context, results, true);
        if (isValid)
        {
            var db = new DataContext();
            // check for unique name
            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
            {
                // generate validation error
                isValid = false;
                results.Add(new ValidationResult("Name exists", ["CategoryName"]));
            }
            else
            {
                logger.Info("Validation passed");
                // TODO: save category to db
            }
        }
        if (!isValid)
        {
            foreach (var result in results)
            {
                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
            }
        }
    }
    else if (choice == "3")
    {
        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryId);
        Console.WriteLine("Select the category whose products you want to display:");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
        }
        Console.ForegroundColor = ConsoleColor.White;
        int id = int.Parse(Console.ReadLine()!);
        Console.Clear();
        logger.Info($"CategoryId {id} selected");

        Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
        Console.WriteLine($"{category.CategoryName} - {category.Description}");
        foreach (Product p in category.Products)
        {
            Console.WriteLine($"\t{p.ProductName}");
        }
    }
    else if (choice == "4")
    {
        var db = new DataContext();
        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryName}");
            foreach (Product p in item.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }
    }
    else if (choice == "5")
    {
        while (true)
        {
            Console.WriteLine("\n=====Product Table Menu=====");
            Console.WriteLine("1) Add new record to product table");
            Console.WriteLine("2) Edit record in product table");
            Console.WriteLine("3) Display records in product table");
            Console.WriteLine("4) Display specific product");
            Console.WriteLine("===================");
            Console.WriteLine("Enter to go back to Main Menu");
            String? choiceProduct = Console.ReadLine();
            Console.Clear();
            logger.Info("Option {choiceProduct} selected", choiceProduct);

            //If presses Enter. break loop and go back to main menu
            if (string.IsNullOrEmpty(choiceProduct))
            {
                break;
            }

            if (choiceProduct == "1")
            {
                //Add new record to product

                var db = new DataContext();
                Product? product = InputProduct(db, logger);
                //save to db
                if (product != null)
                {
                    db.AddProduct(product);
                    logger.Info("Product added - {ProductName}", product.ProductName);
                }
            }
            else if (choiceProduct == "2")
            {
                // Edit specific record in product
                Console.WriteLine("Enter ID of Product you want to edit:");
                var db = new DataContext();
                var product = GetProduct(db, logger);
                if (product != null)
                {
                    Product? UpdatedProduct = InputProduct(db, logger);
                    if (UpdatedProduct != null)
                    {
                        UpdatedProduct.ProductId = product.ProductId;
                        db.EditProduct(UpdatedProduct);
                        logger.Info($"Product updated to \"{product.ProductName}\"");
                    }
                }
            }
            else if (choiceProduct == "3")
            {
                //Display all records in the Products table (ProductName only) opt all, discontinue, active. Discontinue must stand out
                Console.WriteLine("\n1) View all products");
                Console.WriteLine("2) View discontinued products");
                Console.WriteLine("3) View active products");
                string? option = Console.ReadLine();
                logger.Info("Option {option} selected", option);

                if (option == "1")
                {
                    var db = new DataContext();
                    var products = db.Products.OrderBy(p => p.ProductId).ToList();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{products.Count()} records returned");
                    foreach (var p in products)
                    {
                        if (p.Discontinued == true)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"{p.ProductName}");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"{p.ProductName}");
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (option == "2")
                {
                    var db = new DataContext();
                    var products = db.Products.Where(p => p.Discontinued == true).OrderBy(p => p.ProductId).ToList();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{products.Count()} records returned");
                    foreach (var p in products)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{p.ProductName}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (option == "3")
                {
                    var db = new DataContext();
                    var products = db.Products.Where(p => p.Discontinued != true).OrderBy(p => p.ProductId).ToList();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{products.Count()} records returned");
                    foreach (var p in products)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"{p.ProductName}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else if (choiceProduct == "4")
            {
                // Display a specific Product (all product fields should be displayed)
                Console.WriteLine("Enter Id of product to view: ");
                var db = new DataContext();
                var p = GetProduct(db, logger);
                if (p != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{p.ProductName}");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"\tID: {p.ProductId}\n\tSupplierID: {p.SupplierId}\n\tCategoryID: {p.CategoryId}\n\tQuantityPerUnit: {p.QuantityPerUnit}\n\tUnitPrice: {p.UnitPrice}\n\tUnitsOnOrder: {p.UnitsOnOrder}\n\tReorderLevel: {p.ReorderLevel}\n\tDiscontinued: {p.Discontinued}");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
    else if (String.IsNullOrEmpty(choice))
    {
        break;
    }
    Console.WriteLine();
} while (true);

logger.Info("Program ended");




//====================================================================================

static Product? GetProduct(DataContext db, NLog.Logger logger)
{
    if (int.TryParse(Console.ReadLine(), out int ProductId))
    {
        Product product = db.Products.FirstOrDefault(p => p.ProductId == ProductId)!;

        if (product == null)
        {
            // if the ID doesn't exist
            logger.Error($"ID {ProductId} does not exist.");
        }
        return product;
    }
    //if input is not a valid integer
    logger.Error("Invalid input. Please enter a valid Product ID.");
    return null;
}

static Product? InputProduct(DataContext db, NLog.Logger logger)
{
    Product product = new();
    Console.WriteLine("Enter Product Name:");
    product.ProductName = Console.ReadLine()!;

    ValidationContext context = new ValidationContext(product, null, null);
    List<ValidationResult> results = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(product, context, results, true);
    if (isValid)
    {
        // check for unique name
        if (db.Products.Any(p => p.ProductName == product.ProductName))
        {
            // generate validation error
            isValid = false;
            results.Add(new ValidationResult("Name exists", ["ProductName"]));
        }
    }
    if (!isValid)
    {
        foreach (var result in results)
        {
            logger.Error($"{result.MemberNames.First()}: {result.ErrorMessage}");
        }
        return null;
    }
    return product;
}


