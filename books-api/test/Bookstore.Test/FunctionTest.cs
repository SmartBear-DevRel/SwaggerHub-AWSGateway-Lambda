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
            new Book(){ Id = Guid.Parse("d7bc78eb-d755-436f-978c-48af93dd0b28"), Title = "Designing APIs with Swagger and OpenAPI", Authors = new List<string>() {"Joshua S. Ponelat", "Lukas L. Rosenstock"}, Published = "2022-05-01"},
            new Book(){ Id = Guid.Parse("b9d803a9-43a4-427c-80e4-5f38048654d3"), Title = "The Lean Startup", Authors = new List<string>(){ "Eric Ries" }, Published = "2011-01-01"},
            new Book(){ Id = Guid.Parse("7aabd5b9-f87d-40b6-8d35-01cec823a4d1"), Title = "Building Microservices", Authors = new List<string>(){ "Sam Newman" }, Published = "2022-01-01"}             
        };

        return books;
    }

    private Book GetBook(Guid bookId)
    {
        return GetBooksFiltered(null, null).Where(b => b.Id == bookId).FirstOrDefault();
    }

    [Fact]
    public void TestGetBooksFunctionHandler()
    {
        var request = new APIGatewayProxyRequest
        {
            Headers = new Dictionary<string, string>()
        };
        request.Headers.TryAdd("X-Api-Key", "91e69fe1-fefd-4b2d-b689-032fb0947d10");

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
    [InlineData("d7bc78eb-d755-436f-978c-48af93dd0b28", 200, "")]
    [InlineData("b9d803a9-43a4-427c-80e4-5f38048654d3", 200, "")]
    [InlineData("7aabd5b9-f87d-40b6-8d35-01cec823a4d1", 200, "")]
    [InlineData("70d9eba9-803f-4b0c-a4c3-fcec6584c5a3", 404, "404 Not Found")]
    public void TestGetBookByIdFunctionHandler(string bookId, int statusCode, string errorBody)
    {
        var request = new APIGatewayProxyRequest
        {
            Headers = new Dictionary<string, string>()
        };
        request.Headers.TryAdd("X-Api-Key", "91e69fe1-fefd-4b2d-b689-032fb0947d10");
        
        var context = new TestLambdaContext();

        var serializationOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        request.PathParameters = new Dictionary<string, string>
        {
            { "id", bookId }
        };

        Guid.TryParse(bookId, out Guid id);
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
        var request = new APIGatewayHttpApiV2ProxyRequest
        {
            Headers = new Dictionary<string, string>()
        };
        request.Headers.TryAdd("X-Api-Key", "91e69fe1-fefd-4b2d-b689-032fb0947d10");        
        
        var context = new TestLambdaContext();

        var requestBody = new Order()
        {
            Books = new List<BookOrder>()
            {
                new BookOrder(){ BookId = Guid.Parse("d7bc78eb-d755-436f-978c-48af93dd0b28"), Quantity = 2},
                new BookOrder(){ BookId = Guid.Parse("b9d803a9-43a4-427c-80e4-5f38048654d3"), Quantity = 1}
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
            Id = Guid.Parse("fd6c319c-8d7d-45f0-ba39-9f90a7637af8"),
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

    [Fact]
    public void NoAPIKeyShouldReturnUnauthorized()
    {
        var request = new APIGatewayProxyRequest
        {
            Headers = new Dictionary<string, string>()
        };
        
        var context = new TestLambdaContext();

        var function = new Function();
        var response = function.GetBooks(request, context);

        Assert.Equal(401, response.StatusCode);
    } 

  }
}