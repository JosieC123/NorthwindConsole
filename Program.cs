
using NLog;
using System.Linq;
using NorthwindConsole.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();
logger.Info("Program started");

do
{
    Console.WriteLine("=====Main Menu=====");
    Console.WriteLine("1) Display categories");
    Console.WriteLine("2) Add category");
    Console.WriteLine("3) Display Category and related products");
    Console.WriteLine("4) Display all Categories and their related products");
    Console.WriteLine("5) Edit record in categories table");
    Console.WriteLine("6) Options for Product Table");
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
                db.AddCategory(category);
                logger.Info("Category added - {CategoryName}", category.CategoryName);
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
        foreach (Product p in category.Products.Where(p => p.Discontinued != true))
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
        //Edit a specified record from the Categories table 
        var db = new DataContext();
        var query = db.Categories.OrderBy(c => c.CategoryId);
        Console.ForegroundColor = ConsoleColor.Red;
        foreach (var category in query)
        {
            Console.WriteLine($"{category.CategoryId}) {category.CategoryName}");
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Enter ID of Category you want to edit:");
        var c = GetCategory(db, logger);
        if (c != null)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Current Record:\n{c.CategoryName} - {c.Description}");
            Console.ForegroundColor = ConsoleColor.White;

            Category? UpdatedCategory = InputCategory(db, logger);
            if (UpdatedCategory != null)
            {
                UpdatedCategory.CategoryId = c.CategoryId;
                db.EditCategory(UpdatedCategory);
                logger.Info($"Record successfully updated, \"{c.CategoryName}\"");
            }
        }
    }
    else if (choice == "6")
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
            if (string.IsNullOrEmpty(choiceProduct)) { break; }

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
                var p = GetProduct(db, logger);
                if (p != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Current Record:\n{p.ProductName}");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"\tID: {p.ProductId}\n\tSupplierID: {p.SupplierId}\n\tCategoryID: {p.CategoryId}\n\tQuantityPerUnit: {p.QuantityPerUnit}\n\tUnitPrice: {p.UnitPrice}\n\tUnitsInStock: {p.UnitsInStock}\n\tUnitsOnOrder: {p.UnitsOnOrder}\n\tReorderLevel: {p.ReorderLevel}\n\tDiscontinued: {p.Discontinued}");
                    Console.ForegroundColor = ConsoleColor.White;

                    Product? UpdatedProduct = InputProduct(db, logger);
                    if (UpdatedProduct != null)
                    {
                        UpdatedProduct.ProductId = p.ProductId;
                        db.EditProduct(UpdatedProduct);
                        logger.Info($"Record successfully updated, \"{p.ProductName}\"");
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
//Getting the product based on id
static Product? GetProduct(DataContext db, NLog.Logger logger)
{
    if (int.TryParse(Console.ReadLine(), out int ProductId))
    {
        Product product = db.Products.FirstOrDefault(p => p.ProductId == ProductId)!;

        if (product == null)
        {
            logger.Error($"ID {ProductId} does not exist.");
        }
        return product;
    }
    logger.Error("Invalid input. Please enter a valid Product ID.");
    return null;
}
//input a new product. used to add new product and edit a current product
static Product? InputProduct(DataContext db, NLog.Logger logger)
{
    Product product = new();
    product.ProductName = GetStringInput("Enter Product Name:");
    product.SupplierId = GetNumberInput("Enter Supplier ID:");
    product.CategoryId = GetNumberInput("Enter Category ID:");
    product.QuantityPerUnit = GetStringInput("Enter Quantity per Unit:");
    product.UnitPrice = GetDecimalInput("Enter Unit Price:");
    product.UnitsInStock = GetNumberInput("Enter Units in Stock:");
    product.UnitsOnOrder = GetNumberInput("Enter Units on Order:");
    product.ReorderLevel = GetNumberInput("Enter Reorder Level:");
    product.Discontinued = GetDiscontinue("Enter If Discontinued:");

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
            results.Add(new ValidationResult("Product Name exists", ["ProductName"]));
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

//Getting the category based on id
static Category? GetCategory(DataContext db, NLog.Logger logger)
{
    if (int.TryParse(Console.ReadLine(), out int CategoryId))
    {
        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId)!;

        if (category == null)
        {
            logger.Error($"ID {CategoryId} does not exist.");
        }
        return category;
    }
    logger.Error("Invalid input. Please enter a valid Category ID.");
    return null;
}
//input a new category
static Category? InputCategory(DataContext db, NLog.Logger logger)
{
    Category category = new();
    category.CategoryName = GetStringInput("Enter Category Name:");
    category.Description = GetStringInput("Enter Description:");

    ValidationContext context = new ValidationContext(category, null, null);
    List<ValidationResult> results = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(category, context, results, true);
    if (isValid)
    {
        if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
        {
            isValid = false;
            results.Add(new ValidationResult("Category Name exists", ["CategoryName"]));
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
    return category;
}
//getting user input string, short, decimal, discontinued
static string GetStringInput(string input)
{
    Console.WriteLine(input); return Console.ReadLine()!;
}

static short GetNumberInput(string input, bool validate = true)
{
    short number;
    while (true)
    {
        Console.WriteLine(input);
        if (short.TryParse(Console.ReadLine(), out number) && (!validate || number >= 0))
        { return number; }
        else { Console.WriteLine("Invalid input. Please enter a valid number."); }
    }
}

static decimal GetDecimalInput(string input)
{
    decimal number;
    while (true)
    {
        Console.WriteLine(input);
        if (decimal.TryParse(Console.ReadLine(), out number) && number >= 0)
        { return number; }
        else { Console.WriteLine("Invalid input. Please enter a valid decimal."); }
    }
}

static bool GetDiscontinue(string prompt)
{
    while (true)
    {
        Console.WriteLine(prompt);
        var input = Console.ReadLine()?.ToLower();
        if (input == "true") { return true; }
        else if (input == "false") { return false; }
        else { Console.WriteLine("Invalid input. Please enter 'true' or 'false'."); }
    }
}
