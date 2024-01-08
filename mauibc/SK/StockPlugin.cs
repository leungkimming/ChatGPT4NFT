using Azure.AI.OpenAI;
using System.Text.Json;
using System;
using System.ComponentModel;
using Microsoft.SemanticKernel;
using System.Diagnostics;

/// <summary>
/// StockPlugin provides a set of functions to get the quantity in stock of an item and place order for customers.
/// </summary>
/// <example>
/// Usage: kernel.ImportFunctions(new StockPlugin(), "Stock");
/// Examples:
/// {{Stock.CheckStock 'table'}}                                        => "found 11 tables in stock"
/// {{Stock.PlaceOrder 'table', 11, 'KM4585'}}                          => "Success: Order placed"
/// {{Stock.SendInstantMessage 'We have 5 iPad in stock', 'Michael'}}   => "Instant Message sent successfully"
/// </example>
/// <remark>
/// </remark>

namespace ChatMAUI;
public sealed class StockPlugin {
    /// <summary>
    /// Customer place order to purchase a certain amount of an item
    /// </summary>
    /// <example>
    /// {{Stock.PlaceOrder 'table', 5, 'KM4585'}}   => "Success: Order placed"
    /// {{Stock.PlaceOrder 't-shirt', 5, 'KM4585'}}   => "Failed: We only have 3 t-shirts in stock."
    /// {{Stock.PlaceOrder 'chair', 5, 'KM4585'}}   => "Failed: Item not found in stock!"
    /// {{Stock.PlaceOrder 'iPad', 5, 'KM4585'}}   => "Failed: CustomerID does not exist!"
    /// {{Stock.PlaceOrder 'dog', 1, 'KM4585'}}   => "Failed: We don't carry dogs."
    /// </example>
    /// <param name="item"> The item to place order, e.g. table, t-shirt, iPad, chair </param>
    /// <param name="quantity"> The quantity required. </param>
    /// <param name="customerId"> The ID of the customer placing the order. </param>
    /// <returns> Result of placing the order, either Success or Failed. </returns>
    [KernelFunction, Description("Customer place order to purchase a certain amount of an item")]
    static public string PlaceOrder(
        [Description("The item to place order")] string? item,
        [Description("The quantity required")] int? quantity,
        [Description("The stock item to check quanity")] string? customerId) {
        string[] strings = { "Success: Order placed", "Failed: CustomerID does not exist!", "Failed: Item not found in stock!" };
        //Random random = new Random();
        //int randomIndex = random.Next(0, strings.Length);
        //string reply = strings[randomIndex];

        string reply = ValidateOrder(item, quantity, customerId);
		Debug.WriteLine($"******************** Local API validating and placing order:\nCustomer:{customerId}\nItem:{item}\nQuantity:{quantity}\nResult:{reply}");
        return reply;
    }
    static private string ValidateOrder(string? item, int? quantity, string? customerId) {
        if (customerId.ToUpper().Contains("ERR"))
        {
            return "Failed: CustomerID does not exist!";
        }
        string checkstock = CheckStock(item);
        if (checkstock.Contains("sorry"))
        {
            return $"Failed: We don't carry {item}.";
        }
        if (Int16.Parse(checkstock) < quantity)
        {
            return $"Failed: We only have {checkstock} {item} in stock.";
        }
        return "Success: Order placed";
    }

    /// <summary>
    /// Send an Instant Message containing a message to a recipient
    /// </summary>
    /// <example>
    /// {{Stock.SendInstantMessage 'We have 5 iPad in stock', 'Michael'}}   => "Instant Message sent successfully"
    /// {{Stock.SendInstantMessage 'Success: Order placed', 'Mr. Chan'}}   => "Instant Message sent successfully"
    /// </example>
    /// <param name="message"> The message to be delivered to the recipient </param>
    /// <param name="recipient"> The name of the recipient </param>
    /// <returns> "Instant Message sent successfully" </returns>
    [KernelFunction, Description("Send an Instant Message containing a message to a recipient")]
    static public string SendInstantMessage(
        [Description("The message to be delivered to the recipient")] string? message,
        [Description("The name of the recipient")] string? recipient) {
		Debug.WriteLine($"******************** Local API sending IM to {recipient}@WhatsApp.com - \"{message}\"");
        return $"Instant Message sent successfully";
    }

    /// <summary>
    /// Get the quantity on hand of an item
    /// </summary>
    /// <example>
    /// {{Stock.CheckStock 'table'}}   => "found 11 tables in stock"
    /// {{Stock.CheckStock 't-shirt'}} => "found 11 t-shirts in stock"
    /// {{Stock.CheckStock 'iPad'}}    => "found 11 iPads in stock"
    /// {{Stock.CheckStock 'chair'}}   => "found 11 chairs in stock"
    /// {{Stock.CheckStock 'dog'}}     => "sorry, we don't carry dogs"
    /// </example>
    /// <param name="item"> The item, e.g. table, t-shirt, iPad, chair </param>
    /// <returns> The item's quantity in stock </returns>
    [KernelFunction, Description("Get the quantity on hand of an item")]
    static public string CheckStock([Description("The item, e.g. table, t-shirt, iPad, chair")] string? item) {
        Random random = new Random();
        string found = random.Next(1, 201).ToString();
        string reply = $"found {found} {item} in stock";
        if (item.ToUpper().Contains("DOG")) {
            reply = $"sorry, we don't carry {item}";
            found = reply;
        }
        Debug.WriteLine($"******************** Local API running, {reply}");
        return found;
    }
}
