using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

namespace Bookstore.Tests
{
  public class FunctionTest
  {
    private static readonly HttpClient client = new HttpClient();

    private static List<Book> GetBooksFiltered(string title, string author)
    {
        var books = new List<Book>()
        {
            new Book(){ Id = 1001, Title = "Designing APIs with Swagger and OpenAPI", Authors = new List<string>() {"Joshua S. Ponelat", "Lukas L. Rosenstock"}, Published = "2022-05-01"},
            new Book(){ Id = 1002, Title = "The Lean Startup", Authors = new List<string>(){ "Eric Ries" }, Published = "2011-01-01"},
            new Book(){ Id = 1003, Title = "Building Microservices", Authors = new List<string>(){ "Sam Newman" }, Published = "2022-01-01"}                
        };

        return books;
    }

    private static Book GetBook(int bookId)
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
            id = 10001,
            Books = requestBody.Books,
            Status = Status.Placed,
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
  }
}