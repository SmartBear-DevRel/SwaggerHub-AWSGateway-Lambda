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
            new Book(){ Id = "be05885a-41fc-4820-83fb-5db17015ed4a", Title = "Designing APIs with Swagger and OpenAPI", Authors = new List<string>() {"Joshua S. Ponelat", "Lukas L. Rosenstock"}, Published = "2022-05-01"},
            new Book(){ Id = "dd3424c9-17ec-4b20-a89c-ca89d98bbd3b", Title = "The Lean Startup", Authors = new List<string>(){ "Eric Ries" }, Published = "2011-01-01"},
            new Book(){ Id = "bf936dc3-6c70-43a0-a4c5-ddb42569a9c8", Title = "Building Microservices", Authors = new List<string>(){ "Sam Newman" }, Published = "2022-01-01"}                
        };

        return books;
    }

    private static Book GetBook(string bookId)
    {
        return GetBooksFiltered(null, null).Where(b => b.Id.Equals(bookId, System.StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
    }

    [Fact]
    public async Task TestGetBooksFunctionHandler()
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
        var response = await function.GetBooks(request, context);

        Console.WriteLine("Lambda Response: \n" + response.Body);
        Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

        Assert.Equal(expectedResponse.Body, response.Body);
        Assert.Equal(expectedResponse.Headers, response.Headers);
        Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData("be05885a-41fc-4820-83fb-5db17015ed4a", 200, "")]
    [InlineData("dd3424c9-17ec-4b20-a89c-ca89d98bbd3b", 200, "")]
    [InlineData("bf936dc3-6c70-43a0-a4c5-ddb42569a9c8", 200, "")]
    [InlineData("a fictional book id", 404, "404 Not Found")]
    public async Task TestGetBookByIdFunctionHandler(string bookId, int statusCode, string errorBody)
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
      
      var book = GetBook(bookId);
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
      var response = await function.GetBookById(request, context);

      Console.WriteLine("Lambda Response: \n" + response.Body);
      Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

      Assert.Equal(expectedResponse.Body, response.Body);
      Assert.Equal(expectedResponse.Headers, response.Headers);
      Assert.Equal(expectedResponse.StatusCode, response.StatusCode);

    }
  }
}