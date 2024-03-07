using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;


namespace Bookstore.Tests
{
  public class FunctionTest
  {
    private readonly HttpClient httpClient = new HttpClient();
    private readonly MemoryCache cache = new MemoryCache(new MemoryCacheOptions());
    private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private List<Book> GetBooksFiltered(string title, string author)
    {
        var books = new List<Book>()
        {
            new Book(){ Id = 1001, Title = "Designing APIs with Swagger and OpenAPI", Authors = new List<string>() {"Joshua S. Ponelat", "Lukas L. Rosenstock"}, Published = "2022-05-01"},
            new Book(){ Id = 1002, Title = "The Lean Startup", Authors = new List<string>(){ "Eric Ries" }, Published = "2011-01-01"},
            new Book(){ Id = 1003, Title = "Building Microservices", Authors = new List<string>(){ "Sam Newman" }, Published = "2022-01-01"}                
        };

        return books;
    }

    private Book GetBook(int bookId)
    {
        return GetBooksFiltered(null, null).Where(b => b.Id == bookId).FirstOrDefault();
    }

    [Fact]
    public void TestGetBooksFunctionHandler()
    {
        var request = new APIGatewayProxyRequest();
        var context = new TestLambdaContext();

        var serializationOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var books = GetBooksFiltered(null, null);

        var expectedResponse = new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(books),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        var function = new Function();
        var response = function.GetBooks(request, context);

        Console.WriteLine("Lambda Response: \n" + response.Body);
        Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

        Assert.Equal(expectedResponse.Body, response.Body);
        Assert.Equal(expectedResponse.Headers, response.Headers);
        Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData("1001", 200, "")]
    [InlineData("1002", 200, "")]
    [InlineData("1003", 200, "")]
    [InlineData("1", 404, "404 Not Found")]
    public void TestGetBookByIdFunctionHandler(string bookId, int statusCode, string errorBody)
    {
      var request = new APIGatewayProxyRequest();
      var context = new TestLambdaContext();

      var serializationOptions = new JsonSerializerOptions
      {
          PropertyNameCaseInsensitive = true,
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };
     
      request.PathParameters = new Dictionary<string, string>();
      request.PathParameters.Add("id", bookId);

      int.TryParse(bookId, out int id);
      var book = GetBook(id);
      var expectedResponse = new APIGatewayProxyResponse();

      if(book == null)
      {
        expectedResponse = new APIGatewayProxyResponse
        {
            Body = errorBody,
            StatusCode = statusCode,
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };               
      }
      else 
      {
        expectedResponse = new APIGatewayProxyResponse
        {
          Body = JsonSerializer.Serialize(book),
          StatusCode = statusCode,
          Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
      }

      var function = new Function();
      var response = function.GetBookById(request, context);

      Console.WriteLine("Lambda Response: \n" + response.Body);
      Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

      Assert.Equal(expectedResponse.Body, response.Body);
      Assert.Equal(expectedResponse.Headers, response.Headers);
      Assert.Equal(expectedResponse.StatusCode, response.StatusCode);

    }

    [Fact]
    public void TestCreateOrderFunctionHandler()
    {
        var request = new APIGatewayHttpApiV2ProxyRequest();
        var context = new TestLambdaContext();

        var requestBody = new Order()
        {
            Books = new List<BookOrder>()
            {
                new BookOrder(){ BookId = 1001, Quantity = 2},
                new BookOrder(){ BookId = 1002, Quantity = 1}
            },
            DeliveryAddress = "123 Main St, Seattle, WA 98101"
        };

        var serializationOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        request.Body = JsonSerializer.Serialize(requestBody, serializationOptions);

        var placedOrder = new OrderDetails()
        {
            Id = 10001,
            Books = requestBody.Books,
            Status = "placed",
            DeliveryAddress = requestBody.DeliveryAddress,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        var expectedResponse = new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(placedOrder, serializationOptions),
            StatusCode = 201,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        var function = new Function();
        var response = function.CreateOrder(request, context);

        Console.WriteLine("Lambda Response: \n" + response.Body);
        Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

        Assert.Equal(expectedResponse.Body, response.Body);
        Assert.Equal(expectedResponse.Headers, response.Headers);
        Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }    

    /* [Fact]
    public void TestCreateAndRetrieveOrder()
    {
      var request = new APIGatewayHttpApiV2ProxyRequest();
      var context = new TestLambdaContext();

      var requestBody = new Order()
      {
          Books = new List<BookOrder>()
          {
              new BookOrder(){ BookId = 1001, Quantity = 2},
              new BookOrder(){ BookId = 1002, Quantity = 1}
          },
          DeliveryAddress = "123 Main St, Seattle, WA 98101"
      };

      var serializationOptions = new JsonSerializerOptions
      {
          PropertyNameCaseInsensitive = true,
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };

      request.Body = JsonSerializer.Serialize(requestBody, serializationOptions);

      var function = new Function(httpClient, cache, serializerOptions);
      function.CreateOrder(request, context);
      var responseCreateOrder = function.CreateOrder(request, context);

      var expectedPlacedOrder = new OrderDetails()
      {
          Id = 10002,
          Books = requestBody.Books,
          Status = "placed",
          DeliveryAddress = requestBody.DeliveryAddress,
          CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
          UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
      };

      var expectedResponse = new APIGatewayProxyResponse
      {
          Body = JsonSerializer.Serialize(expectedPlacedOrder, serializationOptions),
          StatusCode = 200,
          Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };

      var responseCreateOrderDetails = JsonSerializer.Deserialize<OrderDetails>(responseCreateOrder.Body, serializationOptions);
      var responseGetOrder = function.GetOrderById(new APIGatewayProxyRequest() { PathParameters = new Dictionary<string, string>() { { "orderId", responseCreateOrderDetails.Id.ToString() } } }, context);
      
      Console.WriteLine("Lambda Response: \n" + responseGetOrder.Body);
      Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

      Assert.Equal(expectedResponse.Body, responseGetOrder.Body);
      Assert.Equal(expectedResponse.Headers, responseGetOrder.Headers);
      Assert.Equal(expectedResponse.StatusCode, responseGetOrder.StatusCode);        
    }
 */
  }
}