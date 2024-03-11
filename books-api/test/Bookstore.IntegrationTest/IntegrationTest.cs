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
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", "91e69fe1-fefd-4b2d-b689-032fb0947d10");
        var getBooksResponse = await this._httpClient.GetAsync($"{Setup.ApiUrl}/books");

        Assert.Equal(HttpStatusCode.OK, getBooksResponse.StatusCode);
    }
}