using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Bookstore
{

    public class Function
    {

        private readonly HttpClient client;
        private readonly MemoryCache cache;
        private readonly JsonSerializerOptions serializerOptions;

        public Function()
        {
            client = new HttpClient();
            cache = new MemoryCache(new MemoryCacheOptions());
            serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public Function(HttpClient httpClient, MemoryCache memoryCache, JsonSerializerOptions jsonSerializerOptions)
        {
            client = httpClient;
            cache = memoryCache;
            serializerOptions = jsonSerializerOptions;
        }

        private OrderDetails GetOrderDetails(int orderId)
        {
            if(cache.TryGetValue(orderId, out var orderDetails))
            {
                return (OrderDetails)orderDetails;
            }

            return new OrderDetails()
            {
                Id = orderId,
                Books = new List<BookOrder>()
                {
                    new BookOrder(){ BookId = 1002, Quantity = 2}
                },
                Status = "placed",
                DeliveryAddress = "SmartBear, Mayoralty House, Flood Street Galway, H91 P8PR, Ireland",
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
        private static List<Book> GetBooksFiltered(string title, string author)
        {
            var books = new List<Book>()
            {
                new Book(){ Id = 1001, Title = "Designing APIs with Swagger and OpenAPI", Authors = new List<string>() {"Joshua S. Ponelat", "Lukas L. Rosenstock"}, Published = "2022-05-01"},
                new Book(){ Id = 1002, Title = "The Lean Startup", Authors = new List<string>(){ "Eric Ries" }, Published = "2011-01-01"},
                new Book(){ Id = 1003, Title = "Building Microservices", Authors = new List<string>(){ "Sam Newman" }, Published = "2022-01-01"}                
            };

            // filter by author and title
            if(!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(author))
            {
                return books.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase) 
                    && b.Authors.Any(a => a.Contains(author, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            // filter by title
            if(!string.IsNullOrEmpty(title))
            {
                return books.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // filter by author
            if(!string.IsNullOrEmpty(author))
            {
                return books.Where(b => b.Authors.Any(a => a.Contains(author, StringComparison.OrdinalIgnoreCase))).ToList();
                
            }

            return books;
        }

        private static Book GetBook(int bookId)
        {
            return GetBooksFiltered(null, null).Where(b => b.Id == bookId).FirstOrDefault();
        }

        public APIGatewayProxyResponse GetBooks(APIGatewayProxyRequest input, ILambdaContext context)
        {
           
            var title = "";
            input.QueryStringParameters?.TryGetValue("title", out title);

            var author = "";
            input.QueryStringParameters?.TryGetValue("author", out author);

            var books = GetBooksFiltered(title, author);

            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(books, serializerOptions),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        public APIGatewayProxyResponse GetBookById(APIGatewayProxyRequest input, ILambdaContext context)
        {
            var bookId = "";
            input.PathParameters?.TryGetValue("id", out bookId);

            int providedBookId;
            int.TryParse(bookId, out providedBookId);

            if(providedBookId == 0)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "400 Bad Request - Book Id not provided or invalid.",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            var book = GetBook(providedBookId);

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

        public APIGatewayProxyResponse CreateOrder(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
        {
            var order = JsonSerializer.Deserialize<Order>(input.Body);
            Console.WriteLine("Order: " + order);

            if(order == null)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "400 Bad Request. Order null or empty.",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            //create a new orderId
            var newOrderId = GetNextOrderId();

            var orderDetails = new OrderDetails()
            {
                Id = newOrderId,
                Books = order.Books,
                Status = "placed",
                DeliveryAddress = order.DeliveryAddress,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            // add the order to in memory cache
            cache.Set(newOrderId, orderDetails, TimeSpan.FromSeconds(300));
            cache.Set("latestOrderId", newOrderId, TimeSpan.FromSeconds(300));

            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(orderDetails, serializerOptions),
                StatusCode = 201,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        public APIGatewayProxyResponse GetOrderById(APIGatewayProxyRequest input, ILambdaContext context)
        {
            var orderId = "";
            input.PathParameters?.TryGetValue("orderId", out orderId);

            int.TryParse(orderId, out int providedOrderId);

            if (providedOrderId == 0)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "404 Not Found GET Rq - Order Id not provided or invalid.",
                    StatusCode = 404,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            // get the order from cache or default
            var orderDetails = GetOrderDetails(providedOrderId);

            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(orderDetails, serializerOptions),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        public APIGatewayProxyResponse UpdateOrderById(APIGatewayProxyRequest input, ILambdaContext context)
        {
            var orderId = "";
            input.PathParameters?.TryGetValue("orderId", out orderId);

            int providedOrderId;
            int.TryParse(orderId, out providedOrderId);

            if(providedOrderId == 0)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "400 Bad Request PUT - Order Id not provided or invalid.",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            var orderDetails = JsonSerializer.Deserialize<OrderDetails>(input.Body);

            if(orderDetails == null)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "400 Bad Request",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            // update the order in in memory cache
            cache.Set(providedOrderId, orderDetails, TimeSpan.FromSeconds(300));

            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(orderDetails, serializerOptions),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        private int GetNextOrderId()
        {
            var nextId = 10001;

            if(cache.TryGetValue("latestOrderId", out var value))
            {
                nextId = (int)value + 1;
            }

            return nextId;
        }
    }
}
