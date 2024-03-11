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

        private OrderDetails GetOrderDetails(Guid orderId)
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
                    new BookOrder(){ BookId = Guid.Parse("b9d803a9-43a4-427c-80e4-5f38048654d3"), Quantity = 2}
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
                new Book(){ Id = Guid.Parse("d7bc78eb-d755-436f-978c-48af93dd0b28"), Title = "Designing APIs with Swagger and OpenAPI", Authors = new List<string>() {"Joshua S. Ponelat", "Lukas L. Rosenstock"}, Published = "2022-05-01"},
                new Book(){ Id = Guid.Parse("b9d803a9-43a4-427c-80e4-5f38048654d3"), Title = "The Lean Startup", Authors = new List<string>(){ "Eric Ries" }, Published = "2011-01-01"},
                new Book(){ Id = Guid.Parse("7aabd5b9-f87d-40b6-8d35-01cec823a4d1"), Title = "Building Microservices", Authors = new List<string>(){ "Sam Newman" }, Published = "2022-01-01"}                
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

        private static Book GetBook(Guid bookId)
        {
            return GetBooksFiltered(null, null).Where(b => b.Id == bookId).FirstOrDefault();
        }

        public APIGatewayProxyResponse GetBooks(APIGatewayProxyRequest input, ILambdaContext context)
        {
            //check X-Api-Key header
            var apiKey = "";
            input.Headers?.TryGetValue("X-Api-Key", out apiKey);

            if(string.IsNullOrEmpty(apiKey) || !isValidKey(apiKey))
            {
                return new APIGatewayProxyResponse
                {
                    Body = $"Unauthorized - Invalid or missing API Key",
                    StatusCode = 401,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            var title = "";
            input.QueryStringParameters?.TryGetValue("title", out title);

            var author = "";
            input.QueryStringParameters?.TryGetValue("author", out author);

            var limit = "";
            input.QueryStringParameters?.TryGetValue("limit", out limit);

            //ToDo - validate title and author
            if(!string.IsNullOrEmpty(limit))
            {
                if(!int.TryParse(limit, out int result))
                {
                    return new APIGatewayProxyResponse
                    {
                        Body = "400 Bad Request - Limit must be a number.",
                        StatusCode = 400,
                        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                    };
                }

                if(result < 1 || result > 1000)
                {
                    return new APIGatewayProxyResponse
                    {
                        Body = "400 Bad Request - Limit must be between 1 and 1000.",
                        StatusCode = 400,
                        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                    };
                }
            }

            if(!string.IsNullOrEmpty(title) && title.Length > 200)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "400 Bad Request - Title must be at less than 200 characters.",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            if(!string.IsNullOrEmpty(author) && author.Length > 150)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "400 Bad Request - Author must be at less than 150 characters.",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

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
            //check X-Api-Key header
            var apiKey = "";
            input.Headers?.TryGetValue("X-Api-Key", out apiKey);

            if(string.IsNullOrEmpty(apiKey) || !isValidKey(apiKey))
            {
                return new APIGatewayProxyResponse
                {
                    Body = "Unauthorized - Invalid or missing API Key.",
                    StatusCode = 401,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }
            
            var bookId = "";
            input.PathParameters?.TryGetValue("id", out bookId);

            Guid providedBookId;
            Guid.TryParse(bookId, out providedBookId);

            if(providedBookId == Guid.Empty)
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
            //check X-Api-Key header
            var apiKey = "";
            input.Headers?.TryGetValue("X-Api-Key", out apiKey);

            if(string.IsNullOrEmpty(apiKey) || !isValidKey(apiKey))
            {
                return new APIGatewayProxyResponse
                {
                    Body = "Unauthorized - Invalid or missing API Key.",
                    StatusCode = 401,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

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
            //check X-Api-Key header
            var apiKey = "";
            input.Headers?.TryGetValue("X-Api-Key", out apiKey);

            if(string.IsNullOrEmpty(apiKey) || !isValidKey(apiKey))
            {
                return new APIGatewayProxyResponse
                {
                    Body = "Unauthorized - Invalid or missing API Key.",
                    StatusCode = 401,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            var orderId = "";
            input.PathParameters?.TryGetValue("orderId", out orderId);

            Guid.TryParse(orderId, out Guid providedOrderId);

            if (providedOrderId == Guid.Empty)
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
            //check X-Api-Key header
            var apiKey = "";
            input.Headers?.TryGetValue("X-Api-Key", out apiKey);

            if(string.IsNullOrEmpty(apiKey) || !isValidKey(apiKey))
            {
                return new APIGatewayProxyResponse
                {
                    Body = "Unauthorized - Invalid or missing API Key.",
                    StatusCode = 401,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            var orderId = "";
            input.PathParameters?.TryGetValue("orderId", out orderId);

            Guid.TryParse(orderId, out Guid providedOrderId);

            if(providedOrderId == Guid.Empty)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "400 Bad Request PUT - Order Id not provided or invalid.",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            var orderToUpdate = JsonSerializer.Deserialize<Order>(input.Body);
            var currentOrder = GetCurrentOrderInfo(orderId);

            if(orderToUpdate == null)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "400 Bad Request",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }            

            var updatedOrder = new OrderDetails()
            {
                Id = currentOrder.Id,
                Books = orderToUpdate.Books,
                Status = currentOrder.Status,
                DeliveryAddress = orderToUpdate.DeliveryAddress,
                CreatedAt = currentOrder.CreatedAt,
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };            

            // update the order in in memory cache
            cache.Set(providedOrderId, updatedOrder, TimeSpan.FromSeconds(300));

            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(updatedOrder, serializerOptions),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        private Guid GetNextOrderId()
        {
            var nextId = Guid.Parse("fd6c319c-8d7d-45f0-ba39-9f90a7637af8");

            if(cache.TryGetValue("latestOrderId", out var value))
            {
                return Guid.NewGuid();
            }

            return nextId;
        }

        private bool isValidKey(string apiKey)
        {
            // super silly validation just for demo purposes.........not a real validation function

            if(string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            if(apiKey != "91e69fe1-fefd-4b2d-b689-032fb0947d10")
            {
                return false;
            }

            return true;
        }

        private OrderDetails GetCurrentOrderInfo(string orderId)
        {
            if(cache.TryGetValue(orderId, out var orderDetails))
            {
                return (OrderDetails)orderDetails;
            }

            return new OrderDetails()
            {
                Id = Guid.Parse(orderId),
                Status = "placed",
                DeliveryAddress = "SmartBear, Mayoralty House, Flood Street Galway, H91 P8PR, Ireland",
                CreatedAt = DateTime.UtcNow.AddSeconds(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                UpdatedAt = DateTime.UtcNow.AddSeconds(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
    }
}
