using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Bookstore
{

    public class Function
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

            // filter by author and title
            if(!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(author))
            {
                return books.Where(b => b.Title.Contains(title, System.StringComparison.OrdinalIgnoreCase) 
                    && b.Authors.Any(a => a.Contains(author, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            // filter by title
            if(!string.IsNullOrEmpty(title))
            {
                return books.Where(b => b.Title.Contains(title, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // filter by author
            if(!string.IsNullOrEmpty(author))
            {
                return books.Where(b => b.Authors.Any(a => a.Contains(author, StringComparison.OrdinalIgnoreCase))).ToList();
                
            }

            return books;
        }

        private static Book GetBook(string bookId)
        {
            return GetBooksFiltered(null, null).Where(b => b.Id.Equals(bookId, System.StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public async Task<APIGatewayProxyResponse> GetBooks(APIGatewayProxyRequest input, ILambdaContext context)
        {

            var serializationOptions = new JsonSerializerOptions
            {
                //PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            
            var title = "";
            input.QueryStringParameters?.TryGetValue("title", out title);

            var author = "";
            input.QueryStringParameters?.TryGetValue("author", out author);

            var books = GetBooksFiltered(title, author);

            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(books),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        public async Task<APIGatewayProxyResponse> GetBookById(APIGatewayProxyRequest input, ILambdaContext context)
        {
            var bookId = "";
            input.PathParameters?.TryGetValue("id", out bookId);

            if(string.IsNullOrEmpty(bookId))
            {
                return new APIGatewayProxyResponse
                {
                    Body = "400 Bad Request",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            var book = GetBook(bookId);

            if(book == null)
            {
                 return new APIGatewayProxyResponse
                {
                    Body = "404 Not Found",
                    StatusCode = 404,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };               
            }

            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(book),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
