using System.Net;
using System.Text;
using System.Text.Json;

namespace Bookstore.IntegrationTest;


public class IntegrationTests : IClassFixture<Setup>
{
    private HttpClient _httpClient = new HttpClient();

    [Fact]
    public async void GetBooks_ShouldReturnOk()
    {

        var getBooksResponse = await this._httpClient.GetAsync($"{Setup.ApiUrl}");

        Assert.Equal(HttpStatusCode.OK, getBooksResponse.StatusCode);
    }
}