using System;
using System.Net;
using RestSharp;

namespace M11.Services
{
    public class TicketService
    {
        private readonly string _ticketPageUrl;
        private readonly CookieContainer _cookieContainer;

        public TicketService(string ticketPageUrl, CookieContainer cookieContainer)
        {
            _ticketPageUrl = ticketPageUrl;
            _cookieContainer = cookieContainer;
        }

        public void Start()
        {
            try
            {
                var client = new RestClient(_ticketPageUrl) { CookieContainer = _cookieContainer };
                var request = new RestRequest(Method.POST);
                var response = client.Execute(request);
                var stringContent = response.Content;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
